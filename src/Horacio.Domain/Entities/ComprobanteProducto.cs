using Horacio.Domain.Common;

namespace Horacio.Domain.Entities;

/// <summary>
/// Línea de detalle (producto o servicio) de un <see cref="Comprobante"/> de egreso.
/// </summary>
public class ComprobanteProducto : BaseEntity
{
    public int ComprobanteId { get; set; }
    public Comprobante? Comprobante { get; set; }

    public string? Descripcion { get; set; }
    public decimal? Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public decimal? Importe { get; set; }
}
