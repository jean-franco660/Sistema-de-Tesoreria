using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class AlumnoConfiguration : IEntityTypeConfiguration<Alumno>
{
    public void Configure(EntityTypeBuilder<Alumno> builder)
    {
        builder.ToTable("alumnos");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Dni).IsRequired().HasMaxLength(8);
        builder.Property(a => a.Nombres).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Apellidos).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Seccion).IsRequired().HasMaxLength(5);
        builder.Property(a => a.Sexo).HasMaxLength(1);
        builder.Property(a => a.Celular).HasMaxLength(20);
        builder.Property(a => a.Estado).HasConversion<int>();

        builder.HasOne(a => a.PeriodoAcademico)
               .WithMany()
               .HasForeignKey(a => a.PeriodoAcademicoId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.Property(a => a.FechaRegistro).IsRequired();

        builder.HasIndex(a => a.Dni).IsUnique();

        builder.HasOne(a => a.Programa)
               .WithMany(p => p.Alumnos)
               .HasForeignKey(a => a.ProgramaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Turno)
               .WithMany(t => t.Alumnos)
               .HasForeignKey(a => a.TurnoId)
               .OnDelete(DeleteBehavior.Restrict);

        // Propiedades calculadas: no se mapean a columna.
        builder.Ignore(a => a.NombreCompleto);
        builder.Ignore(a => a.Edad);
    }
}
