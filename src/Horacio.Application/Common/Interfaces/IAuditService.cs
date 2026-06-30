namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Registro de auditoría de acciones de usuario (tabla AuditLogs).
/// </summary>
public interface IAuditService
{
    Task RegistrarAsync(string accion, string? detalle = null, CancellationToken ct = default);
}
