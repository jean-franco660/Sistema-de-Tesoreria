using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Usuarios.Commands;

/// <summary>Elimina un usuario (si no tiene tickets emitidos y no es el usuario actual).</summary>
public record DeleteUsuarioCommand(int Id) : IRequest<Unit>;

public class DeleteUsuarioCommandHandler : IRequestHandler<DeleteUsuarioCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteUsuarioCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _uow.Repository<Usuario>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Usuario", request.Id);

        if (_currentUser.UsuarioId == request.Id)
            throw new DomainException("No puede eliminar su propio usuario.");

        if (await _uow.Repository<Ticket>().AnyAsync(t => t.UsuarioId == request.Id, cancellationToken))
            throw new DomainException("No se puede eliminar: el usuario tiene tickets emitidos. Desactívelo en su lugar.");

        _uow.Repository<Usuario>().Remove(usuario);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
