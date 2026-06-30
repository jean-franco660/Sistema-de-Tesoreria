namespace Horacio.Infrastructure.Settings;

/// <summary>
/// Configuración del OCR (sección "Ocr" de appsettings).
/// Provider = "GoogleVision" (real) | "Mock" (desarrollo sin key).
/// </summary>
public class OcrSettings
{
    public const string SectionName = "Ocr";

    public string Provider { get; set; } = "Mock";

    /// <summary>API key de Google Cloud Vision.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Endpoint REST (annotate). Por defecto el oficial de Google.</summary>
    public string Endpoint { get; set; } = "https://vision.googleapis.com/v1/images:annotate";
}
