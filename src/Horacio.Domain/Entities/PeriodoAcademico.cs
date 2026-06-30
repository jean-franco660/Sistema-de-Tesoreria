using Horacio.Domain.Common;
using Horacio.Domain.Enums;

namespace Horacio.Domain.Entities;

/// <summary>
/// Período académico (ej. 2026-I). Los ingresos se agrupan por período.
/// Solo puede existir un período ABIERTO a la vez.
/// </summary>
public class PeriodoAcademico : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public EstadoPeriodo Estado { get; set; } = EstadoPeriodo.Abierto;

    public string? UsuarioApertura { get; set; }
    public DateTime FechaApertura { get; set; } = DateTime.UtcNow;
    public string? UsuarioCierre { get; set; }
    public DateTime? FechaCierre { get; set; }

    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
