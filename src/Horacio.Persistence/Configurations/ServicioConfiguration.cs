using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
{
    public void Configure(EntityTypeBuilder<Servicio> builder)
    {
        builder.ToTable("servicios");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Precio).HasColumnType("numeric(10,2)");
        builder.Property(s => s.Estado).HasConversion<int>();
        builder.HasIndex(s => s.Nombre).IsUnique();
    }
}
