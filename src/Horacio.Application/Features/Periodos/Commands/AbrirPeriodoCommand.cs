using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Periodos.Commands;

/// <summary>Abre un nuevo período académico (solo puede haber uno abierto).</summary>
public record AbrirPeriodoCommand(string Nombre, DateTime FechaInicio, DateTime FechaFin, string? Observaciones)
    : IRequest<int>;

public class AbrirPeriodoCommandValidator : AbstractValidator<AbrirPeriodoCommand>
{
    public AbrirPeriodoCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FechaFin).GreaterThan(x => x.FechaInicio).WithMessage("La fecha de fin debe ser posterior a la de inicio.");
    }
}

public class AbrirPeriodoCommandHandler : IRequestHandler<AbrirPeriodoCommand, int>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AbrirPeriodoCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(AbrirPeriodoCommand request, CancellationToken cancellationToken)
    {
        if (await _uow.Repository<PeriodoAcademico>().AnyAsync(p => p.Estado == EstadoPeriodo.Abierto, cancellationToken))
            throw new DomainException("Ya existe un período abierto. Ciérrelo antes de abrir uno nuevo.");

        var nombre = request.Nombre.Trim();
        if (await _uow.Repository<PeriodoAcademico>().AnyAsync(p => p.Nombre.ToLower() == nombre.ToLower(), cancellationToken))
            throw new DomainException($"Ya existe un período con el nombre '{nombre}'.");

        var periodo = new PeriodoAcademico
        {
            Nombre = nombre,
            FechaInicio = DateTime.SpecifyKind(request.FechaInicio.Date, DateTimeKind.Utc),
            FechaFin = DateTime.SpecifyKind(request.FechaFin.Date, DateTimeKind.Utc),
            Estado = EstadoPeriodo.Abierto,
            UsuarioApertura = _currentUser.Username ?? "sistema",
            FechaApertura = DateTime.UtcNow,
            Observaciones = request.Observaciones?.Trim()
        };

        await _uow.Repository<PeriodoAcademico>().AddAsync(periodo, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return periodo.Id;
    }
}
