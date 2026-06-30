using Horacio.Application.Features.Periodos.Commands;
using Horacio.Application.Features.Periodos.DTOs;
using Horacio.Application.Features.Periodos.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize]
public class PeriodosController : ApiControllerBase
{
    /// <summary>Histórico de períodos académicos.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PeriodoDto>>> Get(CancellationToken ct)
        => Ok(await Mediator.Send(new GetPeriodosQuery(), ct));

    /// <summary>Período académico ABIERTO actual (o null).</summary>
    [HttpGet("activo")]
    public async Task<ActionResult<PeriodoDto?>> Activo(CancellationToken ct)
        => Ok(await Mediator.Send(new GetPeriodoActivoQuery(), ct));

    /// <summary>Sección de gestión financiera por período (dashboard).</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardPeriodoDto>> Dashboard(CancellationToken ct)
        => Ok(await Mediator.Send(new GetDashboardPeriodoQuery(), ct));

    /// <summary>Resumen de un período (para cierre / acta).</summary>
    [HttpGet("{id:int}/resumen")]
    public async Task<ActionResult<PeriodoResumenDto>> Resumen(int id, CancellationToken ct)
        => Ok(await Mediator.Send(new GetPeriodoResumenQuery(id), ct));

    /// <summary>Detalle completo de ingresos del período (lista + resúmenes, descargable).</summary>
    [HttpGet("{id:int}/detalle")]
    public async Task<ActionResult<PeriodoDetalleDto>> Detalle(int id, CancellationToken ct)
        => Ok(await Mediator.Send(new GetPeriodoDetalleQuery(id), ct));

    /// <summary>Abre un nuevo período (solo Administrador).</summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<int>> Abrir(AbrirPeriodoCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Cierra un período y devuelve el acta-resumen (solo Administrador).</summary>
    [HttpPut("{id:int}/cerrar")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<PeriodoResumenDto>> Cerrar(int id, CancellationToken ct)
        => Ok(await Mediator.Send(new CerrarPeriodoCommand(id), ct));
}
