using Horacio.Application.Common.Interfaces;

namespace Horacio.Infrastructure.Services;

/// <summary>Provee la fecha/hora actual en zona horaria de Perú (UTC-5).</summary>
public class DateTimeService : IDateTimeService
{
    private static readonly TimeZoneInfo PeruTimeZone = ResolverZonaPeru();

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PeruTimeZone);

    public DateTime ToLocal(DateTime utcDateTime)
    {
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, PeruTimeZone);
    }

    private static TimeZoneInfo ResolverZonaPeru()
    {
        // IANA en Linux/macOS, Windows en Windows.
        foreach (var id in new[] { "America/Lima", "SA Pacific Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { /* probar el siguiente */ }
            catch (InvalidTimeZoneException) { /* probar el siguiente */ }
        }

        return TimeZoneInfo.CreateCustomTimeZone("PET", TimeSpan.FromHours(-5), "Peru Time", "Peru Time");
    }
}
