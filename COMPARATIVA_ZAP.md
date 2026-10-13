# 6. Comparar los resultados (ZAP)

Sitio analizado: `http://localhost:5214` (WebEmpresa) · Herramienta: ZAP 2.17.0

## Tabla comparativa de resultados

| Alerta encontrada | Riesgo | Corrección realizada | Resultado final |
|---|---|---|---|
| CSP: Directiva Wildcard | Medio | Se definió una CSP estricta en `Program.cs` sin comodines ni `data:`. Los iconos `data:` pasaron a archivos `.svg` en `wwwroot/img` | Corregida |
| CSP: Failure to Define Directive with No Fallback | Medio | La CSP ahora define `frame-ancestors`, `form-action` y `base-uri`, y reemplaza la cabecera incompleta que Blazor agrega por defecto | Corregida |
| CSP: script-src unsafe-inline | Medio | Se quitó `'unsafe-inline'`. El `importmap` de Blazor se autoriza con hash SHA-256 y el `onclick` de `NavMenu.razor` pasó a `wwwroot/nav.js` | Corregida |
| CSP: style-src unsafe-inline | Medio | Se quitó `'unsafe-inline'`. Los `style="..."` de `Cliente.razor` pasaron a clases CSS en `app.css` | Corregida |
| Falta encabezado X-Content-Type-Options | Bajo | Se agregó `X-Content-Type-Options: nosniff` a todas las respuestas en `Program.cs` | Corregida |
| Divulgación de Marcas de Tiempo - Unix | Bajo | No requiere modificación: ZAP detecta un número de 10 dígitos dentro de un hash o identificador interno de Blazor, no una marca de tiempo real | Falso positivo |
| Divulgación de información - Comentarios sospechosos | Informativo | Se eliminaron los comentarios de `ReconnectModal.razor.js` | Corregida |
| Aplicación Web Moderna | Informativo | No requiere modificación | Informativa |
| Respuesta de Gestión de Sesión Identificada | Informativo | No requiere modificación: corresponde a la cookie antiforgery de Blazor | Informativa |

## Resumen antes y después

| Riesgo | Antes | Después |
|---|---|---|
| Alto | 0 | 0 |
| Medio | 4 | 0 |
| Bajo | 2 | 1 (falso positivo) |
| Informativo | 3 | 2 |
| **Total** | **9** | **3** |

## Vulnerabilidades que permanecen y motivo

- **Divulgación de Marcas de Tiempo - Unix (falso positivo):** el valor detectado forma parte de un hash de Blazor, no es una fecha ni expone información sensible. Su confianza en ZAP es Baja.
- **Aplicación Web Moderna (informativa):** solo indica que la aplicación usa JavaScript moderno; no es una vulnerabilidad.
- **Respuesta de Gestión de Sesión Identificada (informativa):** ZAP identifica la cookie antiforgery que Blazor genera de forma normal.

## Nota sobre alertas de otros sitios

Un informe intermedio incluyó alertas de `https://edge-consumer-static.azureedge.net` (Configuración Incorrecta Cross-Domain, HSTS, `X-Content-Type-Options` y Control de Caché). Esas peticiones las generó el navegador Microsoft Edge por su cuenta, no pertenecen a esta aplicación y se excluyeron del análisis.

## Archivos modificados

`Program.cs`, `Components/App.razor`, `Components/Layout/NavMenu.razor`, `Components/Layout/NavMenu.razor.css`, `Components/Layout/ReconnectModal.razor.js`, `Components/Pages/Cliente.razor`, `wwwroot/app.css`.
Archivos nuevos: `wwwroot/nav.js` y `wwwroot/img/*.svg`.
