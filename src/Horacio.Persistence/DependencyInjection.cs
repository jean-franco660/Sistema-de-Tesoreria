using Horacio.Application.Common.Interfaces;
using Horacio.Persistence.Context;
using Horacio.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Horacio.Persistence;

/// <summary>
/// Registro de dependencias de la capa de persistencia.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npg =>
                npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
