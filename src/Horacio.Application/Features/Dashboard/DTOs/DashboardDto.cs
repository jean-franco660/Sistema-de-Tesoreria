using Horacio.Application.Features.Tickets.Queries;

namespace Horacio.Application.Features.Dashboard.DTOs;

public class DashboardDto
{
    public decimal TotalRecaudadoHoy { get; set; }
    public decimal TotalRecaudadoMes { get; set; }
    public int TicketsHoy { get; set; }
    public int TicketsMes { get; set; }

    // Valores del periodo anterior (para calcular variación %)
    public decimal TotalRecaudadoAyer { get; set; }
    public decimal TotalRecaudadoMesPasado { get; set; }
    public int TicketsAyer { get; set; }
    public int TicketsMesPasado { get; set; }

    public int ProgramasActivos { get; set; }

    public List<ConteoItem> ServiciosMasCobrados { get; set; } = new();
    public List<ConteoItem> ProgramasConMasPagos { get; set; } = new();
    public List<TicketListItemDto> UltimosTickets { get; set; } = new();
    public List<IngresoDiario> IngresosDiarios { get; set; } = new();
}

/// <summary>Total recaudado en un día (para el gráfico de los últimos 30 días).</summary>
public class IngresoDiario
{
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
}

/// <summary>Elemento de ranking: nombre, cantidad y monto acumulado.</summary>
public class ConteoItem
{
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}
