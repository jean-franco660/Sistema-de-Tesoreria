using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.NumeroTicket).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Contador).IsRequired().HasMaxLength(20);
        builder.Property(t => t.FechaEmision).IsRequired();
        builder.Property(t => t.Total).HasColumnType("numeric(10,2)");
        builder.Property(t => t.Estado).HasConversion<int>();

        builder.HasIndex(t => t.Contador).IsUnique();
        builder.HasIndex(t => t.NumeroTicket);
        builder.HasIndex(t => t.FechaEmision);

        builder.HasOne(t => t.Alumno)
               .WithMany(a => a.Tickets)
               .HasForeignKey(t => t.AlumnoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Usuario)
               .WithMany(u => u.Tickets)
               .HasForeignKey(t => t.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Detalles)
               .WithOne(d => d.Ticket)
               .HasForeignKey(d => d.TicketId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
