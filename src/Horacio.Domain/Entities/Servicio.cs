using Horacio.Domain.Common;
using Horacio.Domain.Enums;

namespace Horacio.Domain.Entities;

/// <summary>
/// Servicio cobrable por tesorería (constancias, certificados, carnet, etc.).
/// El importe NO es fijo: se ingresa por cada línea de ticket.
/// </summary>
public class Servicio : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Precio sugerido del servicio (editable por ticket).</summary>
    public decimal Precio { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    // Relaciones
    public ICollection<DetalleTicket> Detalles { get; set; } = new List<DetalleTicket>();
}
