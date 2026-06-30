using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Comparativa.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using MediatR;

namespace Horacio.Application.Features.Comparativa.Queries;

/// <summary>
/// Comparativa de INGRESOS (tickets) vs EGRESOS (comprobantes) entre dos fechas locales.
/// </summary>
public record GetComparativaQuery(DateTime? Desde = null, DateTime? Hasta = null)
    : IRequest<ComparativaDto>;

public class GetComparativaQueryHandler : IRequestHandler<GetComparativaQuery, ComparativaDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;

    public GetComparativaQueryHandler(IUnitOfWork uow, IDateTimeService dateTime)
    {
        _uow = uow;
        _dateTime = dateTime;
    }

    public async Task<ComparativaDto> Handle(GetComparativaQuery request, CancellationToken cancellationToken)
    {
        var desde = request.Desde?.Date;
        var hasta = request.Hasta?.Date;
        bool EnRango(DateTime fecha) =>
            (!desde.HasValue || fecha.Date >= desde.Value) && (!hasta.HasValue || fecha.Date <= hasta.Value);

        // ── INGRESOS (tickets emitidos) ───────────────────────────────────────
        var tickets = (await _uow.Repository<Ticket>().ListAllAsync(cancellationToken))
            .Where(t => t.Estado == EstadoTicket.Emitido)
            .Select(t => new { t, Fecha = _dateTime.ToLocal(t.FechaEmision) })
            .Where(x => EnRango(x.Fecha))
            .ToList();
        var ticketIds = tickets.Select(x => x.t.Id).ToHashSet();

        var detalles = (await _uow.Repository<DetalleTicket>().ListAllAsync(cancellationToken))
            .Where(d => ticketIds.Contains(d.TicketId)).ToList();
        var servicios = (await _uow.Repository<Servicio>().ListAllAsync(cancellationToken))
            .ToDictionary(s => s.Id, s => s.Nombre);

        // ── EGRESOS (comprobantes registrados) ────────────────────────────────
        var comprobantes = (await _uow.Repository<Comprobante>().ListAllAsync(cancellationToken))
            .Where(c => c.Estado == EstadoComprobante.Registrado)
            .Select(c => new { c, Fecha = (c.FechaEmision?.Date ?? _dateTime.ToLocal(c.FechaRegistro).Date) })
            .Where(x => EnRango(x.Fecha))
            .ToList();

        var usuarios = (await _uow.Repository<Usuario>().ListAllAsync(cancellationToken))
            .ToDictionary(u => u.Id, u => u.Username);
        var alumnos = (await _uow.Repository<Alumno>().ListAllAsync(cancellationToken))
            .ToDictionary(a => a.Id, a => a.NombreCompleto);

        var totalIngresos = tickets.Sum(x => x.t.Total);
        var totalEgresos = comprobantes.Sum(x => x.c.Total);

        // ── Serie diaria combinada ────────────────────────────────────────────
        var porDiaIng = tickets.GroupBy(x => x.Fecha.Date).ToDictionary(g => g.Key, g => g.Sum(x => x.t.Total));
        var porDiaEgr = comprobantes.GroupBy(x => x.Fecha.Date).ToDictionary(g => g.Key, g => g.Sum(x => x.c.Total));
        var dias = porDiaIng.Keys.Union(porDiaEgr.Keys).OrderBy(d => d).ToList();
        var serie = dias.Select(d => new ComparativaDiaDto
        {
            Fecha = d.ToString("yyyy-MM-dd"),
            Ingresos = porDiaIng.GetValueOrDefault(d, 0m),
            Egresos = porDiaEgr.GetValueOrDefault(d, 0m)
        }).ToList();

        // ── Egresos por categoría ─────────────────────────────────────────────
        var egresosPorCategoria = comprobantes
            .GroupBy(x => string.IsNullOrWhiteSpace(x.c.Categoria) ? "Otros" : x.c.Categoria!)
            .Select(g => new ComparativaGrupoDto { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(x => x.c.Total) })
            .OrderByDescending(g => g.Monto).ToList();

        // ── Ingresos por concepto (servicio) ──────────────────────────────────
        var ingresosPorConcepto = detalles
            .GroupBy(d => servicios.GetValueOrDefault(d.ServicioId, "Otros"))
            .Select(g => new ComparativaGrupoDto { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(d => d.Importe) })
            .OrderByDescending(g => g.Monto).ToList();

        // ── Movimientos unificados (últimos 60) ───────────────────────────────
        var movimientos = tickets.Select(x => new MovimientoDto
        {
            Tipo = "Ingreso",
            Fecha = x.Fecha,
            Descripcion = x.t.NumeroTicket,
            Detalle = alumnos.GetValueOrDefault(x.t.AlumnoId, string.Empty),
            Monto = x.t.Total,
            Usuario = usuarios.GetValueOrDefault(x.t.UsuarioId, string.Empty)
        }).Concat(comprobantes.Select(x => new MovimientoDto
        {
            Tipo = "Egreso",
            Fecha = x.Fecha,
            Descripcion = x.c.Proveedor ?? x.c.NumeroComprobante ?? "Egreso",
            Detalle = x.c.Categoria ?? "Otros",
            Monto = x.c.Total,
            Usuario = usuarios.GetValueOrDefault(x.c.UsuarioId, string.Empty)
        }))
        .OrderByDescending(m => m.Fecha)
        .Take(60)
        .ToList();

        return new ComparativaDto
        {
            Desde = request.Desde,
            Hasta = request.Hasta,
            TotalIngresos = totalIngresos,
            TotalEgresos = totalEgresos,
            Balance = totalIngresos - totalEgresos,
            CantidadIngresos = tickets.Count,
            CantidadEgresos = comprobantes.Count,
            Serie = serie,
            EgresosPorCategoria = egresosPorCategoria,
            IngresosPorConcepto = ingresosPorConcepto,
            Movimientos = movimientos
        };
    }
}
