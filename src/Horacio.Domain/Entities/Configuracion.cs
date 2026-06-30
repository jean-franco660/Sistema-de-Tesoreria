using Horacio.Domain.Common;

namespace Horacio.Domain.Entities;

/// <summary>
/// Parámetros institucionales del sistema (editables para rebranding / venta a otra institución).
/// Tabla de una sola fila.
/// </summary>
public class Configuracion : BaseEntity
{
    public string NombreInstitucion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string CodigoModular { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string BaseLegal { get; set; } = string.Empty;
    public string TituloComprobante { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = string.Empty;

    // ── Datos fijos del REGISTRO DE MATRÍCULA oficial (constantes institucionales) ──
    /// <summary>Dirección Regional de Educación (ej. "PUNO").</summary>
    public string DreGre { get; set; } = string.Empty;
    /// <summary>Unidad de Gestión Educativa Local (ej. "SAN ROMÁN").</summary>
    public string Ugel { get; set; } = string.Empty;
    /// <summary>Resolución de creación o autorización del CETPRO.</summary>
    public string ResolucionCreacion { get; set; } = string.Empty;
    /// <summary>Resolución de autorización del programa de estudios.</summary>
    public string ResolucionAutorizacion { get; set; } = string.Empty;
    /// <summary>Período/año lectivo (ej. "2026").</summary>
    public string PeriodoLectivo { get; set; } = string.Empty;
    /// <summary>Modalidad del servicio educativo (ej. "PRESENCIAL").</summary>
    public string ModalidadServicio { get; set; } = string.Empty;
    /// <summary>Nivel formativo (ej. "CICLO AUXILIAR TÉCNICO").</summary>
    public string NivelFormativo { get; set; } = string.Empty;
    /// <summary>Tipo de plan de estudios (ej. "POR COMPETENCIAS").</summary>
    public string TipoPlan { get; set; } = string.Empty;

    /// <summary>Logo institucional como data URL base64 (opcional). Si es null, se usa el logo por defecto.</summary>
    public string? LogoBase64 { get; set; }
}
