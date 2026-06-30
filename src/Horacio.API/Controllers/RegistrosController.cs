using Horacio.Application.Features.Matricula.Queries;
using Horacio.Application.Features.Registros.Commands;
using Horacio.Application.Features.Registros.DTOs;
using Horacio.Application.Features.Registros.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize]
public class RegistrosController : ApiControllerBase
{
    /// <summary>Lista los registros de matrícula (aulas) de un período, con su conteo.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RegistroDto>>> Get(
        [FromQuery] int? periodoId, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetRegistrosQuery(periodoId), ct));

    /// <summary>Padrón oficial (encabezado + estudiantes) de un registro concreto.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MatriculaDto>> Roster(int id, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetRegistroRosterQuery(id), ct));

    /// <summary>Descarga el registro de matrícula en .xlsx (formato oficial).</summary>
    [HttpGet("{id:int}/excel")]
    public async Task<IActionResult> Excel(int id, CancellationToken ct = default)
    {
        var bytes = await Mediator.Send(new GetRegistroExcelQuery(id), ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"registro_matricula_{id}.xlsx");
    }

    /// <summary>Crea un registro de matrícula (normalmente vacío).</summary>
    [HttpPost]
    public async Task<ActionResult<int>> Create(CrearRegistroCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(Roster), new { id }, new { id });
    }

    /// <summary>Elimina un registro de matrícula vacío.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await Mediator.Send(new EliminarRegistroCommand(id), ct);
        return NoContent();
    }
}
