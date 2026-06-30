using Horacio.Application.Features.Configuracion.Commands;
using Horacio.Application.Features.Configuracion.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

public class ConfiguracionController : ApiControllerBase
{
    /// <summary>Configuración institucional (pública: el login muestra logo y nombre).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ConfiguracionDto>> Get(CancellationToken ct)
        => Ok(await Mediator.Send(new GetConfiguracionQuery(), ct));

    /// <summary>Actualiza la configuración (solo Administrador).</summary>
    [HttpPut]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(UpdateConfiguracionCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }
}
