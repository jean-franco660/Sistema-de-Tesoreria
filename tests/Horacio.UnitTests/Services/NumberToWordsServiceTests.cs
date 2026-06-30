using FluentAssertions;
using Horacio.Infrastructure.Services;

namespace Horacio.UnitTests.Services;

public class NumberToWordsServiceTests
{
    private readonly NumberToWordsService _sut = new();

    [Theory]
    [InlineData(15.00, "QUINCE CON 00/100 SOLES")]            // ejemplo de la especificación
    [InlineData(0.00, "CERO CON 00/100 SOLES")]
    [InlineData(1.50, "UNO CON 50/100 SOLES")]
    [InlineData(16.00, "DIECISEIS CON 00/100 SOLES")]
    [InlineData(21.00, "VEINTIUNO CON 00/100 SOLES")]
    [InlineData(100.00, "CIEN CON 00/100 SOLES")]
    [InlineData(101.00, "CIENTO UNO CON 00/100 SOLES")]
    [InlineData(999.99, "NOVECIENTOS NOVENTA Y NUEVE CON 99/100 SOLES")]
    [InlineData(1000.00, "MIL CON 00/100 SOLES")]
    [InlineData(2000.00, "DOS MIL CON 00/100 SOLES")]
    [InlineData(21000.00, "VEINTIUN MIL CON 00/100 SOLES")]
    [InlineData(2500.75, "DOS MIL QUINIENTOS CON 75/100 SOLES")]
    [InlineData(100000.00, "CIEN MIL CON 00/100 SOLES")]
    [InlineData(1000000.00, "UN MILLON CON 00/100 SOLES")]
    [InlineData(2000000.00, "DOS MILLONES CON 00/100 SOLES")]
    public void ConvertirImporte_DevuelveLetrasEsperadas(decimal importe, string esperado)
    {
        _sut.ConvertirImporte(importe).Should().Be(esperado);
    }

    [Fact]
    public void ConvertirImporte_RedondeaCentavosHaciaArriba()
    {
        // 0.999 -> 1.00
        _sut.ConvertirImporte(0.999m).Should().Be("UNO CON 00/100 SOLES");
    }

    [Fact]
    public void ConvertirImporte_ConValorNegativo_UsaValorAbsoluto()
    {
        _sut.ConvertirImporte(-15.00m).Should().Be("QUINCE CON 00/100 SOLES");
    }
}
