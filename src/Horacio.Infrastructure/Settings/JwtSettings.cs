namespace Horacio.Infrastructure.Settings;

/// <summary>Configuración del token JWT (sección "Jwt" de appsettings).</summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "HoracioTesoreria";
    public string Audience { get; set; } = "HoracioTesoreriaClient";
    public int ExpirationMinutes { get; set; } = 480;
}
