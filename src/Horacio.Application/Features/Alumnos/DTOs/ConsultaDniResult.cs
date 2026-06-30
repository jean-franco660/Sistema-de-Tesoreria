namespace Horacio.Application.Features.Alumnos.DTOs;

/// <summary>
/// Resultado de la consulta automática por DNI (8 dígitos).
/// El frontend la invoca SIN botón al completar el DNI:
///  - Existe = true  -> el alumno ya está registrado (se muestran sus datos).
///  - EncontradoReniec = true -> los nombres vienen de RENIEC (falta elegir programa y turno).
///  - ambos false -> no se encontró; ingreso manual.
/// </summary>
public class ConsultaDniResult
{
    public string Dni { get; set; } = string.Empty;
    public bool Existe { get; set; }
    public bool EncontradoReniec { get; set; }

    public int? AlumnoId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;

    public int? ProgramaId { get; set; }
    public string? Programa { get; set; }
    public int? TurnoId { get; set; }
    public string? Turno { get; set; }

    /// <summary>Sexo "H"/"M" (de la BD local o, si el proveedor lo entrega, de RENIEC).</summary>
    public string? Sexo { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public int? Edad { get; set; }
    public string? Celular { get; set; }
    public string? Seccion { get; set; }
}
