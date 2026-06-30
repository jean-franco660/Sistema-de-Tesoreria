using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Programas.Commands;

/// <summary>Elimina un programa (si no tiene alumnos asociados).</summary>
public record DeleteProgramaCommand(int Id) : IRequest<Unit>;

public class DeleteProgramaCommandHandler : IRequestHandler<DeleteProgramaCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public DeleteProgramaCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(DeleteProgramaCommand request, CancellationToken cancellationToken)
    {
        var programa = await _uow.Repository<Programa>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Programa", request.Id);

        if (await _uow.Repository<Alumno>().AnyAsync(a => a.ProgramaId == request.Id, cancellationToken))
            throw new DomainException("No se puede eliminar: el programa tiene alumnos asociados. Desactívelo en su lugar.");

        _uow.Repository<Programa>().Remove(programa);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
