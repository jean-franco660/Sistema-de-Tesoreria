using Horacio.Application.Common.Interfaces;
using DomainConfig = Horacio.Domain.Entities.Configuracion;
using MediatR;

namespace Horacio.Application.Features.Configuracion.Queries;

public class ConfiguracionDto
{
    public int Id { get; set; }
    public string NombreInstitucion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string CodigoModular { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string BaseLegal { get; set; } = string.Empty;
    public string TituloComprobante { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = string.Empty;
    public string DreGre { get; set; } = string.Empty;
    public string Ugel { get; set; } = string.Empty;
    public string ResolucionCreacion { get; set; } = string.Empty;
    public string ResolucionAutorizacion { get; set; } = string.Empty;
    public string PeriodoLectivo { get; set; } = string.Empty;
    public string ModalidadServicio { get; set; } = string.Empty;
    public string NivelFormativo { get; set; } = string.Empty;
    public string TipoPlan { get; set; } = string.Empty;
    public string? LogoBase64 { get; set; }
}

/// <summary>Obtiene la configuración institucional (única fila).</summary>
public record GetConfiguracionQuery : IRequest<ConfiguracionDto>;

public class GetConfiguracionQueryHandler : IRequestHandler<GetConfiguracionQuery, ConfiguracionDto>
{
    private readonly IUnitOfWork _uow;

    public GetConfiguracionQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ConfiguracionDto> Handle(GetConfiguracionQuery request, CancellationToken cancellationToken)
    {
        var c = (await _uow.Repository<DomainConfig>().ListAllAsync(cancellationToken)).FirstOrDefault()
            ?? new DomainConfig();

        return new ConfiguracionDto
        {
            Id = c.Id,
            NombreInstitucion = c.NombreInstitucion,
            Ciudad = c.Ciudad,
            CodigoModular = c.CodigoModular,
            Direccion = c.Direccion,
            BaseLegal = c.BaseLegal,
            TituloComprobante = c.TituloComprobante,
            TipoComprobante = c.TipoComprobante,
            DreGre = c.DreGre,
            Ugel = c.Ugel,
            ResolucionCreacion = c.ResolucionCreacion,
            ResolucionAutorizacion = c.ResolucionAutorizacion,
            PeriodoLectivo = c.PeriodoLectivo,
            ModalidadServicio = c.ModalidadServicio,
            NivelFormativo = c.NivelFormativo,
            TipoPlan = c.TipoPlan,
            LogoBase64 = c.LogoBase64
        };
    }
}
