using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Usuarios.Commands;

/// <summary>Activa o desactiva un usuario.</summary>
public record ToggleUsuarioCommand(int Id) : IRequest<Unit>;

public class ToggleUsuarioCommandHandler : IRequestHandler<ToggleUsuarioCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public ToggleUsuarioCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(ToggleUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _uow.Repository<Usuario>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Usuario", request.Id);

        usuario.Estado = usuario.Estado == EstadoRegistro.Activo ? EstadoRegistro.Inactivo : EstadoRegistro.Activo;
        _uow.Repository<Usuario>().Update(usuario);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
