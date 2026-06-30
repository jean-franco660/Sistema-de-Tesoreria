using Horacio.Application.Common.Interfaces;

namespace Horacio.Infrastructure.Services.Ocr;

/// <summary>
/// OCR simulado para desarrollo sin API key. Devuelve un texto de comprobante de ejemplo.
/// </summary>
public class MockOcrService : IOcrService
{
    public Task<string> ExtraerTextoAsync(string imagenBase64, CancellationToken ct = default)
    {
        const string ejemplo = """
LIBRERIA SAN MARTIN E.I.R.L.
RUC 20123456789
FACTURA ELECTRONICA
F001-12345
Fecha: 21/06/2026  Hora: 10:32

PAPEL BOND A4 x10        100.00
LAPICEROS PILOT x20       50.00

SUBTOTAL                 127.12
IGV (18%)                 22.88
TOTAL S/                 150.00

Forma de pago: EFECTIVO
""";
        return Task.FromResult(ejemplo);
    }
}
