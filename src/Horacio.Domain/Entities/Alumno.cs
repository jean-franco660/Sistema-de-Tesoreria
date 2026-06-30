using Horacio.Domain.Common;
using Horacio.Domain.Enums;

namespace Horacio.Domain.Entities;

/// <summary>
/// Estudiante registrado. El DNI (8 dígitos) es único.
/// Los nombres pueden provenir de la BD local o de la consulta automática a RENIEC.
/// </summary>
public class Alumno : BaseEntity
{
    public string Dni { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;

    // Programa de estudios (FK)
    public int ProgramaId { get; set; }
    public Programa? Programa { get; set; }

    // Turno del catálogo (FK). En el documento de origen figura como "Turno".
    public int TurnoId { get; set; }
    public Turno? Turno { get; set; }

    /// <summary>Sección del registro de matrícula (ej. "A", "B", "U").</summary>
    public string Seccion { get; set; } = "U";

    /// <summary>Período académico de matrícula al que pertenece el alumno.</summary>
    public int? PeriodoAcademicoId { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }

    /// <summary>Sexo del estudiante: "H" (Hombre) o "M" (Mujer). Vacío si no se conoce.</summary>
    public string? Sexo { get; set; }

    /// <summary>Fecha de nacimiento (UTC). La edad se calcula a partir de ella.</summary>
    public DateTime? FechaNacimiento { get; set; }

    /// <summary>Número de celular de contacto (opcional).</summary>
    public string? Celular { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relaciones
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    /// <summary>Nombre completo en formato "APELLIDOS NOMBRES".</summary>
    public string NombreCompleto => $"{Apellidos} {Nombres}".Trim();

    /// <summary>Edad en años cumplidos calculada desde <see cref="FechaNacimiento"/>.</summary>
    public int? Edad
    {
        get
        {
            if (FechaNacimiento is null) return null;
            var hoy = DateTime.UtcNow.Date;
            var edad = hoy.Year - FechaNacimiento.Value.Year;
            if (FechaNacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
            return edad < 0 ? null : edad;
        }
    }
}
