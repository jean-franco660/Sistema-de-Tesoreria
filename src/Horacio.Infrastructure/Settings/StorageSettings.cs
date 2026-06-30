namespace Horacio.Infrastructure.Settings;

/// <summary>
/// Configuración del almacenamiento de archivos (sección "Storage").
/// En el Droplet: RootPath apunta a un directorio persistente (ej. /var/www/uploads)
/// y PublicBaseUrl a la URL desde la que se sirven.
/// </summary>
public class StorageSettings
{
    public const string SectionName = "Storage";

    /// <summary>Directorio raíz donde se guardan los archivos. Por defecto "wwwroot/uploads".</summary>
    public string RootPath { get; set; } = "wwwroot/uploads";

    /// <summary>Base URL pública para construir la URL del archivo (ej. "/uploads"). Opcional.</summary>
    public string PublicBaseUrl { get; set; } = "/uploads";
}
