using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Auth.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public LoginCommandHandler(IUnitOfWork uow, IPasswordHasher hasher, IJwtTokenService jwt)
    {
        _uow = uow;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _uow.Repository<Usuario>()
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (usuario is null
            || usuario.Estado != EstadoRegistro.Activo
            || !_hasher.Verify(request.Password, usuario.PasswordHash))
        {
            throw new DomainException("Usuario o contraseña incorrectos.");
        }

        var token = _jwt.GenerarToken(usuario);

        return new AuthResponse
        {
            Token = token,
            Username = usuario.Username,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol.ToString(),
            ExpiraEnMinutos = _jwt.MinutosExpiracion
        };
    }
}
