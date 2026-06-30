using Horacio.Domain.Common;

namespace Horacio.Domain.Entities;

/// <summary>
/// Registro de Matrícula: un grupo/aula identificado por la combinación
/// Período + Programa + Turno + Sección. Puede crearse VACÍO (ej. al abrir
/// un programa o sección nuevos) y se va llenando automáticamente a medida
/// que se registran estudiantes en Tesorería que coincidan con esa combinación.
/// </summary>
public class RegistroMatricula : BaseEntity
{
    public int PeriodoAcademicoId { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }

    public int ProgramaId { get; set; }
    public Programa? Programa { get; set; }

    public int TurnoId { get; set; }
    public Turno? Turno { get; set; }

    /// <summary>Sección del aula (ej. "A", "B", "C", "U").</summary>
    public string Seccion { get; set; } = "U";

    /// <summary>Docente/profesor responsable del aula (se ingresa al crear el registro).</summary>
    public string? Profesor { get; set; }

    /// <summary>Módulo formativo del programa (se ingresa al crear el registro).</summary>
    public string? ModuloFormativo { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
