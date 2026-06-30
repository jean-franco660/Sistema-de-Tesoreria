using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class DetalleTicketConfiguration : IEntityTypeConfiguration<DetalleTicket>
{
    public void Configure(EntityTypeBuilder<DetalleTicket> builder)
    {
        builder.ToTable("detalles_ticket");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Importe).HasColumnType("numeric(10,2)");

        builder.HasOne(d => d.Servicio)
               .WithMany(s => s.Detalles)
               .HasForeignKey(d => d.ServicioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
