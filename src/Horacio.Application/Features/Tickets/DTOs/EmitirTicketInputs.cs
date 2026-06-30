namespace Horacio.Application.Features.Tickets.DTOs;

/// <summary>Línea de servicio al emitir un ticket.</summary>
public class EmitirTicketDetalleInput
{
    public int ServicioId { get; set; }
    public decimal Importe { get; set; }
}
