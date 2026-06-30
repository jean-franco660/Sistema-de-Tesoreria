using Horacio.Domain.Common;
using Horacio.Domain.Enums;

namespace Horacio.Domain.Entities;

/// <summary>
/// Programa de estudios del instituto (carrera técnica productiva).
/// </summary>
public class Programa : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();
}
