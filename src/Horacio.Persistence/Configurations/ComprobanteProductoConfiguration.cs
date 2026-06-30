using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class ComprobanteProductoConfiguration : IEntityTypeConfiguration<ComprobanteProducto>
{
    public void Configure(EntityTypeBuilder<ComprobanteProducto> builder)
    {
        builder.ToTable("comprobante_productos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Descripcion).HasMaxLength(400);
        builder.Property(p => p.Cantidad).HasColumnType("numeric(12,3)");
        builder.Property(p => p.PrecioUnitario).HasColumnType("numeric(12,2)");
        builder.Property(p => p.Importe).HasColumnType("numeric(12,2)");
    }
}
