using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Servicios.Commands;

/// <summary>Elimina un servicio (si no fue usado en tickets).</summary>
public record DeleteServicioCommand(int Id) : IRequest<Unit>;

public class DeleteServicioCommandHandler : IRequestHandler<DeleteServicioCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public DeleteServicioCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(DeleteServicioCommand request, CancellationToken cancellationToken)
    {
        var servicio = await _uow.Repository<Servicio>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Servicio", request.Id);

        if (await _uow.Repository<DetalleTicket>().AnyAsync(d => d.ServicioId == request.Id, cancellationToken))
            throw new DomainException("No se puede eliminar: el servicio ya fue cobrado en tickets. Desactívelo en su lugar.");

        _uow.Repository<Servicio>().Remove(servicio);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
