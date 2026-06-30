using Horacio.Domain.Common;
using Horacio.Domain.Enums;

namespace Horacio.Domain.Entities;

/// <summary>
/// Comprobante de EGRESO (gasto): factura, boleta, ticket, recibo, voucher, etc.
/// Capturado por la app móvil y analizado por IA (DeepSeek) a partir de una foto.
/// Es la contraparte de <see cref="Ticket"/> (ingreso) para la comparativa de tesorería.
/// </summary>
public class Comprobante : BaseEntity
{
    // ── Datos extraídos por la IA ─────────────────────────────────────────────
    /// <summary>Nombre comercial o razón social del proveedor.</summary>
    public string? Proveedor { get; set; }

    /// <summary>RUC del proveedor (solo dígitos).</summary>
    public string? Ruc { get; set; }

    /// <summary>Factura · Boleta · Ticket · Recibo · Voucher · Nota de venta · Otro.</summary>
    public string? TipoDocumento { get; set; }

    /// <summary>Serie y correlativo del documento (ej. "F001-00123").</summary>
    public string? NumeroComprobante { get; set; }

    /// <summary>Fecha de emisión del comprobante (la que figura en el documento).</summary>
    public DateTime? FechaEmision { get; set; }

    /// <summary>Hora de emisión si existe (texto, ej. "14:35").</summary>
    public string? HoraEmision { get; set; }

    /// <summary>Moneda: PEN o USD.</summary>
    public string Moneda { get; set; } = "PEN";

    public decimal? Subtotal { get; set; }
    public decimal? Igv { get; set; }

    /// <summary>Importe final pagado.</summary>
    public decimal Total { get; set; }

    /// <summary>Categoría del gasto (Combustible, Alimentacion, Materiales, …).</summary>
    public string? Categoria { get; set; }

    /// <summary>Descripción corta del gasto generada por la IA.</summary>
    public string? Concepto { get; set; }

    /// <summary>Efectivo · Tarjeta · Transferencia · Yape · Plin · Otro · null.</summary>
    public string? MetodoPago { get; set; }

    /// <summary>Confianza de la extracción (0–100).</summary>
    public int Confianza { get; set; }

    /// <summary>La IA detectó que podría ser una reimpresión / duplicado.</summary>
    public bool EsDuplicadoProbable { get; set; }

    /// <summary>Observaciones relevantes detectadas por la IA.</summary>
    public string? Observaciones { get; set; }

    // ── Metadatos del sistema ─────────────────────────────────────────────────
    /// <summary>Ruta/nombre del archivo de la foto guardada en el servidor (Droplet).</summary>
    public string? ImagenRuta { get; set; }

    /// <summary>URL pública de la foto (si el almacenamiento la expone).</summary>
    public string? ImagenUrl { get; set; }

    /// <summary>Foto del comprobante como data URL base64 (opcional, p. ej. registro manual web).</summary>
    public string? ImagenBase64 { get; set; }

    /// <summary>JSON crudo devuelto por la IA (para trazabilidad).</summary>
    public string? RespuestaIaJson { get; set; }

    /// <summary>Fecha/hora en que se registró en el sistema (UTC).</summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    /// <summary>Usuario que registró el egreso.</summary>
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    /// <summary>Período académico al que pertenece el egreso (igual que los ingresos).</summary>
    public int? PeriodoAcademicoId { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }

    public EstadoComprobante Estado { get; set; } = EstadoComprobante.Registrado;

    // ── Detalle de productos / servicios del comprobante ──────────────────────
    public ICollection<ComprobanteProducto> Productos { get; set; } = new List<ComprobanteProducto>();
}
