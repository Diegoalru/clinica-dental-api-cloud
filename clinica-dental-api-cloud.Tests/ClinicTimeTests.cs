using clinica_dental_api_cloud.Services;

namespace clinica_dental_api_cloud.Tests;

/// <summary>
/// La base guarda UTC y la clínica trabaja en hora de Costa Rica (UTC-6, sin horario de verano).
/// Estas pruebas no deben depender de la zona horaria de la máquina que las corre.
/// </summary>
public class ClinicTimeTests
{
    [Fact]
    public void ToUtc_SinZona_SeTomaComoHoraDeCostaRica()
    {
        var clinic = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Unspecified);

        var utc = ClinicTime.ToUtc(clinic);

        Assert.Equal(new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
    }

    [Fact]
    public void ToUtc_EnUtc_NoCambia()
    {
        var utc = new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc);

        Assert.Equal(utc, ClinicTime.ToUtc(utc));
    }

    [Fact]
    public void ToUtc_EnHoraLocalDeLaMaquina_SeConvierteAlMismoInstante()
    {
        var utc = new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc);
        var machineLocal = utc.ToLocalTime();

        Assert.Equal(utc, ClinicTime.ToUtc(machineLocal));
    }

    [Fact]
    public void ToUtc_NocheEnCostaRica_CaeAlDiaSiguienteEnUtc()
    {
        var clinic = new DateTime(2026, 11, 20, 20, 30, 0, DateTimeKind.Unspecified);

        Assert.Equal(new DateTime(2026, 11, 21, 2, 30, 0, DateTimeKind.Utc), ClinicTime.ToUtc(clinic));
    }

    [Fact]
    public void ToClinic_MadrugadaUtc_EsElDiaAnteriorEnCostaRica()
    {
        var utc = new DateTime(2026, 11, 21, 2, 30, 0, DateTimeKind.Utc);

        var clinic = ClinicTime.ToClinic(utc);

        Assert.Equal(new DateTime(2026, 11, 20, 20, 30, 0), clinic);
    }

    [Theory]
    [InlineData(2026, 1, 15)]
    [InlineData(2026, 4, 5)]
    [InlineData(2026, 7, 1)]
    [InlineData(2026, 11, 1)]
    [InlineData(2030, 3, 10)]
    public void ElDesfaseEsSiempreMenosSeisHoras_TodoElAnio(int year, int month, int day)
    {
        var utc = new DateTime(year, month, day, 12, 0, 0, DateTimeKind.Utc);

        Assert.Equal(TimeSpan.FromHours(-6), ClinicTime.ToClinicOffset(utc).Offset);
        Assert.Equal(utc.AddHours(-6), ClinicTime.ToClinic(utc));
    }

    [Fact]
    public void ElRespaldoDeMenosSeisHoras_CoincideConLaZonaRealDeCostaRica()
    {
        // En el contenedor chiseled no hay tzdata y ClinicTime usa UTC-6 fijo. Donde la zona sí existe
        // (Windows, la imagen del SDK), se comprueba que da lo mismo durante varios años.
        if (!TimeZoneInfo.TryFindSystemTimeZoneById("America/Costa_Rica", out var real))
            return;

        for (var utc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
             utc < new DateTime(2036, 1, 1, 0, 0, 0, DateTimeKind.Utc);
             utc = utc.AddHours(13))
        {
            Assert.Equal(TimeZoneInfo.ConvertTimeFromUtc(utc, real), ClinicTime.ToClinic(utc));
        }
    }

    [Fact]
    public void IsoDayOfWeek_SeCalculaConLaHoraDeCostaRica()
    {
        // Domingo 22/11/2026 a las 19:00 en Costa Rica es lunes 01:00 en UTC
        var utc = ClinicTime.ToUtc(new DateTime(2026, 11, 22, 19, 0, 0));
        Assert.Equal(DayOfWeek.Monday, utc.DayOfWeek);

        Assert.Equal(7, ClinicTime.IsoDayOfWeek(ClinicTime.ToClinic(utc)));
    }

    [Theory]
    [InlineData(2026, 11, 16, 1)] // lunes
    [InlineData(2026, 11, 20, 5)] // viernes
    [InlineData(2026, 11, 21, 6)] // sábado
    [InlineData(2026, 11, 22, 7)] // domingo
    public void IsoDayOfWeek_LunesEsUnoYDomingoEsSiete(int year, int month, int day, short expected)
    {
        Assert.Equal(expected, ClinicTime.IsoDayOfWeek(new DateTime(year, month, day)));
    }

    [Fact]
    public void ToClinicOffset_SinZona_SeTomaComoUtc()
    {
        // Así llegan las columnas timestamptz si el Kind se pierde: son UTC, no hora de la clínica
        var value = new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Unspecified);

        var clinic = ClinicTime.ToClinicOffset(value);

        Assert.Equal(new DateTimeOffset(2026, 11, 20, 9, 0, 0, TimeSpan.FromHours(-6)), clinic);
        Assert.Equal(9, clinic.Hour);
    }
}
