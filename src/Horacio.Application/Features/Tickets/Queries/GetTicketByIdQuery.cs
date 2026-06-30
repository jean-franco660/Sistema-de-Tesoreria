using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Tickets.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Tickets.Queries;

/// <summary>Obtiene un ticket completo (para reimpresión / visualización).</summary>
public record GetTicketByIdQuery(int Id) : IRequest<TicketDto>;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketDto>
{
    private readonly IUnitOfWork _uow;
    private readonly INumberToWordsService _numberToWords;
    private readonly IDateTimeService _dateTime;

    public GetTicketByIdQueryHandler(IUnitOfWork uow, INumberToWordsService numberToWords, IDateTimeService dateTime)
    {
        _uow = uow;
        _numberToWords = numberToWords;
        _dateTime = dateTime;
    }

    public async Task<TicketDto> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _uow.Repository<Ticket>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Ticket", request.Id);

        var detalles = await _uow.Repository<DetalleTicket>()
            .ListAsync(d => d.TicketId == ticket.Id, cancellationToken);
        var serviciosDict = (await _uow.Repository<Servicio>().ListAllAsync(cancellationToken))
            .ToDictionary(s => s.Id, s => s.Nombre);

        var alumno = await _uow.Repository<Alumno>().GetByIdAsync(ticket.AlumnoId, cancellationToken);
        var programa = alumno is null ? null
            : await _uow.Repository<Programa>().GetByIdAsync(alumno.ProgramaId, cancellationToken);
        var turno = alumno is null ? null
            : await _uow.Repository<Turno>().GetByIdAsync(alumno.TurnoId, cancellationToken);
        var usuario = await _uow.Repository<Usuario>().GetByIdAsync(ticket.UsuarioId, cancellationToken);

        return new TicketDto
        {
            Id = ticket.Id,
            NumeroTicket = ticket.NumeroTicket,
            Contador = ticket.Contador,
            FechaEmision = _dateTime.ToLocal(ticket.FechaEmision),
            Dni = alumno?.Dni ?? string.Empty,
            AlumnoNombre = alumno?.NombreCompleto ?? string.Empty,
            Programa = programa?.Nombre ?? string.Empty,
            Turno = turno?.Nombre ?? string.Empty,
            Total = ticket.Total,
            TotalEnLetras = _numberToWords.ConvertirImporte(ticket.Total),
            Usuario = usuario?.Username ?? "TESORERIA",
            Estado = ticket.Estado.ToString(),
            Detalles = detalles
                .Select(d => new TicketDetalleDto
                {
                    ServicioId = d.ServicioId,
                    Servicio = serviciosDict.GetValueOrDefault(d.ServicioId, string.Empty),
                    Importe = d.Importe
                })
                .ToList()
        };
    }
}
