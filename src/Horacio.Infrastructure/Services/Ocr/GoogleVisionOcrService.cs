using System.Text;
using System.Text.Json;
using Horacio.Application.Common.Interfaces;
using Horacio.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Horacio.Infrastructure.Services.Ocr;

/// <summary>
/// OCR vía Google Cloud Vision (REST, TEXT_DETECTION). Recibe la imagen en base64
/// y devuelve el texto completo detectado.
/// </summary>
public class GoogleVisionOcrService : IOcrService
{
    private readonly HttpClient _http;
    private readonly OcrSettings _settings;
    private readonly ILogger<GoogleVisionOcrService> _logger;

    public GoogleVisionOcrService(HttpClient http, IOptions<OcrSettings> options, ILogger<GoogleVisionOcrService> logger)
    {
        _http = http;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> ExtraerTextoAsync(string imagenBase64, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("Falta configurar la API key de Google Cloud Vision (Ocr:ApiKey).");

        var contenido = LimpiarBase64(imagenBase64);

        var payload = new
        {
            requests = new[]
            {
                new
                {
                    image = new { content = contenido },
                    features = new[] { new { type = "TEXT_DETECTION" } }
                }
            }
        };

        var url = $"{_settings.Endpoint}?key={_settings.ApiKey}";
        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        using var resp = await _http.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Google Vision devolvió {Status}: {Body}", (int)resp.StatusCode, body);
            throw new InvalidOperationException($"Error del OCR (Google Vision): {(int)resp.StatusCode}.");
        }

        using var doc = JsonDocument.Parse(body);
        var responses = doc.RootElement.GetProperty("responses");
        if (responses.GetArrayLength() == 0) return string.Empty;
        var first = responses[0];

        if (first.TryGetProperty("error", out var error))
        {
            var msg = error.TryGetProperty("message", out var m) ? m.GetString() : "desconocido";
            _logger.LogError("Google Vision error: {Msg}", msg);
            throw new InvalidOperationException($"Error del OCR (Google Vision): {msg}.");
        }

        // fullTextAnnotation.text es el texto completo; si no, textAnnotations[0].description.
        if (first.TryGetProperty("fullTextAnnotation", out var full)
            && full.TryGetProperty("text", out var text))
            return text.GetString() ?? string.Empty;

        if (first.TryGetProperty("textAnnotations", out var anns) && anns.GetArrayLength() > 0
            && anns[0].TryGetProperty("description", out var desc))
            return desc.GetString() ?? string.Empty;

        return string.Empty;
    }

    /// <summary>Quita el prefijo data URL (ej. "data:image/jpeg;base64,") si lo trae.</summary>
    private static string LimpiarBase64(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var idx = raw.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? raw[(idx + "base64,".Length)..].Trim() : raw.Trim();
    }
}
