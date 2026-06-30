using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Periodos.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using MediatR;

namespace Horacio.Application.Features.Periodos.Queries;

/// <summary>Lista todos los períodos académicos (histórico) con totales.</summary>
public record GetPeriodosQuery : IRequest<IReadOnlyList<PeriodoDto>>;

/// <summary>Devuelve el período ABIERTO actual (o null si no hay).</summary>
public record GetPeriodoActivoQuery : IRequest<PeriodoDto?>;

public static class PeriodoMapper
{
    public static PeriodoDto Map(PeriodoAcademico p, IReadOnlyList<Ticket> ticketsPeriodo, DateTime hoy)
    {
        var diasRestantes = p.Estado == EstadoPeriodo.Abierto
            ? Math.Max(0, (p.FechaFin.Date - hoy.Date).Days)
            : (int?)null;

        return new PeriodoDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            Estado = p.Estado.ToString(),
            UsuarioApertura = p.UsuarioApertura,
            FechaApertura = p.FechaApertura,
            UsuarioCierre = p.UsuarioCierre,
            FechaCierre = p.FechaCierre,
            Observaciones = p.Observaciones,
            Tickets = ticketsPeriodo.Count,
            Ingresos = ticketsPeriodo.Sum(t => t.Total),
            DiasRestantes = diasRestantes
        };
    }
}

public class GetPeriodosQueryHandler : IRequestHandler<GetPeriodosQuery, IReadOnlyList<PeriodoDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;
    public GetPeriodosQueryHandler(IUnitOfWork uow, IDateTimeService dateTime) { _uow = uow; _dateTime = dateTime; }

    public async Task<IReadOnlyList<PeriodoDto>> Handle(GetPeriodosQuery request, CancellationToken ct)
    {
        var periodos = await _uow.Repository<PeriodoAcademico>().ListAllAsync(ct);
        var tickets = (await _uow.Repository<Ticket>().ListAllAsync(ct)).Where(t => t.Estado == EstadoTicket.Emitido).ToList();
        var hoy = _dateTime.Now;
        return periodos
            .OrderByDescending(p => p.FechaInicio)
            .Select(p => PeriodoMapper.Map(p, tickets.Where(t => t.PeriodoAcademicoId == p.Id).ToList(), hoy))
            .ToList();
    }
}

public class GetPeriodoActivoQueryHandler : IRequestHandler<GetPeriodoActivoQuery, PeriodoDto?>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;
    public GetPeriodoActivoQueryHandler(IUnitOfWork uow, IDateTimeService dateTime) { _uow = uow; _dateTime = dateTime; }

    public async Task<PeriodoDto?> Handle(GetPeriodoActivoQuery request, CancellationToken ct)
    {
        var activo = await _uow.Repository<PeriodoAcademico>().FirstOrDefaultAsync(p => p.Estado == EstadoPeriodo.Abierto, ct);
        if (activo is null) return null;
        var tickets = (await _uow.Repository<Ticket>().ListAsync(t => t.PeriodoAcademicoId == activo.Id && t.Estado == EstadoTicket.Emitido, ct));
        return PeriodoMapper.Map(activo, tickets, _dateTime.Now);
    }
}
