using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Turnos.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using MediatR;

namespace Horacio.Application.Features.Turnos.Queries;

/// <summary>Lista el catálogo de turnos activos (MAÑANA, TARDE, NOCHE).</summary>
public record GetTurnosQuery : IRequest<IReadOnlyList<TurnoDto>>;

public class GetTurnosQueryHandler : IRequestHandler<GetTurnosQuery, IReadOnlyList<TurnoDto>>
{
    private readonly IUnitOfWork _uow;

    public GetTurnosQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<TurnoDto>> Handle(GetTurnosQuery request, CancellationToken cancellationToken)
    {
        var turnos = await _uow.Repository<Turno>().ListAllAsync(cancellationToken);

        return turnos
            .Where(t => t.Estado == EstadoRegistro.Activo)
            .OrderBy(t => t.Id)
            .Select(t => new TurnoDto { Id = t.Id, Nombre = t.Nombre })
            .ToList();
    }
}
