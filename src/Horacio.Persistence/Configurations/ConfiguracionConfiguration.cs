using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class ConfiguracionConfiguration : IEntityTypeConfiguration<Configuracion>
{
    public void Configure(EntityTypeBuilder<Configuracion> builder)
    {
        builder.ToTable("configuracion");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.NombreInstitucion).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Ciudad).HasMaxLength(150);
        builder.Property(c => c.CodigoModular).HasMaxLength(50);
        builder.Property(c => c.Direccion).HasMaxLength(200);
        builder.Property(c => c.BaseLegal).HasMaxLength(150);
        builder.Property(c => c.TituloComprobante).HasMaxLength(200);
        builder.Property(c => c.TipoComprobante).HasMaxLength(100);
        builder.Property(c => c.DreGre).HasMaxLength(100);
        builder.Property(c => c.Ugel).HasMaxLength(100);
        builder.Property(c => c.ResolucionCreacion).HasMaxLength(200);
        builder.Property(c => c.ResolucionAutorizacion).HasMaxLength(200);
        builder.Property(c => c.PeriodoLectivo).HasMaxLength(20);
        builder.Property(c => c.ModalidadServicio).HasMaxLength(100);
        builder.Property(c => c.NivelFormativo).HasMaxLength(100);
        builder.Property(c => c.TipoPlan).HasMaxLength(100);
        // LogoBase64: sin límite (puede ser grande).
    }
}
