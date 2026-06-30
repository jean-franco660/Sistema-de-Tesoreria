using Horacio.Application.Features.Programas.Commands;
using Horacio.Application.Features.Programas.DTOs;
using Horacio.Application.Features.Programas.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize]
public class ProgramasController : ApiControllerBase
{
    /// <summary>Lista los programas de estudio.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgramaDto>>> Get(
        [FromQuery] bool soloActivos = false, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetProgramasQuery(soloActivos), ct));

    /// <summary>Crea un programa (solo Administrador).</summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<int>> Create(CreateProgramaCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Edita un programa (solo Administrador).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, UpdateProgramaCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest(new { mensaje = "El id de la ruta no coincide." });
        await Mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Elimina un programa (solo Administrador).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteProgramaCommand(id), ct);
        return NoContent();
    }
}
