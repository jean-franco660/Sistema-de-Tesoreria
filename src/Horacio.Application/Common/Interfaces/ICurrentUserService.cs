namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Información del usuario autenticado en la petición actual.
/// </summary>
public interface ICurrentUserService
{
    int? UsuarioId { get; }
    string? Username { get; }
    string? Rol { get; }
    string? IpAddress { get; }
}
