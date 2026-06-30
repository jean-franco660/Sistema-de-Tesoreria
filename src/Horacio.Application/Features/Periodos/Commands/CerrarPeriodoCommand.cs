using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Periodos.DTOs;
using Horacio.Application.Features.Periodos.Queries;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Periodos.Commands;

/// <summary>Cierra definitivamente un período académico y devuelve el resumen (acta).</summary>
public record CerrarPeriodoCommand(int Id) : IRequest<PeriodoResumenDto>;

public class CerrarPeriodoCommandHandler : IRequestHandler<CerrarPeriodoCommand, PeriodoResumenDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CerrarPeriodoCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PeriodoResumenDto> Handle(CerrarPeriodoCommand request, CancellationToken cancellationToken)
    {
        var periodo = await _uow.Repository<PeriodoAcademico>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Período", request.Id);

        if (periodo.Estado != EstadoPeriodo.Abierto)
            throw new DomainException("El período ya está cerrado.");

        periodo.Estado = EstadoPeriodo.Cerrado;
        periodo.UsuarioCierre = _currentUser.Username ?? "sistema";
        periodo.FechaCierre = DateTime.UtcNow;
        _uow.Repository<PeriodoAcademico>().Update(periodo);
        await _uow.SaveChangesAsync(cancellationToken);

        return await ResumenHelper.CalcularAsync(_uow, periodo, cancellationToken);
    }
}
