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
        Console.WriteLine($"[LOGIN DBG] Intentando login para usuario: '{request.Username}'");

        var usuario = await _uow.Repository<Usuario>()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower(), cancellationToken);

        if (usuario is null)
        {
            Console.WriteLine($"[LOGIN DBG] Usuario '{request.Username}' no encontrado en la base de datos.");
            throw new DomainException("Usuario o contraseña incorrectos.");
        }

        Console.WriteLine($"[LOGIN DBG] Usuario encontrado. Estado: {usuario.Estado}, Rol: {usuario.Rol}");
        
        bool passwordValido = _hasher.Verify(request.Password, usuario.PasswordHash);
        Console.WriteLine($"[LOGIN DBG] Verificación de password: {passwordValido}");

        if (usuario.Estado != EstadoRegistro.Activo || !passwordValido)
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
