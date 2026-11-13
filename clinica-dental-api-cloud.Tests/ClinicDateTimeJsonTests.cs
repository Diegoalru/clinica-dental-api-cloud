using System.Text.Json;
using clinica_dental_api_cloud.DTOs.Appointments;
using clinica_dental_api_cloud.Services;

namespace clinica_dental_api_cloud.Tests;

/// <summary>
/// Cómo viajan las fechas en el JSON del API: salen en hora de Costa Rica con "-06:00" y entran en cualquiera
/// de los tres formatos (sin zona = hora de Costa Rica, "Z" o con desfase).
/// </summary>
public class ClinicDateTimeJsonTests
{
    // Las mismas opciones que usa el API: camelCase (valores web por defecto) y el convertidor
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new ClinicDateTimeJsonConverter() }
    };

    private static string StartsAtJson(DateTime value) =>
        JsonSerializer.Serialize(new { StartsAt = value }, Options);

    [Fact]
    public void Escribe_UnaFechaUtc_EnHoraDeCostaRicaConDesfase()
    {
        var json = StartsAtJson(new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc));

        Assert.Equal("""{"startsAt":"2026-11-20T09:00:00-06:00"}""", json);
    }

    [Fact]
    public void Escribe_SinZona_LaTomaComoUtc()
    {
        var json = StartsAtJson(new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Unspecified));

        Assert.Equal("""{"startsAt":"2026-11-20T09:00:00-06:00"}""", json);
    }

    [Fact]
    public void Escribe_HoraLocalDeLaMaquina_ElMismoInstante()
    {
        var local = new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc).ToLocalTime();

        Assert.Equal("""{"startsAt":"2026-11-20T09:00:00-06:00"}""", StartsAtJson(local));
    }

    [Fact]
    public void Escribe_UnaCitaNocturna_ConLaFechaDeCostaRica()
    {
        var json = StartsAtJson(new DateTime(2026, 11, 21, 2, 30, 0, DateTimeKind.Utc));

        Assert.Equal("""{"startsAt":"2026-11-20T20:30:00-06:00"}""", json);
    }

    [Fact]
    public void Escribe_ConservaLasFraccionesDeSegundo()
    {
        // created_at viene de now() con microsegundos
        var value = new DateTime(2026, 11, 13, 16, 22, 33, DateTimeKind.Utc).AddTicks(1234560);

        Assert.Equal("""{"startsAt":"2026-11-13T10:22:33.123456-06:00"}""", StartsAtJson(value));
    }

    [Fact]
    public void Escribe_FechaNulable()
    {
        DateTime? empty = null;
        DateTime? value = new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc);

        Assert.Equal("""{"at":null}""", JsonSerializer.Serialize(new { At = empty }, Options));
        Assert.Equal("""{"at":"2026-11-20T09:00:00-06:00"}""", JsonSerializer.Serialize(new { At = value }, Options));
    }

    [Fact]
    public void Escribe_LaRespuestaDeUnaCita_TodasLasFechasEnHoraDeCostaRica()
    {
        var response = new AppointmentResponse(1, 2, "Paciente", 3, "Odontóloga", 4, "Limpieza",
            StartsAt: new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc),
            EndsAt: new DateTime(2026, 11, 20, 15, 45, 0, DateTimeKind.Utc),
            Status: "PENDING",
            PriceSnapshot: 25000m,
            CreatedAt: new DateTime(2026, 11, 13, 14, 5, 0, DateTimeKind.Utc));

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(response, Options));
        var root = json.RootElement;

        Assert.Equal("2026-11-20T09:00:00-06:00", root.GetProperty("startsAt").GetString());
        Assert.Equal("2026-11-20T09:45:00-06:00", root.GetProperty("endsAt").GetString());
        Assert.Equal("2026-11-13T08:05:00-06:00", root.GetProperty("createdAt").GetString());
    }

    [Theory]
    [InlineData("2026-11-20T09:00")]           // sin zona: hora de Costa Rica (lo que envía un <input type="datetime-local">)
    [InlineData("2026-11-20T09:00:00")]
    [InlineData("2026-11-20T09:00:00-06:00")]  // con el desfase de Costa Rica
    [InlineData("2026-11-20T15:00:00Z")]       // en UTC
    [InlineData("2026-11-20T10:00:00-05:00")]  // con otro desfase, el mismo instante
    public void Lee_LosFormatosAceptados_DanElMismoInstante(string startsAt)
    {
        var request = JsonSerializer.Deserialize<CreateAppointmentRequest>(
            $$"""{"dentistId":1,"serviceId":1,"startsAt":"{{startsAt}}"}""", Options)!;

        var utc = ClinicTime.ToUtc(request.StartsAt!.Value);

        Assert.Equal(new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
    }

    [Fact]
    public void IdaYVuelta_LoQueResponde_SePuedeVolverAEnviar()
    {
        var original = new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc);

        var json = JsonSerializer.Serialize(new CreateAppointmentRequest { StartsAt = original }, Options);
        var back = JsonSerializer.Deserialize<CreateAppointmentRequest>(json, Options)!;

        Assert.Equal(original, ClinicTime.ToUtc(back.StartsAt!.Value));
    }
}
