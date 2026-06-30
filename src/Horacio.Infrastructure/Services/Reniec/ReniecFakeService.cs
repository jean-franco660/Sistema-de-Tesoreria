using Horacio.Application.Common.Interfaces;
using Horacio.Application.Common.Models;

namespace Horacio.Infrastructure.Services.Reniec;

/// <summary>
/// Proveedor RENIEC simulado para desarrollo/pruebas. Genera datos
/// deterministas a partir del DNI (sin llamadas externas).
/// </summary>
public class ReniecFakeService : IReniecService
{
    private static readonly string[] Nombres =
        { "JUAN CARLOS", "MARIA ELENA", "JOSE LUIS", "ANA LUCIA", "PEDRO PABLO",
          "ROSA MARIA", "LUIS ALBERTO", "CARMEN ROSA", "MIGUEL ANGEL", "SANDRA PAOLA" };

    private static readonly string[] ApellidosPaternos =
        { "QUISPE", "MAMANI", "FLORES", "CONDORI", "HUANCA",
          "APAZA", "CHOQUE", "TICONA", "CCAPA", "VILCA" };

    private static readonly string[] ApellidosMaternos =
        { "GAMEZ", "ZEBALLOS", "CALLA", "CHAMBI", "POMA",
          "SUCASO", "LARICO", "CRUZ", "AROHUANCA", "JOVE" };

    public Task<ReniecPersona?> ConsultarDniAsync(string dni, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dni) || dni.Length != 8 || !dni.All(char.IsDigit))
            return Task.FromResult<ReniecPersona?>(null);

        int semilla = dni.Sum(c => c - '0');

        var persona = new ReniecPersona
        {
            Dni = dni,
            Nombres = Nombres[semilla % Nombres.Length],
            ApellidoPaterno = ApellidosPaternos[(semilla / 2) % ApellidosPaternos.Length],
            ApellidoMaterno = ApellidosMaternos[(semilla / 3) % ApellidosMaternos.Length]
        };

        return Task.FromResult<ReniecPersona?>(persona);
    }
}
