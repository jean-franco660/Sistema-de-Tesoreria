namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Provee la fecha/hora actual (facilita pruebas y maneja la zona horaria de Perú).
/// </summary>
public interface IDateTimeService
{
    /// <summary>Hora local de Perú (America/Lima, UTC-5).</summary>
    DateTime Now { get; }

    DateTime UtcNow { get; }

    /// <summary>Convierte una fecha/hora UTC a la hora local de Perú.</summary>
    DateTime ToLocal(DateTime utcDateTime);
}
