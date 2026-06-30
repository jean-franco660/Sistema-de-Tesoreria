using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Programas.Commands;

/// <summary>Edita el nombre y el estado (activo/inactivo) de un programa.</summary>
public record UpdateProgramaCommand(int Id, string Nombre, bool Activo) : IRequest<Unit>;

public class UpdateProgramaCommandValidator : AbstractValidator<UpdateProgramaCommand>
{
    public UpdateProgramaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}

public class UpdateProgramaCommandHandler : IRequestHandler<UpdateProgramaCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public UpdateProgramaCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(UpdateProgramaCommand request, CancellationToken cancellationToken)
    {
        var programa = await _uow.Repository<Programa>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Programa", request.Id);

        var nombre = request.Nombre.Trim();
        var repetido = await _uow.Repository<Programa>()
            .AnyAsync(p => p.Id != request.Id && p.Nombre.ToLower() == nombre.ToLower(), cancellationToken);
        if (repetido)
            throw new DomainException($"Ya existe otro programa con el nombre '{nombre}'.");

        programa.Nombre = nombre;
        programa.Estado = request.Activo ? EstadoRegistro.Activo : EstadoRegistro.Inactivo;
        _uow.Repository<Programa>().Update(programa);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
