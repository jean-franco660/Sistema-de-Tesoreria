namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Convierte importes numéricos a su representación en letras (español).
/// Ej.: 15.00 -> "QUINCE CON 00/100 SOLES".
/// </summary>
public interface INumberToWordsService
{
    string ConvertirImporte(decimal importe);
}
