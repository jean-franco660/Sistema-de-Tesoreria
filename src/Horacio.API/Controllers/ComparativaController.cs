using Horacio.Application.Features.Comparativa.DTOs;
using Horacio.Application.Features.Comparativa.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

/// <summary>
/// Comparativa de Ingresos (tickets) vs Egresos (comprobantes) para tesorería.
/// </summary>
[Authorize]
public class ComparativaController : ApiControllerBase
{
    /// <summary>Comparativa ingresos vs egresos entre dos fechas (opcionales).</summary>
    [HttpGet]
    public async Task<ActionResult<ComparativaDto>> Get(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetComparativaQuery(desde, hasta), ct));
}
