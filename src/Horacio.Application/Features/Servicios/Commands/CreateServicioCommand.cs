using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Servicios.Commands;

/// <summary>Crea un nuevo servicio cobrable.</summary>
public record CreateServicioCommand(string Nombre, decimal Precio) : IRequest<int>;

public class CreateServicioCommandValidator : AbstractValidator<CreateServicioCommand>
{
    public CreateServicioCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Precio).GreaterThanOrEqualTo(0);
    }
}

public class CreateServicioCommandHandler : IRequestHandler<CreateServicioCommand, int>
{
    private readonly IUnitOfWork _uow;

    public CreateServicioCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<int> Handle(CreateServicioCommand request, CancellationToken cancellationToken)
    {
        var nombre = request.Nombre.Trim();

        var existe = await _uow.Repository<Servicio>()
            .AnyAsync(s => s.Nombre.ToLower() == nombre.ToLower(), cancellationToken);
        if (existe)
            throw new DomainException($"Ya existe un servicio con el nombre '{nombre}'.");

        var servicio = new Servicio { Nombre = nombre, Precio = request.Precio, Estado = EstadoRegistro.Activo };
        await _uow.Repository<Servicio>().AddAsync(servicio, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return servicio.Id;
    }
}
