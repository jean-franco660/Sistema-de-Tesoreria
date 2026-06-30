using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Matricula.Queries;
using Horacio.Domain.Entities;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Registros.Queries;

/// <summary>
/// Devuelve el padrón oficial (encabezado + estudiantes) de un Registro de
/// Matrícula concreto. Reutiliza el armado de <see cref="GetMatriculaQuery"/>.
/// </summary>
public record GetRegistroRosterQuery(int RegistroId) : IRequest<MatriculaDto>;

public class GetRegistroRosterQueryHandler : IRequestHandler<GetRegistroRosterQuery, MatriculaDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMediator _mediator;

    public GetRegistroRosterQueryHandler(IUnitOfWork uow, IMediator mediator)
    {
        _uow = uow;
        _mediator = mediator;
    }

    public async Task<MatriculaDto> Handle(GetRegistroRosterQuery request, CancellationToken ct)
    {
        var registro = await _uow.Repository<RegistroMatricula>().GetByIdAsync(request.RegistroId, ct)
            ?? throw new NotFoundException("RegistroMatricula", request.RegistroId);

        var dto = await _mediator.Send(
            new GetMatriculaQuery(registro.ProgramaId, registro.TurnoId, registro.Seccion, registro.PeriodoAcademicoId), ct);

        dto.Profesor = registro.Profesor;
        dto.ModuloFormativo = registro.ModuloFormativo;
        return dto;
    }
}
