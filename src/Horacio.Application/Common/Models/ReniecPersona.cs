namespace Horacio.Application.Common.Models;

/// <summary>
/// Datos de una persona devueltos por el proveedor RENIEC.
/// </summary>
public class ReniecPersona
{
    public string Dni { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;

    /// <summary>Sexo "H"/"M" si el proveedor lo entrega (no en el plan gratuito).</summary>
    public string? Sexo { get; set; }

    /// <summary>Fecha de nacimiento si el proveedor la entrega (no en el plan gratuito).</summary>
    public DateTime? FechaNacimiento { get; set; }

    /// <summary>Apellidos concatenados "PATERNO MATERNO".</summary>
    public string Apellidos => $"{ApellidoPaterno} {ApellidoMaterno}".Trim();
}
