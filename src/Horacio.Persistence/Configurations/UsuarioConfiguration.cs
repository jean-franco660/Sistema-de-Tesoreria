using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Horacio.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.NombreCompleto).HasMaxLength(150);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Rol).HasConversion<int>();
        builder.Property(u => u.Estado).HasConversion<int>();
        builder.Property(u => u.FechaCreacion).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();
    }
}
