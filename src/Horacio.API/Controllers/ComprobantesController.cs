using Horacio.Application.Features.Comprobantes.Commands;
using Horacio.Application.Features.Comprobantes.DTOs;
using Horacio.Application.Features.Comprobantes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

/// <summary>
/// Egresos: registro de comprobantes (facturas, boletas, tickets…) capturados por
/// la app móvil y procesados con OCR + IA, además de su consulta.
/// </summary>
[Authorize]
public class ComprobantesController : ApiControllerBase
{
    /// <summary>
    /// Tubería completa (app móvil): sube la foto del comprobante y la procesa
    /// (guardar → OCR → IA → validar → dedupe → registrar). Devuelve el egreso creado.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(20_000_000)] // 20 MB
    public async Task<ActionResult<ComprobanteDto>> Procesar(IFormFile imagen, CancellationToken ct)
    {
        if (imagen is null || imagen.Length == 0)
            return BadRequest(new { mensaje = "Debe adjuntar la foto del comprobante (campo 'imagen')." });

        using var ms = new MemoryStream();
        await imagen.CopyToAsync(ms, ct);
        var ext = Path.GetExtension(imagen.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

        var dto = await Mediator.Send(new ProcesarComprobanteCommand(ms.ToArray(), ext), ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>Solo analiza una imagen (base64) y devuelve el JSON, sin guardar (vista previa).</summary>
    [HttpPost("analizar")]
    public async Task<ActionResult<AnalisisComprobanteDto>> Analizar([FromBody] AnalizarRequest body, CancellationToken ct)
        => Ok(await Mediator.Send(new AnalizarComprobanteCommand(body.ImagenBase64), ct));

    /// <summary>Registro manual (web): guarda un egreso con los datos ya revisados/corregidos.</summary>
    [HttpPost("manual")]
    public async Task<ActionResult<ComprobanteDto>> RegistrarManual([FromBody] RegistrarComprobanteCommand command, CancellationToken ct)
    {
        var dto = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>Lista egresos (filtros opcionales por fecha y categoría).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ComprobanteListItemDto>>> Get(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string? categoria, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetComprobantesQuery(desde, hasta, categoria), ct));

    /// <summary>Detalle completo de un egreso.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ComprobanteDto>> GetById(int id, CancellationToken ct)
        => Ok(await Mediator.Send(new GetComprobanteByIdQuery(id), ct));
}

/// <summary>Cuerpo para el análisis por base64 (vista previa web).</summary>
public record AnalizarRequest(string ImagenBase64);
