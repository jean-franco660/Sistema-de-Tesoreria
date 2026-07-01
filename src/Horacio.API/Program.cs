using System.Text;
using System.Text.Json.Serialization;
using Horacio.API.Middleware;
using Horacio.API.Services;
using Horacio.Application;
using Horacio.Application.Common.Interfaces;
using Horacio.Infrastructure;
using Horacio.Persistence;
using Horacio.Persistence.Context;
using Horacio.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Parse PostgreSQL connection URI if provided (e.g. from Railway DATABASE_URL)
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(rawConnectionString) && (rawConnectionString.StartsWith("postgres://") || rawConnectionString.StartsWith("postgresql://")))
{
    try
    {
        var uri = new Uri(rawConnectionString);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        
        builder.Configuration["ConnectionStrings:DefaultConnection"] = 
            $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing database connection URI: {ex.Message}");
    }
}

// ----------------------------------------------------------------------------
// Serilog
// ----------------------------------------------------------------------------
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// ----------------------------------------------------------------------------
// Servicios de las capas (Clean Architecture)
// ----------------------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ----------------------------------------------------------------------------
// Autenticación JWT
// ----------------------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Falta configurar 'Jwt:Key'.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ----------------------------------------------------------------------------
// CORS (frontend React)
// ----------------------------------------------------------------------------
const string CorsPolicy = "frontend";
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
{
    var allowedOriginsSetting = builder.Configuration["Cors:AllowedOrigins"];
    if (!string.IsNullOrEmpty(allowedOriginsSetting))
    {
        if (allowedOriginsSetting == "*")
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            var origins = allowedOriginsSetting.Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < origins.Length; i++)
            {
                origins[i] = origins[i].Trim();
            }
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    }
    else
    {
        // Por defecto, en producción permitimos cualquier origen si no se define uno específico para evitar problemas de CORS
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    }
}));

// ----------------------------------------------------------------------------
// Swagger / OpenAPI (con esquema Bearer)
// ----------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sistema de Ingresos por Recursos Propios — IEST \"Horacio Zeballos Gámez\"",
        Version = "v1",
        Description = "API de tesorería interna (comprobantes internos). NO es facturación electrónica / SUNAT."
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT (sin el prefijo 'Bearer ')."
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ----------------------------------------------------------------------------
// Health Checks
// ----------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString!, name: "postgresql");

var app = builder.Build();

// ----------------------------------------------------------------------------
// Migración + seeders al iniciar
// ----------------------------------------------------------------------------
await InicializarBaseDeDatosAsync(app);

// ----------------------------------------------------------------------------
// Pipeline HTTP
// ----------------------------------------------------------------------------
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Horacio Tesorería API v1");
    c.DocumentTitle = "Horacio Tesorería — API";
});

// Archivos estáticos: fotos de comprobantes subidas (Storage:RootPath → Storage:PublicBaseUrl).
var storageRoot = Path.GetFullPath(builder.Configuration.GetSection("Storage")["RootPath"] ?? "wwwroot/uploads");
var publicBase = (builder.Configuration.GetSection("Storage")["PublicBaseUrl"] ?? "/uploads").TrimEnd('/');
Directory.CreateDirectory(storageRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storageRoot),
    RequestPath = string.IsNullOrEmpty(publicBase) ? "" : publicBase
});

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// ----------------------------------------------------------------------------
// Inicialización de BD (migración + datos semilla)
// ----------------------------------------------------------------------------
static async Task InicializarBaseDeDatosAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();

        var hasher = services.GetRequiredService<IPasswordHasher>();
        await DbInitializer.SeedAsync(db, hasher);
        logger.LogInformation("Base de datos migrada y sembrada correctamente.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al migrar/sembrar la base de datos.");
    }
}

// Necesario para las pruebas de integración (WebApplicationFactory<Program>).
public partial class Program { }
