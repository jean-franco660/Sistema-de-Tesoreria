using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class ContadorConfiguration : IEntityTypeConfiguration<Contador>
{
    public void Configure(EntityTypeBuilder<Contador> builder)
    {
        builder.ToTable("contadores");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Serie).IsRequired().HasMaxLength(10);
        builder.Property(c => c.UltimoValor).IsRequired();
        builder.HasIndex(c => c.Nombre).IsUnique();
    }
}
