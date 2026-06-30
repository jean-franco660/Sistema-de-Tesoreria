using FluentValidation;
using Horacio.Application.Common.Interfaces;
using DomainConfig = Horacio.Domain.Entities.Configuracion;
using MediatR;

namespace Horacio.Application.Features.Configuracion.Commands;

/// <summary>Actualiza la configuración institucional (rebranding).</summary>
public record UpdateConfiguracionCommand(
    string NombreInstitucion,
    string Ciudad,
    string CodigoModular,
    string Direccion,
    string BaseLegal,
    string TituloComprobante,
    string TipoComprobante,
    string? LogoBase64,
    string? DreGre = null,
    string? Ugel = null,
    string? ResolucionCreacion = null,
    string? ResolucionAutorizacion = null,
    string? PeriodoLectivo = null,
    string? ModalidadServicio = null,
    string? NivelFormativo = null,
    string? TipoPlan = null) : IRequest<Unit>;

public class UpdateConfiguracionCommandValidator : AbstractValidator<UpdateConfiguracionCommand>
{
    public UpdateConfiguracionCommandValidator()
    {
        RuleFor(x => x.NombreInstitucion).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Ciudad).MaximumLength(150);
        RuleFor(x => x.CodigoModular).MaximumLength(50);
    }
}

public class UpdateConfiguracionCommandHandler : IRequestHandler<UpdateConfiguracionCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public UpdateConfiguracionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(UpdateConfiguracionCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<DomainConfig>();
        var config = (await repo.ListAllAsync(cancellationToken)).FirstOrDefault();

        if (config is null)
        {
            config = new DomainConfig();
            await repo.AddAsync(config, cancellationToken);
        }

        config.NombreInstitucion = request.NombreInstitucion.Trim();
        config.Ciudad = request.Ciudad.Trim();
        config.CodigoModular = request.CodigoModular.Trim();
        config.Direccion = request.Direccion.Trim();
        config.BaseLegal = request.BaseLegal.Trim();
        config.TituloComprobante = request.TituloComprobante.Trim();
        config.TipoComprobante = request.TipoComprobante.Trim();
        config.LogoBase64 = string.IsNullOrWhiteSpace(request.LogoBase64) ? config.LogoBase64 : request.LogoBase64;

        // Datos fijos del registro: solo se actualizan si se envían (no se borran).
        if (request.DreGre is not null) config.DreGre = request.DreGre.Trim();
        if (request.Ugel is not null) config.Ugel = request.Ugel.Trim();
        if (request.ResolucionCreacion is not null) config.ResolucionCreacion = request.ResolucionCreacion.Trim();
        if (request.ResolucionAutorizacion is not null) config.ResolucionAutorizacion = request.ResolucionAutorizacion.Trim();
        if (request.PeriodoLectivo is not null) config.PeriodoLectivo = request.PeriodoLectivo.Trim();
        if (request.ModalidadServicio is not null) config.ModalidadServicio = request.ModalidadServicio.Trim();
        if (request.NivelFormativo is not null) config.NivelFormativo = request.NivelFormativo.Trim();
        if (request.TipoPlan is not null) config.TipoPlan = request.TipoPlan.Trim();

        repo.Update(config);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
