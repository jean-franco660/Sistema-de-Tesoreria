using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Registros.Commands;

/// <summary>
/// Elimina un Registro de Matrícula vacío. No se permite si ya tiene estudiantes
/// (para no perder el vínculo del aula con sus matriculados).
/// </summary>
public record EliminarRegistroCommand(int Id) : IRequest<Unit>;

public class EliminarRegistroCommandHandler : IRequestHandler<EliminarRegistroCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public EliminarRegistroCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(EliminarRegistroCommand request, CancellationToken ct)
    {
        var registro = await _uow.Repository<RegistroMatricula>().GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("RegistroMatricula", request.Id);

        var tieneAlumnos = await _uow.Repository<Alumno>().AnyAsync(
            a => a.PeriodoAcademicoId == registro.PeriodoAcademicoId
                 && a.ProgramaId == registro.ProgramaId
                 && a.TurnoId == registro.TurnoId
                 && a.Seccion == registro.Seccion, ct);

        if (tieneAlumnos)
            throw new DomainException("No se puede eliminar un registro que ya tiene estudiantes matriculados.");

        _uow.Repository<RegistroMatricula>().Remove(registro);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
