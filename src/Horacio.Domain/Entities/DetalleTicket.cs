using Horacio.Domain.Common;

namespace Horacio.Domain.Entities;

/// <summary>
/// Línea de un ticket: un servicio y su importe.
/// </summary>
public class DetalleTicket : BaseEntity
{
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public int ServicioId { get; set; }
    public Servicio? Servicio { get; set; }

    public decimal Importe { get; set; }
}
