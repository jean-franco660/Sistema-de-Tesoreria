using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using MediatR;

namespace Horacio.Application.Features.Auditoria.Queries;

public class AuditLogDto
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string? Detalle { get; set; }
}

/// <summary>Lista los últimos registros de auditoría.</summary>
public record GetAuditLogsQuery(int Top = 200) : IRequest<IReadOnlyList<AuditLogDto>>;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;

    public GetAuditLogsQueryHandler(IUnitOfWork uow, IDateTimeService dateTime)
    {
        _uow = uow;
        _dateTime = dateTime;
    }

    public async Task<IReadOnlyList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _uow.Repository<AuditLog>().ListAllAsync(cancellationToken);
        return logs
            .OrderByDescending(l => l.Id)
            .Take(request.Top)
            .Select(l => new AuditLogDto
            {
                Id = l.Id,
                Usuario = l.Usuario,
                Fecha = _dateTime.ToLocal(l.Fecha),
                Ip = l.Ip,
                Accion = l.Accion,
                Detalle = l.Detalle
            })
            .ToList();
    }
}
