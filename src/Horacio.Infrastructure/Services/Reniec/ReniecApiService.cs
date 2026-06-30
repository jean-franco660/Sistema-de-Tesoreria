using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Horacio.Application.Common.Interfaces;
using Horacio.Application.Common.Models;
using Horacio.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Horacio.Infrastructure.Services.Reniec;

/// <summary>
/// Proveedor RENIEC real vía HTTP. Es agnóstico del proveedor: hace
/// GET {BaseUrl}{dni} (con token Bearer opcional) y parsea el JSON de forma
/// flexible (acepta varias convenciones de nombres de campos).
/// Cambiar de proveedor solo requiere ajustar la configuración "Reniec".
/// </summary>
public class ReniecApiService : IReniecService
{
    private readonly HttpClient _httpClient;
    private readonly ReniecSettings _settings;

    public ReniecApiService(HttpClient httpClient, IOptions<ReniecSettings> options)
    {
        _settings = options.Value;
        _httpClient = httpClient;

        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HoracioTesoreria/1.0");

        if (!string.IsNullOrWhiteSpace(_settings.Token))
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.Token);
    }

    public async Task<ReniecPersona?> ConsultarDniAsync(string dni, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dni) || dni.Length != 8 || !dni.All(char.IsDigit))
            return null;

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
            return null;

        try
        {
            var url = _settings.BaseUrl.Contains("{dni}", StringComparison.OrdinalIgnoreCase)
                ? _settings.BaseUrl.Replace("{dni}", dni, StringComparison.OrdinalIgnoreCase)
                : $"{_settings.BaseUrl}{dni}";

            using var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = doc.RootElement;

            // Algunos proveedores envuelven la respuesta en "data".
            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
                root = data;

            var nombres = Leer(root, "nombres", "nombre", "first_name", "firstName");
            var paterno = Leer(root, "apellidoPaterno", "apellido_paterno", "ape_paterno", "apePaterno", "first_last_name");
            var materno = Leer(root, "apellidoMaterno", "apellido_materno", "ape_materno", "apeMaterno", "second_last_name");

            if (string.IsNullOrWhiteSpace(nombres) && string.IsNullOrWhiteSpace(paterno))
                return null;

            // Campos opcionales: solo algunos proveedores (de pago) los devuelven.
            var sexoRaw = Leer(root, "sexo", "genero", "gender");
            var sexo = sexoRaw?.Trim().ToUpperInvariant() switch
            {
                "M" or "MASCULINO" or "H" or "HOMBRE" => "H",
                "F" or "FEMENINO" or "MUJER" => "M",
                _ => (string?)null
            };

            var fechaRaw = Leer(root, "fechaNacimiento", "fecha_nacimiento", "fechaNac", "fecha_nac",
                                       "fechaNaci", "fecnac", "nacimiento", "birthDate", "birthdate");
            var fechaNac = ParsearFecha(fechaRaw);

            return new ReniecPersona
            {
                Dni = dni,
                Nombres = (nombres ?? string.Empty).ToUpperInvariant().Trim(),
                ApellidoPaterno = (paterno ?? string.Empty).ToUpperInvariant().Trim(),
                ApellidoMaterno = (materno ?? string.Empty).ToUpperInvariant().Trim(),
                Sexo = sexo,
                FechaNacimiento = fechaNac
            };
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Convierte la fecha de nacimiento que devuelven los proveedores RENIEC de pago.
    /// Acepta formato peruano (dd/MM/yyyy) e ISO (yyyy-MM-dd), entre otros.
    /// </summary>
    private static DateTime? ParsearFecha(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        raw = raw.Trim();

        string[] formatos = { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd" };
        if (DateTime.TryParseExact(raw, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exacta))
            return DateTime.SpecifyKind(exacta.Date, DateTimeKind.Utc);

        if (DateTime.TryParse(raw, new CultureInfo("es-PE"), DateTimeStyles.None, out var pe))
            return DateTime.SpecifyKind(pe.Date, DateTimeKind.Utc);

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var inv))
            return DateTime.SpecifyKind(inv.Date, DateTimeKind.Utc);

        return null;
    }

    private static string? Leer(JsonElement element, params string[] posiblesNombres)
    {
        foreach (var nombre in posiblesNombres)
        {
            foreach (var prop in element.EnumerateObject())
            {
                if (string.Equals(prop.Name, nombre, StringComparison.OrdinalIgnoreCase)
                    && prop.Value.ValueKind == JsonValueKind.String)
                {
                    return prop.Value.GetString();
                }
            }
        }
        return null;
    }
}
