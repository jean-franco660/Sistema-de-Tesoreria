using Horacio.Application.Features.Turnos.DTOs;
using Horacio.Application.Features.Turnos.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize]
public class TurnosController : ApiControllerBase
{
    /// <summary>Lista el catálogo de turnos (MAÑANA, TARDE, NOCHE).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TurnoDto>>> Get(CancellationToken ct)
        => Ok(await Mediator.Send(new GetTurnosQuery(), ct));
}
