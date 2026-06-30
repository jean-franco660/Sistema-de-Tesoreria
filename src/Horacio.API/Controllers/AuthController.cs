using Horacio.Application.Features.Auth.Commands.Login;
using Horacio.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

public class AuthController : ApiControllerBase
{
    /// <summary>Inicia sesión y devuelve un token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginCommand command, CancellationToken ct)
        => Ok(await Mediator.Send(command, ct));
}
