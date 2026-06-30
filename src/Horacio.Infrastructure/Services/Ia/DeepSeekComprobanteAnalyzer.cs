using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Comprobantes.DTOs;
using Horacio.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Horacio.Infrastructure.Services.Ia;

/// <summary>
/// Estructura el comprobante usando DeepSeek (API compatible con OpenAI).
/// Recibe el TEXTO del comprobante (extraído por OCR) y devuelve el JSON parseado.
/// </summary>
public class DeepSeekComprobanteAnalyzer : IComprobanteAnalyzer
{
    private readonly HttpClient _http;
    private readonly DeepSeekSettings _settings;
    private readonly ILogger<DeepSeekComprobanteAnalyzer> _logger;

    private static readonly JsonSerializerOptions ParseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public DeepSeekComprobanteAnalyzer(HttpClient http, IOptions<DeepSeekSettings> options, ILogger<DeepSeekComprobanteAnalyzer> logger)
    {
        _http = http;
        _settings = options.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public async Task<AnalisisComprobanteDto> AnalizarTextoAsync(string textoOcr, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("Falta configurar la API key de DeepSeek (DeepSeek:ApiKey).");

        var payload = new
        {
            model = _settings.Model,
            temperature = 0,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = $"TEXTO DEL COMPROBANTE (extraído por OCR):\n\n{textoOcr}" }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        using var resp = await _http.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("DeepSeek devolvió {Status}: {Body}", (int)resp.StatusCode, body);
            throw new InvalidOperationException($"Error de la IA (DeepSeek): {(int)resp.StatusCode}.");
        }

        string contenido;
        using (var doc = JsonDocument.Parse(body))
        {
            contenido = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        var json = LimpiarJson(contenido);
        AnalisisComprobanteDto? analisis;
        try
        {
            analisis = JsonSerializer.Deserialize<AnalisisComprobanteDto>(json, ParseOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "No se pudo parsear el JSON de DeepSeek: {Json}", json);
            throw new InvalidOperationException("La IA devolvió una respuesta no válida. Intente nuevamente.");
        }

        analisis ??= new AnalisisComprobanteDto();
        analisis.RespuestaIaJson = json;
        if (string.IsNullOrWhiteSpace(analisis.Moneda)) analisis.Moneda = "PEN";
        return analisis;
    }

    /// <summary>Quita posibles cercos markdown (```json ... ```) que algunos modelos agregan.</summary>
    private static string LimpiarJson(string raw)
    {
        var s = raw.Trim();
        if (s.StartsWith("```"))
        {
            var start = s.IndexOf('{');
            var end = s.LastIndexOf('}');
            if (start >= 0 && end > start) return s[start..(end + 1)];
        }
        return s;
    }

    /// <summary>
    /// Prompt del motor de extracción. Adaptado para recibir TEXTO (OCR) en vez de imagen.
    /// </summary>
    private const string SystemPrompt = """
Eres un motor especializado en análisis de comprobantes de pago peruanos para sistemas de tesorería.

Tu tarea es analizar el TEXTO de un comprobante (extraído por OCR) y devolver EXCLUSIVAMENTE un JSON válido.

IMPORTANTE:
- No escribas explicaciones.
- No escribas texto adicional.
- No utilices markdown.
- No utilices bloques de código.
- No inventes información.
- Si un dato no es visible o no puede determinarse con seguridad, devuelve null.
- Todos los importes deben devolverse como números decimales.
- Todas las fechas deben devolverse en formato YYYY-MM-DD.
- La respuesta debe ser un único JSON válido.

TIPOS DE DOCUMENTOS POSIBLES: Factura, Boleta, Ticket, Recibo, Voucher, Nota de venta, Otro.

CATEGORÍAS PERMITIDAS: Combustible, Alimentacion, Papeleria, Materiales, Servicios, Movilidad, Mantenimiento, Tecnologia, Limpieza, Seguridad, Salud, Capacitacion, Otros.

REGLAS DE EXTRACCIÓN:
1. proveedor: Nombre comercial o razón social visible.
2. ruc: Extraer únicamente el número de RUC.
3. tipo_documento: Factura, Boleta, Ticket, Recibo, Voucher, Nota de venta u Otro.
4. numero_comprobante: Serie y correlativo del documento.
5. fecha_emision: Fecha del comprobante.
6. hora_emision: Hora si existe.
7. moneda: PEN o USD.
8. subtotal: Subtotal antes de impuestos si existe.
9. igv: Impuesto IGV si existe.
10. total: Importe final pagado.
11. categoria: Elegir UNA sola categoría de la lista permitida.
12. concepto: Generar una descripción corta del gasto.
13. metodo_pago: Efectivo, Tarjeta, Transferencia, Yape, Plin, Otro o null.
14. productos: Lista detallada de productos o servicios encontrados.
15. confianza: Número entre 0 y 100 representando la confianza de la extracción.
16. observaciones: Información relevante detectada.
17. es_duplicado_probable: true si se observa que podría ser una reimpresión o duplicado; false en caso contrario.

REGLAS DE CLASIFICACIÓN:
- Grifos y estaciones de servicio -> Combustible
- Restaurantes, cafeterías, mercados -> Alimentacion
- Librerías y útiles -> Papeleria
- Ferreterías -> Materiales
- Empresas de internet, telefonía y software -> Tecnologia
- Empresas de limpieza -> Limpieza
- Talleres y reparaciones -> Mantenimiento
- Transporte, taxis, buses -> Movilidad
- Clínicas, farmacias y boticas -> Salud
- Servicios generales -> Servicios
- Si no aplica ninguna -> Otros

FORMATO DE RESPUESTA (usa exactamente estas claves):
{
"proveedor": null,
"ruc": null,
"tipo_documento": null,
"numero_comprobante": null,
"fecha_emision": null,
"hora_emision": null,
"moneda": "PEN",
"subtotal": null,
"igv": null,
"total": null,
"categoria": null,
"concepto": null,
"metodo_pago": null,
"confianza": 0,
"es_duplicado_probable": false,
"observaciones": null,
"productos": [
{ "descripcion": null, "cantidad": null, "precio_unitario": null, "importe": null }
]
}
""";
}
