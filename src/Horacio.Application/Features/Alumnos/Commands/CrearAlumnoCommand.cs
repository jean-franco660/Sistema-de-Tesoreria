using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Alumnos.Commands;

/// <summary>
/// Registra un alumno. Tras la consulta automática por DNI, el usuario solo
/// elige Programa y Turno; los nombres ya vienen de RENIEC o se ingresan.
/// </summary>
public record CrearAlumnoCommand(
    string Dni,
    string Nombres,
    string Apellidos,
    int ProgramaId,
    int TurnoId,
    string? Seccion = "U",
    string? Sexo = null,
    DateTime? FechaNacimiento = null,
    string? Celular = null) : IRequest<int>;

public class CrearAlumnoCommandValidator : AbstractValidator<CrearAlumnoCommand>
{
    public CrearAlumnoCommandValidator()
    {
        RuleFor(x => x.Dni)
            .NotEmpty().Length(8).Matches("^[0-9]{8}$")
            .WithMessage("El DNI debe tener 8 dígitos.");
        RuleFor(x => x.Nombres).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ProgramaId).GreaterThan(0).WithMessage("Debe seleccionar un programa.");
        RuleFor(x => x.TurnoId).GreaterThan(0).WithMessage("Debe seleccionar un turno.");
    }
}

public class CrearAlumnoCommandHandler : IRequestHandler<CrearAlumnoCommand, int>
{
    private readonly IUnitOfWork _uow;

    public CrearAlumnoCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<int> Handle(CrearAlumnoCommand request, CancellationToken cancellationToken)
    {
        var dni = request.Dni.Trim();

        if (await _uow.Repository<Alumno>().AnyAsync(a => a.Dni == dni, cancellationToken))
            throw new DomainException($"Ya existe un alumno con DNI {dni}.");

        var programa = await _uow.Repository<Programa>().GetByIdAsync(request.ProgramaId, cancellationToken)
            ?? throw new NotFoundException("Programa", request.ProgramaId);
        if (programa.Estado != EstadoRegistro.Activo)
            throw new DomainException("El programa seleccionado no está activo.");

        var turno = await _uow.Repository<Turno>().GetByIdAsync(request.TurnoId, cancellationToken)
            ?? throw new NotFoundException("Turno", request.TurnoId);

        // Se asocia automáticamente al período académico ABIERTO (matrícula del período actual).
        var periodoActivo = await _uow.Repository<PeriodoAcademico>()
            .FirstOrDefaultAsync(p => p.Estado == EstadoPeriodo.Abierto, cancellationToken);

        var seccion = string.IsNullOrWhiteSpace(request.Seccion) ? "U" : request.Seccion.Trim().ToUpperInvariant();
        var sexo = request.Sexo?.Trim().ToUpperInvariant();
        if (sexo is not ("H" or "M")) sexo = null;
        var fechaNac = request.FechaNacimiento is { } fn
            ? DateTime.SpecifyKind(fn.Date, DateTimeKind.Utc)
            : (DateTime?)null;

        var alumno = new Alumno
        {
            Dni = dni,
            Nombres = request.Nombres.Trim().ToUpperInvariant(),
            Apellidos = request.Apellidos.Trim().ToUpperInvariant(),
            ProgramaId = programa.Id,
            TurnoId = turno.Id,
            Seccion = seccion,
            PeriodoAcademicoId = periodoActivo?.Id,
            Sexo = sexo,
            FechaNacimiento = fechaNac,
            Celular = string.IsNullOrWhiteSpace(request.Celular) ? null : request.Celular.Trim(),
            Estado = EstadoRegistro.Activo
        };

        await _uow.Repository<Alumno>().AddAsync(alumno, cancellationToken);

        // El estudiante entra automáticamente a su Registro de Matrícula
        // (Período + Programa + Turno + Sección). Si aún no existe, se crea.
        if (periodoActivo is not null)
        {
            var existeRegistro = await _uow.Repository<RegistroMatricula>().AnyAsync(
                r => r.PeriodoAcademicoId == periodoActivo.Id
                     && r.ProgramaId == programa.Id
                     && r.TurnoId == turno.Id
                     && r.Seccion == seccion,
                cancellationToken);

            if (!existeRegistro)
            {
                await _uow.Repository<RegistroMatricula>().AddAsync(new RegistroMatricula
                {
                    PeriodoAcademicoId = periodoActivo.Id,
                    ProgramaId = programa.Id,
                    TurnoId = turno.Id,
                    Seccion = seccion
                }, cancellationToken);
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);

        return alumno.Id;
    }
}
