using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class PeriodoAcademicoConfiguration : IEntityTypeConfiguration<PeriodoAcademico>
{
    public void Configure(EntityTypeBuilder<PeriodoAcademico> builder)
    {
        builder.ToTable("periodos_academicos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Estado).HasConversion<int>();
        builder.Property(p => p.UsuarioApertura).HasMaxLength(50);
        builder.Property(p => p.UsuarioCierre).HasMaxLength(50);
        builder.Property(p => p.Observaciones).HasMaxLength(500);
        builder.HasIndex(p => p.Nombre).IsUnique();

        builder.HasMany(p => p.Tickets)
               .WithOne(t => t.PeriodoAcademico)
               .HasForeignKey(t => t.PeriodoAcademicoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
