using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WebEmpresa.Components;

var builder = WebApplication.CreateBuilder(args);

// Servicios interactivos de Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Cliente HTTP conectado directamente al puerto HTTP de ApiEmpresa
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5014/")
});

var app = builder.Build();

// [ZAP] Content Security Policy estricta (sin comodines, sin data:, sin 'unsafe-inline').
// El unico script en linea de Blazor (<script type="importmap">) se autoriza con su hash SHA-256.
var importMapRegex = new Regex(
    @"<script[^>]*type\s*=\s*""importmap""[^>]*>(.*?)</script>",
    RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

static string ConstruirCsp(string? hashImportMap)
{
    var scriptSrc = hashImportMap is null ? "'self'" : $"'self' 'sha256-{hashImportMap}'";
    return "default-src 'self'; " +
           $"script-src {scriptSrc}; " +
           "style-src 'self'; " +
           "img-src 'self'; " +
           "font-src 'self'; " +
           "connect-src 'self'; " +
           "object-src 'none'; " +
           "base-uri 'self'; " +
           "form-action 'self'; " +
           "frame-ancestors 'none'";
}

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var h = context.Response.Headers;

        // [ZAP] Falta encabezado X-Content-Type-Options
        h["X-Content-Type-Options"] = "nosniff";

        // Blazor agrega por defecto "Content-Security-Policy: frame-ancestors 'self'" (incompleta);
        // se reemplaza por la CSP completa en cualquier respuesta que no la haya fijado ya con hash
        if (!context.Items.ContainsKey("csp-establecida"))
        {
            h["Content-Security-Policy"] = ConstruirCsp(null);
        }

        return Task.CompletedTask;
    });

    var ruta = context.Request.Path;
    var esPagina = HttpMethods.IsGet(context.Request.Method)
                   && !ruta.StartsWithSegments("/_blazor")
                   && !ruta.StartsWithSegments("/_framework")
                   && (!Path.HasExtension(ruta.Value)
                       || context.Request.Headers.Accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase));

    if (!esPagina)
    {
        await next();
        return;
    }

    // Paginas HTML: se calcula el hash del importmap antes de enviar la respuesta
    var cuerpoOriginal = context.Response.Body;
    await using var buffer = new MemoryStream();
    context.Response.Body = buffer;

    try
    {
        await next();
    }
    finally
    {
        context.Response.Body = cuerpoOriginal;
    }

    if (!context.Response.HasStarted)
    {
        var esHtml = context.Response.ContentType?
            .StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true;

        if (esHtml)
        {
            var html = Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
            var coincidencia = importMapRegex.Match(html);
            string? hash = null;

            if (coincidencia.Success)
            {
                // El navegador normaliza los saltos de linea a LF antes de calcular el hash
                var contenido = coincidencia.Groups[1].Value.Replace("\r\n", "\n").Replace("\r", "\n");
                hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(contenido)));
            }

            context.Response.Headers["Content-Security-Policy"] = ConstruirCsp(hash);
            context.Items["csp-establecida"] = true;
        }

        if (buffer.Length > 0)
        {
            context.Response.ContentLength = buffer.Length;
        }
    }

    buffer.Position = 0;
    await buffer.CopyToAsync(cuerpoOriginal, context.RequestAborted);
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();