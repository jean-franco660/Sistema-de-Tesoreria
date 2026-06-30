using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Dashboard.DTOs;
using Horacio.Application.Features.Tickets.Queries;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using MediatR;

namespace Horacio.Application.Features.Dashboard.Queries;

/// <summary>Métricas para el panel principal.</summary>
public record GetDashboardQuery : IRequest<DashboardDto>;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;

    public GetDashboardQueryHandler(IUnitOfWork uow, IDateTimeService dateTime)
    {
        _uow = uow;
        _dateTime = dateTime;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var tickets = (await _uow.Repository<Ticket>().ListAllAsync(cancellationToken))
            .Where(t => t.Estado == EstadoTicket.Emitido)
            .ToList();
        var detalles = await _uow.Repository<DetalleTicket>().ListAllAsync(cancellationToken);
        var servicios = (await _uow.Repository<Servicio>().ListAllAsync(cancellationToken))
            .ToDictionary(s => s.Id, s => s.Nombre);
        var alumnos = (await _uow.Repository<Alumno>().ListAllAsync(cancellationToken))
            .ToDictionary(a => a.Id);
        var programasList = await _uow.Repository<Programa>().ListAllAsync(cancellationToken);
        var programas = programasList.ToDictionary(p => p.Id, p => p.Nombre);
        var usuarios = (await _uow.Repository<Usuario>().ListAllAsync(cancellationToken))
            .ToDictionary(u => u.Id, u => u.Username);

        var hoy = _dateTime.Now.Date;
        var ayer = hoy.AddDays(-1);
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var inicioMesPasado = inicioMes.AddMonths(-1);

        var ticketsLocal = tickets
            .Select(t => new { Ticket = t, Local = _dateTime.ToLocal(t.FechaEmision) })
            .ToList();

        var dto = new DashboardDto
        {
            TotalRecaudadoHoy = ticketsLocal.Where(x => x.Local.Date == hoy).Sum(x => x.Ticket.Total),
            TotalRecaudadoMes = ticketsLocal.Where(x => x.Local.Date >= inicioMes).Sum(x => x.Ticket.Total),
            TicketsHoy = ticketsLocal.Count(x => x.Local.Date == hoy),
            TicketsMes = ticketsLocal.Count(x => x.Local.Date >= inicioMes),

            TotalRecaudadoAyer = ticketsLocal.Where(x => x.Local.Date == ayer).Sum(x => x.Ticket.Total),
            TicketsAyer = ticketsLocal.Count(x => x.Local.Date == ayer),
            TotalRecaudadoMesPasado = ticketsLocal.Where(x => x.Local.Date >= inicioMesPasado && x.Local.Date < inicioMes).Sum(x => x.Ticket.Total),
            TicketsMesPasado = ticketsLocal.Count(x => x.Local.Date >= inicioMesPasado && x.Local.Date < inicioMes)
        };

        // Servicios más cobrados (por cantidad de líneas y monto).
        dto.ServiciosMasCobrados = detalles
            .GroupBy(d => d.ServicioId)
            .Select(g => new ConteoItem
            {
                Nombre = servicios.GetValueOrDefault(g.Key, "—"),
                Cantidad = g.Count(),
                Monto = g.Sum(d => d.Importe)
            })
            .OrderByDescending(c => c.Monto)
            .Take(5)
            .ToList();

        // Programas con más pagos (por cantidad de tickets y monto).
        dto.ProgramasConMasPagos = tickets
            .Where(t => alumnos.ContainsKey(t.AlumnoId))
            .GroupBy(t => alumnos[t.AlumnoId].ProgramaId)
            .Select(g => new ConteoItem
            {
                Nombre = programas.GetValueOrDefault(g.Key, "—"),
                Cantidad = g.Count(),
                Monto = g.Sum(t => t.Total)
            })
            .OrderByDescending(c => c.Monto)
            .Take(5)
            .ToList();

        // Programas activos.
        dto.ProgramasActivos = programasList.Count(p => p.Estado == EstadoRegistro.Activo);

        // Ingresos de los últimos 30 días (serie para el gráfico).
        dto.IngresosDiarios = Enumerable.Range(0, 30)
            .Select(offset => hoy.AddDays(-29 + offset))
            .Select(d => new IngresoDiario
            {
                Fecha = d,
                Monto = ticketsLocal.Where(x => x.Local.Date == d).Sum(x => x.Ticket.Total)
            })
            .ToList();

        // Últimos 5 tickets emitidos.
        dto.UltimosTickets = ticketsLocal
            .OrderByDescending(x => x.Ticket.Id)
            .Take(5)
            .Select(x => new TicketListItemDto
            {
                Id = x.Ticket.Id,
                NumeroTicket = x.Ticket.NumeroTicket,
                Contador = x.Ticket.Contador,
                FechaEmision = x.Local,
                Dni = alumnos.TryGetValue(x.Ticket.AlumnoId, out var a) ? a.Dni : string.Empty,
                AlumnoNombre = alumnos.TryGetValue(x.Ticket.AlumnoId, out var a2) ? a2.NombreCompleto : string.Empty,
                Total = x.Ticket.Total,
                Usuario = usuarios.GetValueOrDefault(x.Ticket.UsuarioId, string.Empty),
                Estado = x.Ticket.Estado.ToString()
            })
            .ToList();

        return dto;
    }
}
