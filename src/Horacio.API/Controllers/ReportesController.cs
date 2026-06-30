using Horacio.Application.Features.Reportes.DTOs;
using Horacio.Application.Features.Reportes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize]
public class ReportesController : ApiControllerBase
{
    /// <summary>Informe de ingresos por recursos propios (contabilidad) entre fechas.</summary>
    [HttpGet("ingresos")]
    public async Task<ActionResult<ReporteIngresosDto>> Ingresos(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetReporteIngresosQuery(desde, hasta), ct));
}
