using Horacio.Domain.Common;

namespace Horacio.Domain.Entities;

/// <summary>
/// Registro de auditoría: usuario, fecha/hora, IP y acción realizada.
/// </summary>
public class AuditLog : BaseEntity
{
    public string Usuario { get; set; } = string.Empty;

    /// <summary>Fecha y hora del evento (incluye ambos componentes).</summary>
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public string Ip { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string? Detalle { get; set; }
}
