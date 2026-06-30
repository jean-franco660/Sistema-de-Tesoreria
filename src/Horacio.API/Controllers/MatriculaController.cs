using Horacio.Application.Features.Matricula.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize]
public class MatriculaController : ApiControllerBase
{
    /// <summary>Padrón oficial de matrícula por programa, turno y sección (período activo por defecto).</summary>
    [HttpGet]
    public async Task<ActionResult<MatriculaDto>> Get(
        [FromQuery] int programaId, [FromQuery] int turnoId,
        [FromQuery] string? seccion, [FromQuery] int? periodoId, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetMatriculaQuery(programaId, turnoId, seccion, periodoId), ct));

    /// <summary>Combinaciones matriculables existentes (programa + turno + sección) con su conteo.</summary>
    [HttpGet("opciones")]
    public async Task<ActionResult<IReadOnlyList<MatriculaOpcionDto>>> Opciones(
        [FromQuery] int? periodoId, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetMatriculaOpcionesQuery(periodoId), ct));
}
