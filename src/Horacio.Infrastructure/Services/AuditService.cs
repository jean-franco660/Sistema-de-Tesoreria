using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;

namespace Horacio.Infrastructure.Services;

/// <summary>Registra acciones de usuario en la tabla AuditLogs.</summary>
public class AuditService : IAuditService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;

    public AuditService(IUnitOfWork uow, ICurrentUserService currentUser, IDateTimeService dateTime)
    {
        _uow = uow;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task RegistrarAsync(string accion, string? detalle = null, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            Usuario = _currentUser.Username ?? "sistema",
            Fecha = _dateTime.UtcNow,
            Ip = _currentUser.IpAddress ?? "-",
            Accion = accion,
            Detalle = detalle
        };

        await _uow.Repository<AuditLog>().AddAsync(log, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
