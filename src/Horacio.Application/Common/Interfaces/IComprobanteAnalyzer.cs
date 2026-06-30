using Horacio.Application.Features.Comprobantes.DTOs;

namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Motor de análisis de comprobantes peruanos. Recibe el TEXTO (ya extraído por
/// OCR) y devuelve los datos estructurados. Implementación por defecto: DeepSeek.
/// Intercambiable por configuración.
/// </summary>
public interface IComprobanteAnalyzer
{
    /// <summary>
    /// Analiza el texto OCR de un comprobante y devuelve el JSON estructurado.
    /// </summary>
    Task<AnalisisComprobanteDto> AnalizarTextoAsync(string textoOcr, CancellationToken ct = default);
}
