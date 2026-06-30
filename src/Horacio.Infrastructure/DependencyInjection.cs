using Horacio.Application.Common.Interfaces;
using Horacio.Infrastructure.Services;
using Horacio.Infrastructure.Services.Excel;
using Horacio.Infrastructure.Services.Ia;
using Horacio.Infrastructure.Services.Ocr;
using Horacio.Infrastructure.Services.Reniec;
using Horacio.Infrastructure.Services.Storage;
using Horacio.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Horacio.Infrastructure;

/// <summary>
/// Registro de dependencias de la capa de infraestructura (servicios técnicos).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<ReniecSettings>(configuration.GetSection(ReniecSettings.SectionName));
        services.Configure<OcrSettings>(configuration.GetSection(OcrSettings.SectionName));
        services.Configure<DeepSeekSettings>(configuration.GetSection(DeepSeekSettings.SectionName));
        services.Configure<StorageSettings>(configuration.GetSection(StorageSettings.SectionName));

        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<INumberToWordsService, NumberToWordsService>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IExcelRegistroService, ExcelRegistroService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // OCR (foto → texto). Proveedor intercambiable por configuración.
        var ocrProvider = configuration.GetSection(OcrSettings.SectionName)["Provider"] ?? "Mock";
        if (ocrProvider.Equals("GoogleVision", StringComparison.OrdinalIgnoreCase))
            services.AddHttpClient<IOcrService, GoogleVisionOcrService>(c => c.Timeout = TimeSpan.FromSeconds(30));
        else
            services.AddScoped<IOcrService, MockOcrService>();

        // IA (texto → JSON). Proveedor intercambiable por configuración.
        var iaProvider = configuration.GetSection(DeepSeekSettings.SectionName)["Provider"] ?? "Mock";
        if (iaProvider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
            services.AddHttpClient<IComprobanteAnalyzer, DeepSeekComprobanteAnalyzer>(c => c.Timeout = TimeSpan.FromSeconds(60));
        else
            services.AddScoped<IComprobanteAnalyzer, MockComprobanteAnalyzer>();

        // RENIEC: el proveedor se elige por configuración (intercambiable).
        var provider = configuration.GetSection(ReniecSettings.SectionName)["Provider"] ?? "Fake";
        if (provider.Equals("Api", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IReniecService, ReniecApiService>(client =>
                client.Timeout = TimeSpan.FromSeconds(10));
        }
        else
        {
            services.AddScoped<IReniecService, ReniecFakeService>();
        }

        return services;
    }
}
