using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class RegistroMatriculaConfiguration : IEntityTypeConfiguration<RegistroMatricula>
{
    public void Configure(EntityTypeBuilder<RegistroMatricula> builder)
    {
        builder.ToTable("registros_matricula");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Seccion).IsRequired().HasMaxLength(5);
        builder.Property(r => r.Profesor).HasMaxLength(150);
        builder.Property(r => r.ModuloFormativo).HasMaxLength(200);
        builder.Property(r => r.FechaCreacion).IsRequired();

        builder.HasOne(r => r.PeriodoAcademico)
               .WithMany()
               .HasForeignKey(r => r.PeriodoAcademicoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Programa)
               .WithMany()
               .HasForeignKey(r => r.ProgramaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Turno)
               .WithMany()
               .HasForeignKey(r => r.TurnoId)
               .OnDelete(DeleteBehavior.Restrict);

        // Un solo registro por combinación período + programa + turno + sección.
        builder.HasIndex(r => new { r.PeriodoAcademicoId, r.ProgramaId, r.TurnoId, r.Seccion }).IsUnique();
    }
}
