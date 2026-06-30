namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Servicio de OCR (reconocimiento óptico de caracteres). Extrae el texto plano
/// de la foto de un comprobante. Implementación por defecto: Google Cloud Vision.
/// Intercambiable por configuración.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Devuelve el texto detectado en la imagen.
    /// </summary>
    /// <param name="imagenBase64">Imagen en base64 (con o sin prefijo data URL).</param>
    Task<string> ExtraerTextoAsync(string imagenBase64, CancellationToken ct = default);
}
