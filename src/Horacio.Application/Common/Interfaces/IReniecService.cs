using Horacio.Application.Common.Models;

namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Abstracción del proveedor de consulta RENIEC.
/// Permite cambiar de proveedor (API real, mock u otro servicio) en el futuro
/// sin modificar la lógica de aplicación.
/// </summary>
public interface IReniecService
{
    /// <summary>Consulta los datos de una persona por DNI. Devuelve null si no se encuentra.</summary>
    Task<ReniecPersona?> ConsultarDniAsync(string dni, CancellationToken ct = default);
}
