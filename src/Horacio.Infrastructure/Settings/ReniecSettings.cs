namespace Horacio.Infrastructure.Settings;

/// <summary>
/// Configuración del proveedor RENIEC (sección "Reniec" de appsettings).
/// Provider = "Fake" (desarrollo) | "Api" (proveedor HTTP real).
/// </summary>
public class ReniecSettings
{
    public const string SectionName = "Reniec";

    public string Provider { get; set; } = "Fake";
    public string BaseUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
