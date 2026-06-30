using Horacio.Application.Features.Dashboard.DTOs;
using Horacio.Application.Features.Dashboard.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize]
public class DashboardController : ApiControllerBase
{
    /// <summary>Métricas del panel principal (recaudación, conteos, rankings, últimos tickets).</summary>
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
        => Ok(await Mediator.Send(new GetDashboardQuery(), ct));
}
