using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Programas.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using MediatR;

namespace Horacio.Application.Features.Programas.Queries;

/// <summary>Lista los programas de estudio (opcionalmente solo los activos).</summary>
public record GetProgramasQuery(bool SoloActivos = false) : IRequest<IReadOnlyList<ProgramaDto>>;

public class GetProgramasQueryHandler : IRequestHandler<GetProgramasQuery, IReadOnlyList<ProgramaDto>>
{
    private readonly IUnitOfWork _uow;

    public GetProgramasQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<ProgramaDto>> Handle(GetProgramasQuery request, CancellationToken cancellationToken)
    {
        var programas = await _uow.Repository<Programa>().ListAllAsync(cancellationToken);

        return programas
            .Where(p => !request.SoloActivos || p.Estado == EstadoRegistro.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProgramaDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Estado = p.Estado.ToString(),
                FechaCreacion = p.FechaCreacion
            })
            .ToList();
    }
}
