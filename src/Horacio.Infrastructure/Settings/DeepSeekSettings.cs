namespace Horacio.Infrastructure.Settings;

/// <summary>
/// Configuración del motor de IA que estructura el comprobante (sección "DeepSeek").
/// Provider = "DeepSeek" (real) | "Mock" (desarrollo sin key).
/// La API de DeepSeek es compatible con el formato de OpenAI (chat/completions).
/// </summary>
public class DeepSeekSettings
{
    public const string SectionName = "DeepSeek";

    public string Provider { get; set; } = "Mock";

    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Endpoint de chat (OpenAI-compatible).</summary>
    public string BaseUrl { get; set; } = "https://api.deepseek.com/chat/completions";

    public string Model { get; set; } = "deepseek-chat";
}
