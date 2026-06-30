namespace Horacio.Application.Features.Tickets.DTOs;

/// <summary>Ticket completo, con todo lo necesario para impresión térmica.</summary>
public class TicketDto
{
    public int Id { get; set; }
    public string NumeroTicket { get; set; } = string.Empty;
    public string Contador { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }

    public string Dni { get; set; } = string.Empty;
    public string AlumnoNombre { get; set; } = string.Empty;
    public string Programa { get; set; } = string.Empty;
    public string Turno { get; set; } = string.Empty;

    public decimal Total { get; set; }
    public string TotalEnLetras { get; set; } = string.Empty;

    public string Usuario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    public List<TicketDetalleDto> Detalles { get; set; } = new();
}

public class TicketDetalleDto
{
    public int ServicioId { get; set; }
    public string Servicio { get; set; } = string.Empty;
    public decimal Importe { get; set; }
}
