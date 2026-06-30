using Horacio.Domain.Common;

namespace Horacio.Domain.Entities;

/// <summary>
/// Contador persistente para la numeración automática.
/// Se usan dos contadores lógicos:
///  - "GENERAL": correlativo global del comprobante  -> "000000001".
///  - "TICKET" : correlativo por serie del ticket     -> "001/001".
/// </summary>
public class Contador : BaseEntity
{
    /// <summary>Identificador lógico del contador: "GENERAL" o "TICKET".</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Serie actual (solo aplica a la numeración de tickets), ej. "001".</summary>
    public string Serie { get; set; } = "001";

    /// <summary>Último valor emitido (correlativo).</summary>
    public long UltimoValor { get; set; }
}
