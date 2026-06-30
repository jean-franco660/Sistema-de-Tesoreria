using Horacio.Application.Features.Usuarios.Commands;
using Horacio.Application.Features.Usuarios.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : ApiControllerBase
{
    /// <summary>Lista los usuarios del sistema.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> Get(CancellationToken ct)
        => Ok(await Mediator.Send(new GetUsuariosQuery(), ct));

    /// <summary>Crea un usuario.</summary>
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateUsuarioCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Activa o desactiva un usuario.</summary>
    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct)
    {
        await Mediator.Send(new ToggleUsuarioCommand(id), ct);
        return NoContent();
    }

    /// <summary>Elimina un usuario.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteUsuarioCommand(id), ct);
        return NoContent();
    }
}
