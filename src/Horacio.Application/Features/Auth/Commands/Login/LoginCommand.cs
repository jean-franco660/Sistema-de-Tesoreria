using Horacio.Application.Features.Auth.DTOs;
using MediatR;

namespace Horacio.Application.Features.Auth.Commands.Login;

/// <summary>Comando de inicio de sesión.</summary>
public record LoginCommand(string Username, string Password) : IRequest<AuthResponse>;
