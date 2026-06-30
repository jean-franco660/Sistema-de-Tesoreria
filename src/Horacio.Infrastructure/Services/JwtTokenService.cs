using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Horacio.Infrastructure.Services;

/// <summary>Genera tokens JWT firmados con HMAC-SHA256.</summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options) => _settings = options.Value;

    public int MinutosExpiracion => _settings.ExpirationMinutes;

    public string GenerarToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Username),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new("nombre", usuario.NombreCompleto)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
