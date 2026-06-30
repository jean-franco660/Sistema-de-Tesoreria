using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Periodos.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Periodos.Queries;

/// <summary>Resumen completo de un período (tickets, estudiantes, ingresos, servicios) para cierre/acta.</summary>
public record GetPeriodoResumenQuery(int Id) : IRequest<PeriodoResumenDto>;

public class GetPeriodoResumenQueryHandler : IRequestHandler<GetPeriodoResumenQuery, PeriodoResumenDto>
{
    private readonly IUnitOfWork _uow;
    public GetPeriodoResumenQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PeriodoResumenDto> Handle(GetPeriodoResumenQuery request, CancellationToken ct)
    {
        var p = await _uow.Repository<PeriodoAcademico>().GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Período", request.Id);

        return await ResumenHelper.CalcularAsync(_uow, p, ct);
    }
}

public static class ResumenHelper
{
    public static async Task<PeriodoResumenDto> CalcularAsync(IUnitOfWork uow, PeriodoAcademico p, CancellationToken ct)
    {
        var tickets = (await uow.Repository<Ticket>()
            .ListAsync(t => t.PeriodoAcademicoId == p.Id && t.Estado == EstadoTicket.Emitido, ct)).ToList();
        var ids = tickets.Select(t => t.Id).ToHashSet();
        var detalles = await uow.Repository<DetalleTicket>().ListAllAsync(ct);

        return new PeriodoResumenDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            Estado = p.Estado.ToString(),
            Tickets = tickets.Count,
            Estudiantes = tickets.Select(t => t.AlumnoId).Distinct().Count(),
            Ingresos = tickets.Sum(t => t.Total),
            Servicios = detalles.Count(d => ids.Contains(d.TicketId)),
            UsuarioApertura = p.UsuarioApertura,
            UsuarioCierre = p.UsuarioCierre,
            FechaCierre = p.FechaCierre
        };
    }
}
