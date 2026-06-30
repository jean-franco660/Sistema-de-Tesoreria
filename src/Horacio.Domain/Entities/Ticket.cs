using Horacio.Domain.Common;
using Horacio.Domain.Enums;

namespace Horacio.Domain.Entities;

/// <summary>
/// Comprobante interno de tesorería. Puede agrupar varios servicios (DetalleTicket).
/// NO es factura/boleta electrónica ni documento SUNAT.
/// </summary>
public class Ticket : BaseEntity
{
    /// <summary>Número visible formateado, ej. "001/001".</summary>
    public string NumeroTicket { get; set; } = string.Empty;

    /// <summary>Contador general persistente, ej. "000000001".</summary>
    public string Contador { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

    public int AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }

    public decimal Total { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    // Período académico al que pertenece el ingreso.
    public int? PeriodoAcademicoId { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }

    public EstadoTicket Estado { get; set; } = EstadoTicket.Emitido;

    // Relaciones
    public ICollection<DetalleTicket> Detalles { get; set; } = new List<DetalleTicket>();

    /// <summary>Recalcula el total a partir de las líneas de detalle.</summary>
    public void RecalcularTotal() => Total = Detalles.Sum(d => d.Importe);
}
