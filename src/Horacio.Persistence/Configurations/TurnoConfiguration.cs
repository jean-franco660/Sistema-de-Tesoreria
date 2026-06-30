using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("turnos");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Nombre).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Estado).HasConversion<int>();
        builder.HasIndex(t => t.Nombre).IsUnique();
    }
}
