namespace Horacio.Application.Features.Comprobantes.DTOs;

/// <summary>
/// Resultado del análisis de un comprobante (lo que devuelve la IA, ya parseado).
/// Refleja exactamente el JSON del prompt del motor de extracción.
/// </summary>
public class AnalisisComprobanteDto
{
    public string? Proveedor { get; set; }
    public string? Ruc { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroComprobante { get; set; }

    /// <summary>Fecha de emisión en formato YYYY-MM-DD (texto, tal como la devuelve la IA).</summary>
    public string? FechaEmision { get; set; }
    public string? HoraEmision { get; set; }

    public string Moneda { get; set; } = "PEN";
    public decimal? Subtotal { get; set; }
    public decimal? Igv { get; set; }
    public decimal? Total { get; set; }

    public string? Categoria { get; set; }
    public string? Concepto { get; set; }
    public string? MetodoPago { get; set; }

    public int Confianza { get; set; }
    public bool EsDuplicadoProbable { get; set; }
    public string? Observaciones { get; set; }

    public List<AnalisisProductoDto> Productos { get; set; } = new();

    // ── Metadatos (no provienen de la IA; los rellena el backend) ─────────────
    /// <summary>Texto plano extraído por el OCR (para trazabilidad / depuración).</summary>
    public string? TextoOcr { get; set; }

    /// <summary>JSON crudo devuelto por la IA.</summary>
    public string? RespuestaIaJson { get; set; }
}

public class AnalisisProductoDto
{
    public string? Descripcion { get; set; }
    public decimal? Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public decimal? Importe { get; set; }
}
