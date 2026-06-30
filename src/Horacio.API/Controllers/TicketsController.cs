using Horacio.Application.Features.Tickets.Commands;
using Horacio.Application.Features.Tickets.DTOs;
using Horacio.Application.Features.Tickets.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

[Authorize]
public class TicketsController : ApiControllerBase
{
    /// <summary>Emite un ticket (numeración automática + total + importe en letras).</summary>
    [HttpPost]
    public async Task<ActionResult<TicketDto>> Emitir(EmitirTicketCommand command, CancellationToken ct)
    {
        var ticket = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    /// <summary>Obtiene un ticket completo (para reimpresión).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDto>> GetById(int id, CancellationToken ct)
        => Ok(await Mediator.Send(new GetTicketByIdQuery(id), ct));

    /// <summary>Lista tickets, con filtro opcional por rango de fechas y/o DNI del alumno.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketListItemDto>>> Get(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string? dni, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetTicketsQuery(desde, hasta, dni), ct));
}
