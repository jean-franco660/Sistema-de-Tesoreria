using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Periodos.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using MediatR;

namespace Horacio.Application.Features.Periodos.Queries;

/// <summary>Sección "Gestión financiera por período" del dashboard (período activo).</summary>
public record GetDashboardPeriodoQuery : IRequest<DashboardPeriodoDto>;

public class GetDashboardPeriodoQueryHandler : IRequestHandler<GetDashboardPeriodoQuery, DashboardPeriodoDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;
    public GetDashboardPeriodoQueryHandler(IUnitOfWork uow, IDateTimeService dateTime) { _uow = uow; _dateTime = dateTime; }

    public async Task<DashboardPeriodoDto> Handle(GetDashboardPeriodoQuery request, CancellationToken ct)
    {
        var activo = await _uow.Repository<PeriodoAcademico>().FirstOrDefaultAsync(p => p.Estado == EstadoPeriodo.Abierto, ct);
        if (activo is null) return new DashboardPeriodoDto { HayPeriodoActivo = false };

        var resumen = await ResumenHelper.CalcularAsync(_uow, activo, ct);
        var hoy = _dateTime.Now;
        return new DashboardPeriodoDto
        {
            HayPeriodoActivo = true,
            Nombre = activo.Nombre,
            FechaInicio = activo.FechaInicio,
            FechaFin = activo.FechaFin,
            IngresosAcumulados = resumen.Ingresos,
            Tickets = resumen.Tickets,
            Estudiantes = resumen.Estudiantes,
            Servicios = resumen.Servicios,
            DiasRestantes = Math.Max(0, (activo.FechaFin.Date - hoy.Date).Days)
        };
    }
}
