using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Servicios.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using MediatR;

namespace Horacio.Application.Features.Servicios.Queries;

/// <summary>Lista los servicios cobrables (opcionalmente solo los activos).</summary>
public record GetServiciosQuery(bool SoloActivos = false) : IRequest<IReadOnlyList<ServicioDto>>;

public class GetServiciosQueryHandler : IRequestHandler<GetServiciosQuery, IReadOnlyList<ServicioDto>>
{
    private readonly IUnitOfWork _uow;

    public GetServiciosQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<ServicioDto>> Handle(GetServiciosQuery request, CancellationToken cancellationToken)
    {
        var servicios = await _uow.Repository<Servicio>().ListAllAsync(cancellationToken);

        return servicios
            .Where(s => !request.SoloActivos || s.Estado == EstadoRegistro.Activo)
            .OrderBy(s => s.Id)
            .Select(s => new ServicioDto
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Precio = s.Precio,
                Estado = s.Estado.ToString()
            })
            .ToList();
    }
}
