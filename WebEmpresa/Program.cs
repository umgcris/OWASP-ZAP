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