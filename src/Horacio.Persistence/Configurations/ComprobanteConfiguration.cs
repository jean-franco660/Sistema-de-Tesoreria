using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class ComprobanteConfiguration : IEntityTypeConfiguration<Comprobante>
{
    public void Configure(EntityTypeBuilder<Comprobante> builder)
    {
        builder.ToTable("comprobantes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Proveedor).HasMaxLength(300);
        builder.Property(c => c.Ruc).HasMaxLength(20);
        builder.Property(c => c.TipoDocumento).HasMaxLength(50);
        builder.Property(c => c.NumeroComprobante).HasMaxLength(60);
        builder.Property(c => c.HoraEmision).HasMaxLength(20);
        builder.Property(c => c.Moneda).IsRequired().HasMaxLength(10);
        builder.Property(c => c.Subtotal).HasColumnType("numeric(12,2)");
        builder.Property(c => c.Igv).HasColumnType("numeric(12,2)");
        builder.Property(c => c.Total).HasColumnType("numeric(12,2)");
        builder.Property(c => c.Categoria).HasMaxLength(50);
        builder.Property(c => c.Concepto).HasMaxLength(400);
        builder.Property(c => c.MetodoPago).HasMaxLength(30);
        builder.Property(c => c.Observaciones).HasMaxLength(1000);
        builder.Property(c => c.Estado).HasConversion<int>();
        builder.Property(c => c.ImagenRuta).HasMaxLength(500);
        builder.Property(c => c.ImagenUrl).HasMaxLength(700);
        // ImagenBase64 y RespuestaIaJson: sin límite (pueden ser grandes).

        builder.HasIndex(c => c.FechaEmision);
        builder.HasIndex(c => c.FechaRegistro);
        builder.HasIndex(c => c.Categoria);

        builder.HasOne(c => c.Usuario)
               .WithMany()
               .HasForeignKey(c => c.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PeriodoAcademico)
               .WithMany()
               .HasForeignKey(c => c.PeriodoAcademicoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Productos)
               .WithOne(p => p.Comprobante)
               .HasForeignKey(p => p.ComprobanteId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
