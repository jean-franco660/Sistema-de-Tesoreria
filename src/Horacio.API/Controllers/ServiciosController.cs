using Horacio.Application.Features.Servicios.Commands;
using Horacio.Application.Features.Servicios.DTOs;
using Horacio.Application.Features.Servicios.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize]
public class ServiciosController : ApiControllerBase
{
    /// <summary>Lista los servicios cobrables.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServicioDto>>> Get(
        [FromQuery] bool soloActivos = false, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetServiciosQuery(soloActivos), ct));

    /// <summary>Crea un servicio (solo Administrador).</summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<int>> Create(CreateServicioCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Edita un servicio (solo Administrador).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, UpdateServicioCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest(new { mensaje = "El id de la ruta no coincide." });
        await Mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Elimina un servicio (solo Administrador).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteServicioCommand(id), ct);
        return NoContent();
    }
}
