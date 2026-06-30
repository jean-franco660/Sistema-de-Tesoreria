using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class ProgramaConfiguration : IEntityTypeConfiguration<Programa>
{
    public void Configure(EntityTypeBuilder<Programa> builder)
    {
        builder.ToTable("programas");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Estado).HasConversion<int>();
        builder.Property(p => p.FechaCreacion).IsRequired();
        builder.HasIndex(p => p.Nombre).IsUnique();
    }
}
