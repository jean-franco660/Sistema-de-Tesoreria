using Horacio.Application.Common.Interfaces;
using Horacio.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Horacio.Infrastructure.Services.Storage;

/// <summary>
/// Almacenamiento en el sistema de archivos local del servidor (Droplet).
/// Organiza las fotos por año/mes: {RootPath}/comprobantes/yyyy/MM/{guid}{ext}.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly StorageSettings _settings;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IOptions<StorageSettings> options, ILogger<LocalFileStorageService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<ArchivoGuardado> GuardarComprobanteAsync(byte[] contenido, string extension, CancellationToken ct = default)
    {
        if (contenido is null || contenido.Length == 0)
            throw new InvalidOperationException("El archivo está vacío.");

        var ext = string.IsNullOrWhiteSpace(extension) ? ".jpg" : (extension.StartsWith('.') ? extension : "." + extension);
        var ahora = DateTime.UtcNow;
        var subcarpeta = Path.Combine("comprobantes", ahora.ToString("yyyy"), ahora.ToString("MM"));
        var nombre = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";

        var carpetaAbs = Path.Combine(_settings.RootPath, subcarpeta);
        Directory.CreateDirectory(carpetaAbs);

        var rutaAbs = Path.Combine(carpetaAbs, nombre);
        await File.WriteAllBytesAsync(rutaAbs, contenido, ct);

        // Ruta relativa (con separadores web) y URL pública.
        var rutaRel = Path.Combine(subcarpeta, nombre).Replace('\\', '/');
        var url = string.IsNullOrWhiteSpace(_settings.PublicBaseUrl)
            ? null
            : $"{_settings.PublicBaseUrl.TrimEnd('/')}/{rutaRel}";

        _logger.LogInformation("Comprobante guardado: {Ruta} ({Bytes} bytes)", rutaRel, contenido.Length);
        return new ArchivoGuardado(rutaRel, url);
    }
}
