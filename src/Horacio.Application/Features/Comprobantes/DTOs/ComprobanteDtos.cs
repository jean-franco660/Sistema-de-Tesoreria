namespace Horacio.Application.Features.Comprobantes.DTOs;

/// <summary>Comprobante de egreso completo (vista de detalle).</summary>
public class ComprobanteDto
{
    public int Id { get; set; }
    public string? Proveedor { get; set; }
    public string? Ruc { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroComprobante { get; set; }
    public DateTime? FechaEmision { get; set; }
    public string? HoraEmision { get; set; }
    public string Moneda { get; set; } = "PEN";
    public decimal? Subtotal { get; set; }
    public decimal? Igv { get; set; }
    public decimal Total { get; set; }
    public string? Categoria { get; set; }
    public string? Concepto { get; set; }
    public string? MetodoPago { get; set; }
    public int Confianza { get; set; }
    public bool EsDuplicadoProbable { get; set; }
    public string? Observaciones { get; set; }
    public string? ImagenRuta { get; set; }
    public string? ImagenUrl { get; set; }
    public string? ImagenBase64 { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public List<ComprobanteProductoDto> Productos { get; set; } = new();
}

public class ComprobanteProductoDto
{
    public string? Descripcion { get; set; }
    public decimal? Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public decimal? Importe { get; set; }
}

/// <summary>Resumen de comprobante para listados.</summary>
public class ComprobanteListItemDto
{
    public int Id { get; set; }
    public string? Proveedor { get; set; }
    public string? Ruc { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroComprobante { get; set; }
    public DateTime? FechaEmision { get; set; }
    public DateTime FechaRegistro { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "PEN";
    public string? Categoria { get; set; }
    public string? Concepto { get; set; }
    public int Confianza { get; set; }
    public bool EsDuplicadoProbable { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
