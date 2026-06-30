using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using MediatR;

namespace Horacio.Application.Features.Tickets.Queries;

/// <summary>Resumen de ticket para listados.</summary>
public class TicketListItemDto
{
    public int Id { get; set; }
    public string NumeroTicket { get; set; } = string.Empty;
    public string Contador { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string AlumnoNombre { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

/// <summary>Lista tickets, opcionalmente filtrados por rango de fechas (local) y/o DNI del alumno.</summary>
public record GetTicketsQuery(DateTime? Desde = null, DateTime? Hasta = null, string? Dni = null) : IRequest<IReadOnlyList<TicketListItemDto>>;

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, IReadOnlyList<TicketListItemDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;

    public GetTicketsQueryHandler(IUnitOfWork uow, IDateTimeService dateTime)
    {
        _uow = uow;
        _dateTime = dateTime;
    }

    public async Task<IReadOnlyList<TicketListItemDto>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _uow.Repository<Ticket>().ListAllAsync(cancellationToken);
        var alumnos = (await _uow.Repository<Alumno>().ListAllAsync(cancellationToken))
            .ToDictionary(a => a.Id);
        var usuarios = (await _uow.Repository<Usuario>().ListAllAsync(cancellationToken))
            .ToDictionary(u => u.Id, u => u.Username);

        var dni = request.Dni?.Trim();

        return tickets
            .Select(t => new { Ticket = t, Local = _dateTime.ToLocal(t.FechaEmision), Dni = alumnos.TryGetValue(t.AlumnoId, out var a) ? a.Dni : string.Empty })
            .Where(x => (!request.Desde.HasValue || x.Local.Date >= request.Desde.Value.Date)
                        && (!request.Hasta.HasValue || x.Local.Date <= request.Hasta.Value.Date)
                        && (string.IsNullOrEmpty(dni) || x.Dni == dni))
            .OrderByDescending(x => x.Ticket.Id)
            .Select(x => new TicketListItemDto
            {
                Id = x.Ticket.Id,
                NumeroTicket = x.Ticket.NumeroTicket,
                Contador = x.Ticket.Contador,
                FechaEmision = x.Local,
                Dni = alumnos.TryGetValue(x.Ticket.AlumnoId, out var al) ? al.Dni : string.Empty,
                AlumnoNombre = alumnos.TryGetValue(x.Ticket.AlumnoId, out var al2) ? al2.NombreCompleto : string.Empty,
                Total = x.Ticket.Total,
                Usuario = usuarios.GetValueOrDefault(x.Ticket.UsuarioId, string.Empty),
                Estado = x.Ticket.Estado.ToString()
            })
            .ToList();
    }
}
