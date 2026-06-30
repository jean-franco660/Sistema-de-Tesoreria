using System.Security.Claims;
using Horacio.Application.Common.Interfaces;

namespace Horacio.API.Services;

/// <summary>Obtiene los datos del usuario autenticado desde el HttpContext.</summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public int? UsuarioId
        => int.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? Username => User?.FindFirstValue(ClaimTypes.Name);

    public string? Rol => User?.FindFirstValue(ClaimTypes.Role);

    public string? IpAddress => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
