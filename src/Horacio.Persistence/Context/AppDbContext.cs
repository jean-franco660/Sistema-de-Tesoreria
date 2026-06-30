using System.Reflection;
using Horacio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Horacio.Persistence.Context;

/// <summary>
/// Contexto de EF Core. Aplica automáticamente todas las configuraciones del ensamblado.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Programa> Programas => Set<Programa>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<DetalleTicket> DetallesTicket => Set<DetalleTicket>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Contador> Contadores => Set<Contador>();
    public DbSet<Configuracion> Configuraciones => Set<Configuracion>();
    public DbSet<PeriodoAcademico> PeriodosAcademicos => Set<PeriodoAcademico>();
    public DbSet<RegistroMatricula> RegistrosMatricula => Set<RegistroMatricula>();
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<ComprobanteProducto> ComprobanteProductos => Set<ComprobanteProducto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
