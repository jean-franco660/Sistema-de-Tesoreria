using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Periodos.DTOs;
using Horacio.Application.Features.Reportes.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Periodos.Queries;

/// <summary>Detalle completo de ingresos de un período (lista + resúmenes) — guardado para el histórico.</summary>
public class PeriodoDetalleDto
{
    public PeriodoResumenDto Periodo { get; set; } = new();
    public List<ReporteIngresoItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public List<ReporteResumen> ResumenPorServicio { get; set; } = new();
    public List<ReporteResumen> ResumenPorPrograma { get; set; } = new();
    public List<ReporteResumen> ResumenPorUsuario { get; set; } = new();
}

public record GetPeriodoDetalleQuery(int Id) : IRequest<PeriodoDetalleDto>;

public class GetPeriodoDetalleQueryHandler : IRequestHandler<GetPeriodoDetalleQuery, PeriodoDetalleDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;
    public GetPeriodoDetalleQueryHandler(IUnitOfWork uow, IDateTimeService dateTime) { _uow = uow; _dateTime = dateTime; }

    public async Task<PeriodoDetalleDto> Handle(GetPeriodoDetalleQuery request, CancellationToken ct)
    {
        var periodo = await _uow.Repository<PeriodoAcademico>().GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Período", request.Id);

        var tickets = (await _uow.Repository<Ticket>()
            .ListAsync(t => t.PeriodoAcademicoId == request.Id && t.Estado == EstadoTicket.Emitido, ct))
            .ToDictionary(t => t.Id);
        var detalles = await _uow.Repository<DetalleTicket>().ListAllAsync(ct);
        var servicios = (await _uow.Repository<Servicio>().ListAllAsync(ct)).ToDictionary(s => s.Id, s => s.Nombre);
        var alumnos = (await _uow.Repository<Alumno>().ListAllAsync(ct)).ToDictionary(a => a.Id);
        var programas = (await _uow.Repository<Programa>().ListAllAsync(ct)).ToDictionary(p => p.Id, p => p.Nombre);
        var usuarios = (await _uow.Repository<Usuario>().ListAllAsync(ct)).ToDictionary(u => u.Id, u => u.Username);

        var items = new List<ReporteIngresoItem>();
        foreach (var d in detalles)
        {
            if (!tickets.TryGetValue(d.TicketId, out var t)) continue;
            alumnos.TryGetValue(t.AlumnoId, out var al);
            items.Add(new ReporteIngresoItem
            {
                Fecha = _dateTime.ToLocal(t.FechaEmision),
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

        ReporteResumen[] Resumen(Func<ReporteIngresoItem, string> key) => ordenados
            .GroupBy(key)
            .Select(g => new ReporteResumen { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(i => i.Importe) })
            .OrderByDescending(r => r.Monto).ToArray();

        return new PeriodoDetalleDto
        {
            Periodo = await ResumenHelper.CalcularAsync(_uow, periodo, ct),
            Items = ordenados,
            Total = ordenados.Sum(i => i.Importe),
            ResumenPorServicio = Resumen(i => i.Servicio).ToList(),
            ResumenPorPrograma = Resumen(i => i.Programa).ToList(),
            ResumenPorUsuario = Resumen(i => i.Usuario).ToList()
        };
    }
}
