using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Programas.Commands;

/// <summary>Crea un nuevo programa de estudios.</summary>
public record CreateProgramaCommand(string Nombre) : IRequest<int>;

public class CreateProgramaCommandValidator : AbstractValidator<CreateProgramaCommand>
{
    public CreateProgramaCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}

public class CreateProgramaCommandHandler : IRequestHandler<CreateProgramaCommand, int>
{
    private readonly IUnitOfWork _uow;

    public CreateProgramaCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<int> Handle(CreateProgramaCommand request, CancellationToken cancellationToken)
    {
        var nombre = request.Nombre.Trim();

        var existe = await _uow.Repository<Programa>()
            .AnyAsync(p => p.Nombre.ToLower() == nombre.ToLower(), cancellationToken);
        if (existe)
            throw new DomainException($"Ya existe un programa con el nombre '{nombre}'.");

        var programa = new Programa { Nombre = nombre, Estado = EstadoRegistro.Activo };
        await _uow.Repository<Programa>().AddAsync(programa, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return programa.Id;
    }
}
