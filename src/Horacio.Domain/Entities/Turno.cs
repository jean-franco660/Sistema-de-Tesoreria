using Horacio.Domain.Common;
using Horacio.Domain.Enums;

namespace Horacio.Domain.Entities;

/// <summary>
/// Catálogo de turnos: MAÑANA, TARDE, NOCHE.
/// El alumno referencia un turno de este catálogo (TurnoId).
/// </summary>
public class Turno : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    // Relaciones
    public ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();
}
