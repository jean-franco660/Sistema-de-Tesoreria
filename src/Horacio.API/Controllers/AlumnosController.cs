using Horacio.Application.Features.Alumnos.Commands;
using Horacio.Application.Features.Alumnos.DTOs;
using Horacio.Application.Features.Alumnos.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize]
public class AlumnosController : ApiControllerBase
{
    /// <summary>Lista alumnos (filtro opcional por DNI/nombre).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AlumnoDto>>> Get(
        [FromQuery] string? buscar = null, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetAlumnosQuery(buscar), ct));

    /// <summary>
    /// Consulta automática por DNI: BD local y, si no existe, RENIEC.
    /// El cliente la invoca al completar los 8 dígitos (sin botón).
    /// </summary>
    [HttpGet("consulta-dni/{dni}")]
    public async Task<ActionResult<ConsultaDniResult>> ConsultarDni(string dni, CancellationToken ct)
        => Ok(await Mediator.Send(new BuscarAlumnoPorDniQuery(dni), ct));

    /// <summary>Registra un alumno (programa y turno seleccionados).</summary>
    [HttpPost]
    public async Task<ActionResult<int>> Create(CrearAlumnoCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }
}
