using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Comprobantes.DTOs;
using Horacio.Domain.Entities;
using MediatR;

namespace Horacio.Application.Features.Comprobantes.Queries;

/// <summary>
/// Lista comprobantes de egreso, opcionalmente filtrados por rango de fechas
/// (de registro, local) y/o categoría.
/// </summary>
public record GetComprobantesQuery(DateTime? Desde = null, DateTime? Hasta = null, string? Categoria = null)
    : IRequest<IReadOnlyList<ComprobanteListItemDto>>;

public class GetComprobantesQueryHandler : IRequestHandler<GetComprobantesQuery, IReadOnlyList<ComprobanteListItemDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;

    public GetComprobantesQueryHandler(IUnitOfWork uow, IDateTimeService dateTime)
    {
        _uow = uow;
        _dateTime = dateTime;
    }

    public async Task<IReadOnlyList<ComprobanteListItemDto>> Handle(GetComprobantesQuery request, CancellationToken cancellationToken)
    {
        var comprobantes = await _uow.Repository<Comprobante>().ListAllAsync(cancellationToken);
        var usuarios = (await _uow.Repository<Usuario>().ListAllAsync(cancellationToken))
            .ToDictionary(u => u.Id, u => u.Username);

        var categoria = request.Categoria?.Trim();

        return comprobantes
            .Select(c => new { C = c, Local = _dateTime.ToLocal(c.FechaRegistro) })
            .Where(x => (!request.Desde.HasValue || x.Local.Date >= request.Desde.Value.Date)
                        && (!request.Hasta.HasValue || x.Local.Date <= request.Hasta.Value.Date)
                        && (string.IsNullOrEmpty(categoria) || string.Equals(x.C.Categoria, categoria, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(x => x.C.Id)
            .Select(x => new ComprobanteListItemDto
            {
                Id = x.C.Id,
                Proveedor = x.C.Proveedor,
                Ruc = x.C.Ruc,
                TipoDocumento = x.C.TipoDocumento,
                NumeroComprobante = x.C.NumeroComprobante,
                FechaEmision = x.C.FechaEmision,
                FechaRegistro = x.Local,
                Total = x.C.Total,
                Moneda = x.C.Moneda,
                Categoria = x.C.Categoria,
                Concepto = x.C.Concepto,
                Confianza = x.C.Confianza,
                EsDuplicadoProbable = x.C.EsDuplicadoProbable,
                Usuario = usuarios.GetValueOrDefault(x.C.UsuarioId, string.Empty),
                Estado = x.C.Estado.ToString()
            })
            .ToList();
    }
}
