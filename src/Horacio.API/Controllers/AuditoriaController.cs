using Horacio.Application.Features.Auditoria.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize(Roles = "Administrador")]
public class AuditoriaController : ApiControllerBase
{
    /// <summary>Últimos registros de auditoría (solo Administrador).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> Get([FromQuery] int top = 200, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetAuditLogsQuery(top), ct));
}
