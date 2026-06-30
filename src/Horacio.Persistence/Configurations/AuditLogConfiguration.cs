using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Usuario).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Fecha).IsRequired();
        builder.Property(a => a.Ip).HasMaxLength(45);
        builder.Property(a => a.Accion).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Detalle).HasMaxLength(1000);
        builder.HasIndex(a => a.Fecha);
    }
}
