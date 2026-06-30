using System.Globalization;
using System.Text.RegularExpressions;
using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Comprobantes.DTOs;

namespace Horacio.Infrastructure.Services.Ia;

/// <summary>
/// Analizador simulado para desarrollo sin API key. Aplica heurísticas simples
/// sobre el texto OCR (proveedor, RUC, total, fecha) para probar el flujo completo.
/// </summary>
public class MockComprobanteAnalyzer : IComprobanteAnalyzer
{
    public Task<AnalisisComprobanteDto> AnalizarTextoAsync(string textoOcr, CancellationToken ct = default)
    {
        var lineas = textoOcr.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var dto = new AnalisisComprobanteDto
        {
            Proveedor = lineas.FirstOrDefault(),
            Ruc = Regex.Match(textoOcr, @"\b(\d{11})\b").Groups[1].Value is { Length: 11 } ruc ? ruc : null,
            Total = BuscarTotal(textoOcr),
            FechaEmision = BuscarFecha(textoOcr),
            Moneda = "PEN",
            Categoria = "Otros",
            Concepto = "Gasto registrado (modo demo sin IA real)",
            MetodoPago = textoOcr.Contains("EFECTIVO", StringComparison.OrdinalIgnoreCase) ? "Efectivo" : null,
            TipoDocumento = textoOcr.Contains("FACTURA", StringComparison.OrdinalIgnoreCase) ? "Factura"
                          : textoOcr.Contains("BOLETA", StringComparison.OrdinalIgnoreCase) ? "Boleta" : "Ticket",
            Confianza = 40,
            EsDuplicadoProbable = false,
            Observaciones = "Generado por el analizador DEMO (configure DeepSeek para resultados reales).",
            RespuestaIaJson = "{\"_mock\":true}"
        };

        return Task.FromResult(dto);
    }

    private static decimal? BuscarTotal(string texto)
    {
        var m = Regex.Match(texto, @"TOTAL\D*?(\d+[.,]\d{2})", RegexOptions.IgnoreCase);
        if (m.Success && decimal.TryParse(m.Groups[1].Value.Replace(',', '.'),
            NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
        return null;
    }

    private static string? BuscarFecha(string texto)
    {
        var m = Regex.Match(texto, @"(\d{2})[/-](\d{2})[/-](\d{4})");
        if (m.Success) return $"{m.Groups[3].Value}-{m.Groups[2].Value}-{m.Groups[1].Value}";
        var iso = Regex.Match(texto, @"(\d{4})-(\d{2})-(\d{2})");
        return iso.Success ? iso.Value : null;
    }
}
