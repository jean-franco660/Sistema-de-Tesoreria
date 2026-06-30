using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Usuarios.Commands;

/// <summary>Crea un usuario del sistema.</summary>
public record CreateUsuarioCommand(string Username, string NombreCompleto, string Password, string Rol) : IRequest<int>;

public class CreateUsuarioCommandValidator : AbstractValidator<CreateUsuarioCommand>
{
    public CreateUsuarioCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
        RuleFor(x => x.Rol).Must(r => r is "Administrador" or "Finanzas").WithMessage("Rol inválido (Administrador o Finanzas).");
    }
}

public class CreateUsuarioCommandHandler : IRequestHandler<CreateUsuarioCommand, int>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;

    public CreateUsuarioCommandHandler(IUnitOfWork uow, IPasswordHasher hasher)
    {
        _uow = uow;
        _hasher = hasher;
    }

    public async Task<int> Handle(CreateUsuarioCommand request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim().ToLowerInvariant();

        if (await _uow.Repository<Usuario>().AnyAsync(u => u.Username.ToLower() == username, cancellationToken))
            throw new DomainException($"Ya existe el usuario '{username}'.");

        var usuario = new Usuario
        {
            Username = username,
            NombreCompleto = request.NombreCompleto.Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            Rol = request.Rol == "Administrador" ? RolUsuario.Administrador : RolUsuario.Finanzas,
            Estado = EstadoRegistro.Activo
        };

        await _uow.Repository<Usuario>().AddAsync(usuario, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return usuario.Id;
    }
}
