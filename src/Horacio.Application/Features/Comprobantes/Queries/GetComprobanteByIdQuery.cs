using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Comprobantes.Commands;
using Horacio.Application.Features.Comprobantes.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Comprobantes.Queries;

/// <summary>Obtiene un comprobante de egreso completo (con su detalle e imagen).</summary>
public record GetComprobanteByIdQuery(int Id) : IRequest<ComprobanteDto>;

public class GetComprobanteByIdQueryHandler : IRequestHandler<GetComprobanteByIdQuery, ComprobanteDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;

    public GetComprobanteByIdQueryHandler(IUnitOfWork uow, IDateTimeService dateTime)
    {
        _uow = uow;
        _dateTime = dateTime;
    }

    public async Task<ComprobanteDto> Handle(GetComprobanteByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await _uow.Repository<Comprobante>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Comprobante", request.Id);

        var productos = await _uow.Repository<ComprobanteProducto>()
            .ListAsync(p => p.ComprobanteId == c.Id, cancellationToken);
        c.Productos = productos.ToList();

        var usuario = (await _uow.Repository<Usuario>().GetByIdAsync(c.UsuarioId, cancellationToken))?.Username ?? string.Empty;

        var dto = RegistrarComprobanteCommandHandler.Mapear(c, usuario);
        dto.FechaRegistro = _dateTime.ToLocal(c.FechaRegistro);
        return dto;
    }
}
