using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Servicios.Commands;

/// <summary>Edita el nombre y el estado (activo/inactivo) de un servicio.</summary>
public record UpdateServicioCommand(int Id, string Nombre, decimal Precio, bool Activo) : IRequest<Unit>;

public class UpdateServicioCommandValidator : AbstractValidator<UpdateServicioCommand>
{
    public UpdateServicioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Precio).GreaterThanOrEqualTo(0);
    }
}

public class UpdateServicioCommandHandler : IRequestHandler<UpdateServicioCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public UpdateServicioCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(UpdateServicioCommand request, CancellationToken cancellationToken)
    {
        var servicio = await _uow.Repository<Servicio>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Servicio", request.Id);

        var nombre = request.Nombre.Trim();
        var repetido = await _uow.Repository<Servicio>()
            .AnyAsync(s => s.Id != request.Id && s.Nombre.ToLower() == nombre.ToLower(), cancellationToken);
        if (repetido)
            throw new DomainException($"Ya existe otro servicio con el nombre '{nombre}'.");

        servicio.Nombre = nombre;
        servicio.Precio = request.Precio;
        servicio.Estado = request.Activo ? EstadoRegistro.Activo : EstadoRegistro.Inactivo;
        _uow.Repository<Servicio>().Update(servicio);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
