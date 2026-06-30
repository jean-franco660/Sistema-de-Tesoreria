namespace Horacio.Application.Common.Interfaces;

/// <summary>Resultado de almacenar un archivo.</summary>
public record ArchivoGuardado(string Ruta, string? Url);

/// <summary>
/// Almacenamiento de archivos (fotos de comprobantes). Implementación por defecto:
/// sistema de archivos local del servidor (Droplet). Intercambiable (S3/Spaces) por config.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Guarda la foto de un comprobante y devuelve su ruta relativa y URL pública (si aplica).
    /// </summary>
    /// <param name="contenido">Bytes del archivo.</param>
    /// <param name="extension">Extensión con punto (ej. ".jpg").</param>
    Task<ArchivoGuardado> GuardarComprobanteAsync(byte[] contenido, string extension, CancellationToken ct = default);
}
