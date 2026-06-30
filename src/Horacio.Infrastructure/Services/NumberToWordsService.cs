using System.Text;
using Horacio.Application.Common.Interfaces;

namespace Horacio.Infrastructure.Services;

/// <summary>
/// Convierte importes a letras en español (sin tildes, en mayúsculas) para soles.
/// Ej.: 15.00 -> "QUINCE CON 00/100 SOLES".
/// Soporta de 0 hasta 999 999 999.99.
/// </summary>
public class NumberToWordsService : INumberToWordsService
{
    private static readonly string[] Unidades =
        { "", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };

    private static readonly string[] Especiales =
        { "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE",
          "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE" };

    private static readonly string[] Decenas =
        { "", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA",
          "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };

    private static readonly string[] Centenas =
        { "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS",
          "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };

    public string ConvertirImporte(decimal importe)
    {
        if (importe < 0) importe = Math.Abs(importe);

        long entero = (long)decimal.Truncate(importe);
        int centavos = (int)Math.Round((importe - entero) * 100m, MidpointRounding.AwayFromZero);

        // El redondeo de centavos puede arrastrar una unidad (p. ej. 0.999 -> 1.00).
        if (centavos == 100)
        {
            entero += 1;
            centavos = 0;
        }

        string letras = ConvertirEntero(entero);
        return $"{letras} CON {centavos:00}/100 SOLES";
    }

    private static string ConvertirEntero(long n)
    {
        if (n == 0) return "CERO";

        long millones = n / 1_000_000;
        long resto = n % 1_000_000;
        long miles = resto / 1000;
        long cientos = resto % 1000;

        var sb = new StringBuilder();

        if (millones > 0)
        {
            if (millones == 1)
                sb.Append("UN MILLON");
            else
                sb.Append(Apocopar(Grupo((int)millones))).Append(" MILLONES");
        }

        if (miles > 0)
        {
            if (sb.Length > 0) sb.Append(' ');
            if (miles == 1)
                sb.Append("MIL");
            else
                sb.Append(Apocopar(Grupo((int)miles))).Append(" MIL");
        }

        if (cientos > 0)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(Grupo((int)cientos));
        }

        return sb.ToString().Trim();
    }

    /// <summary>Convierte un grupo de 1 a 999 a palabras.</summary>
    private static string Grupo(int n)
    {
        if (n == 0) return string.Empty;
        if (n == 100) return "CIEN";

        int c = n / 100;
        int resto = n % 100;

        var sb = new StringBuilder();
        if (c > 0) sb.Append(Centenas[c]);

        if (resto > 0)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(DecenasUnidades(resto));
        }

        return sb.ToString().Trim();
    }

    private static string DecenasUnidades(int n)
    {
        if (n < 10) return Unidades[n];
        if (n < 20) return Especiales[n - 10];
        if (n < 30) return n == 20 ? "VEINTE" : "VEINTI" + Unidades[n - 20];

        int d = n / 10;
        int u = n % 10;
        return u == 0 ? Decenas[d] : $"{Decenas[d]} Y {Unidades[u]}";
    }

    /// <summary>Apócope de "UNO"/"VEINTIUNO" -> "UN"/"VEINTIUN" antes de MIL/MILLONES.</summary>
    private static string Apocopar(string grupo)
        => grupo.EndsWith("UNO", StringComparison.Ordinal) ? grupo[..^1] : grupo;
}
