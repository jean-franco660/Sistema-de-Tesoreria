using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Reportes.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using MediatR;

namespace Horacio.Application.Features.Reportes.Queries;

/// <summary>Informe de ingresos por recursos propios entre dos fechas (locales).</summary>
public record GetReporteIngresosQuery(DateTime? Desde = null, DateTime? Hasta = null)
    : IRequest<ReporteIngresosDto>;

public class GetReporteIngresosQueryHandler : IRequestHandler<GetReporteIngresosQuery, ReporteIngresosDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;

    public GetReporteIngresosQueryHandler(IUnitOfWork uow, IDateTimeService dateTime)
    {
        _uow = uow;
        _dateTime = dateTime;
    }

    public async Task<ReporteIngresosDto> Handle(GetReporteIngresosQuery request, CancellationToken cancellationToken)
    {
        var tickets = (await _uow.Repository<Ticket>().ListAllAsync(cancellationToken))
            .Where(t => t.Estado == EstadoTicket.Emitido)
            .ToDictionary(t => t.Id);
        var detalles = await _uow.Repository<DetalleTicket>().ListAllAsync(cancellationToken);
        var servicios = (await _uow.Repository<Servicio>().ListAllAsync(cancellationToken)).ToDictionary(s => s.Id, s => s.Nombre);
        var alumnos = (await _uow.Repository<Alumno>().ListAllAsync(cancellationToken)).ToDictionary(a => a.Id);
        var programas = (await _uow.Repository<Programa>().ListAllAsync(cancellationToken)).ToDictionary(p => p.Id, p => p.Nombre);
        var usuarios = (await _uow.Repository<Usuario>().ListAllAsync(cancellationToken)).ToDictionary(u => u.Id, u => u.Username);

        var items = new List<ReporteIngresoItem>();
        foreach (var d in detalles)
        {
            if (!tickets.TryGetValue(d.TicketId, out var t)) continue;
            var local = _dateTime.ToLocal(t.FechaEmision);
            if (request.Desde.HasValue && local.Date < request.Desde.Value.Date) continue;
            if (request.Hasta.HasValue && local.Date > request.Hasta.Value.Date) continue;

            alumnos.TryGetValue(t.AlumnoId, out var al);
            items.Add(new ReporteIngresoItem
            {
                Fecha = local,
                NumeroTicket = t.NumeroTicket,
                Contador = t.Contador,
                Dni = al?.Dni ?? string.Empty,
                Alumno = al?.NombreCompleto ?? string.Empty,
                Programa = al is null ? string.Empty : programas.GetValueOrDefault(al.ProgramaId, string.Empty),
                Servicio = servicios.GetValueOrDefault(d.ServicioId, string.Empty),
                Importe = d.Importe,
                Usuario = usuarios.GetValueOrDefault(t.UsuarioId, string.Empty)
            });
        }

        var ordenados = items.OrderBy(i => i.Fecha).ThenBy(i => i.NumeroTicket).ToList();

        return new ReporteIngresosDto
        {
            Desde = request.Desde,
            Hasta = request.Hasta,
            CantidadTickets = ordenados.Select(i => i.NumeroTicket).Distinct().Count(),
            CantidadServicios = ordenados.Count,
            Total = ordenados.Sum(i => i.Importe),
            Items = ordenados,
            ResumenPorServicio = ordenados
                .GroupBy(i => i.Servicio)
                .Select(g => new ReporteResumen { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(i => i.Importe) })
                .OrderByDescending(r => r.Monto).ToList(),
            ResumenPorPrograma = ordenados
                .GroupBy(i => i.Programa)
                .Select(g => new ReporteResumen { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(i => i.Importe) })
                .OrderByDescending(r => r.Monto).ToList()
        };
    }
}
