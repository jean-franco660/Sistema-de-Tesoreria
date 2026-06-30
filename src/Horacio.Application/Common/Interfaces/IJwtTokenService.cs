using Horacio.Domain.Entities;

namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Generación de tokens JWT para los usuarios autenticados.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Genera un JWT con los claims del usuario (id, username, rol).</summary>
    string GenerarToken(Usuario usuario);

    /// <summary>Minutos de validez del token (para informar al cliente).</summary>
    int MinutosExpiracion { get; }
}
