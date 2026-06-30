using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Registros.Commands;

/// <summary>
/// Crea un Registro de Matrícula (aula) — normalmente VACÍO — para un programa,
/// turno y sección dentro de un período. Si no se indica período, se usa el activo.
/// </summary>
public record CrearRegistroCommand(
    int ProgramaId, int TurnoId, string? Seccion = "U", int? PeriodoId = null,
    string? Profesor = null, string? ModuloFormativo = null)
    : IRequest<int>;

public class CrearRegistroCommandValidator : AbstractValidator<CrearRegistroCommand>
{
    public CrearRegistroCommandValidator()
    {
        RuleFor(x => x.ProgramaId).GreaterThan(0).WithMessage("Debe seleccionar un programa.");
        RuleFor(x => x.TurnoId).GreaterThan(0).WithMessage("Debe seleccionar un turno (horario).");
    }
}

public class CrearRegistroCommandHandler : IRequestHandler<CrearRegistroCommand, int>
{
    private readonly IUnitOfWork _uow;
    public CrearRegistroCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<int> Handle(CrearRegistroCommand request, CancellationToken ct)
    {
        var programa = await _uow.Repository<Programa>().GetByIdAsync(request.ProgramaId, ct)
            ?? throw new NotFoundException("Programa", request.ProgramaId);

        var turno = await _uow.Repository<Turno>().GetByIdAsync(request.TurnoId, ct)
            ?? throw new NotFoundException("Turno", request.TurnoId);

        var periodo = request.PeriodoId.HasValue
            ? await _uow.Repository<PeriodoAcademico>().GetByIdAsync(request.PeriodoId.Value, ct)
            : await _uow.Repository<PeriodoAcademico>().FirstOrDefaultAsync(p => p.Estado == EstadoPeriodo.Abierto, ct);

        if (periodo is null)
            throw new DomainException("No existe un período académico para crear el registro. Abra un período primero.");

        var seccion = string.IsNullOrWhiteSpace(request.Seccion) ? "U" : request.Seccion.Trim().ToUpperInvariant();

        var yaExiste = await _uow.Repository<RegistroMatricula>().AnyAsync(
            r => r.PeriodoAcademicoId == periodo.Id && r.ProgramaId == programa.Id
                 && r.TurnoId == turno.Id && r.Seccion == seccion, ct);

        if (yaExiste)
            throw new DomainException($"Ya existe un registro de matrícula para {programa.Nombre} · {turno.Nombre} · Sección {seccion} en {periodo.Nombre}.");

        var registro = new RegistroMatricula
        {
            PeriodoAcademicoId = periodo.Id,
            ProgramaId = programa.Id,
            TurnoId = turno.Id,
            Seccion = seccion,
            Profesor = string.IsNullOrWhiteSpace(request.Profesor) ? null : request.Profesor.Trim(),
            ModuloFormativo = string.IsNullOrWhiteSpace(request.ModuloFormativo) ? null : request.ModuloFormativo.Trim()
        };

        await _uow.Repository<RegistroMatricula>().AddAsync(registro, ct);
        await _uow.SaveChangesAsync(ct);

        return registro.Id;
    }
}
