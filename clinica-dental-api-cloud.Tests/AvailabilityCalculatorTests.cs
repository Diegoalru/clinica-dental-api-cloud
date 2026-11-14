using clinica_dental_api_cloud.Services;

namespace clinica_dental_api_cloud.Tests;

/// <summary>
/// Los espacios se arman en hora de Costa Rica; las citas y bloqueos llegan en UTC, como los guarda la base.
/// </summary>
public class AvailabilityCalculatorTests
{
    // Viernes 20/11/2026; "ahora" es un día antes para que ningún espacio haya pasado
    private static readonly DateOnly Friday = new(2026, 11, 20);
    private static readonly DateTime DayBefore = new(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);
    private static readonly (DateTime, DateTime)[] NoBusy = [];

    private static (TimeOnly, TimeOnly) Period(int startHour, int endHour) =>
        (new TimeOnly(startHour, 0), new TimeOnly(endHour, 0));

    private static DateTime Utc(int day, int hour, int minute = 0) =>
        new(2026, 11, day, hour, minute, 0, DateTimeKind.Utc);

    private static string[] Times(IEnumerable<(TimeOnly Time, bool Available)> slots, bool? available = null) =>
        slots.Where(s => available is null || s.Available == available)
            .Select(s => s.Time.ToString("HH:mm"))
            .ToArray();

    [Fact]
    public void Espacios_CadaQuinceMinutos_YElUltimoTerminaAlCierre()
    {
        var slots = AvailabilityCalculator.Compute(Friday, 30, [Period(8, 10)], NoBusy, DayBefore);

        Assert.Equal(["08:00", "08:15", "08:30", "08:45", "09:00", "09:15", "09:30"], Times(slots));
        Assert.All(slots, s => Assert.True(s.Available));
    }

    [Fact]
    public void ServicioMasLargoQueLaJornada_NoDaEspacios()
    {
        var slots = AvailabilityCalculator.Compute(Friday, 90, [Period(8, 9)], NoBusy, DayBefore);

        Assert.Empty(slots);
    }

    [Fact]
    public void DosJornadas_RespetanElAlmuerzo()
    {
        var slots = AvailabilityCalculator.Compute(Friday, 60, [Period(13, 15), Period(8, 10)], NoBusy, DayBefore);

        // 09:15 no cabe (terminaría 10:15) y 12:00-13:00 no es jornada; salen en orden aunque lleguen desordenadas
        Assert.Equal(["08:00", "08:15", "08:30", "08:45", "09:00", "13:00", "13:15", "13:30", "13:45", "14:00"],
            Times(slots));
    }

    [Fact]
    public void CitaEnUtc_OcupaLaHoraDeCostaRica()
    {
        // Cita de 09:00 a 09:30 en Costa Rica = 15:00 a 15:30 UTC
        (DateTime, DateTime)[] busy = [(Utc(20, 15), Utc(20, 15, 30))];

        var slots = AvailabilityCalculator.Compute(Friday, 30, [Period(8, 11)], busy, DayBefore);

        // 08:45 terminaría 09:15 y 09:15 empieza antes de 09:30: también chocan
        Assert.Equal(["08:45", "09:00", "09:15"], Times(slots, available: false));
        Assert.Contains("08:30", Times(slots, available: true));
        Assert.Contains("09:30", Times(slots, available: true));
    }

    [Fact]
    public void CitaQueTerminaJustoAlEmpezarElEspacio_NoLoOcupa()
    {
        // Cita de 08:00 a 09:00 Costa Rica; el espacio de 09:00 queda libre
        (DateTime, DateTime)[] busy = [(Utc(20, 14), Utc(20, 15))];

        var slots = AvailabilityCalculator.Compute(Friday, 30, [Period(8, 10)], busy, DayBefore);

        Assert.Equal(["09:00", "09:15", "09:30"], Times(slots, available: true));
    }

    [Fact]
    public void BloqueoDeTodoElDia_DejaTodoOcupado()
    {
        // Vacaciones del 20/11 a medianoche al 21/11 a medianoche en Costa Rica
        (DateTime, DateTime)[] busy = [(Utc(20, 6), Utc(21, 6))];

        var slots = AvailabilityCalculator.Compute(Friday, 30, [Period(8, 12)], busy, DayBefore);

        Assert.NotEmpty(slots);
        Assert.All(slots, s => Assert.False(s.Available));
    }

    [Fact]
    public void CitaNocturnaDelDiaAnteriorEnUtc_NoAfectaLaManana()
    {
        // 20/11 02:00 UTC es el 19/11 a las 20:00 en Costa Rica: no toca el viernes
        (DateTime, DateTime)[] busy = [(Utc(20, 2), Utc(20, 3))];

        var slots = AvailabilityCalculator.Compute(Friday, 30, [Period(8, 9)], busy, DayBefore);

        Assert.All(slots, s => Assert.True(s.Available));
    }

    [Fact]
    public void Hoy_NoDevuelveLosEspaciosQueYaEmpezaron()
    {
        // Son las 10:05 en Costa Rica (16:05 UTC)
        var now = Utc(20, 16, 5);

        var slots = AvailabilityCalculator.Compute(Friday, 30, [Period(8, 12)], NoBusy, now);

        Assert.Equal("10:15", Times(slots)[0]);
    }

    [Fact]
    public void Hoy_ALasOchoDeLaNocheEnCostaRica_YaEsOtroDiaEnUtc_PeroElDiaSigueTerminado()
    {
        // 21/11 02:00 UTC = 20/11 20:00 Costa Rica: la jornada del 20 ya pasó y la del 21 sigue entera
        var now = Utc(21, 2);

        Assert.Empty(AvailabilityCalculator.Compute(Friday, 30, [Period(8, 17)], NoBusy, now));
        Assert.Equal("08:00",
            Times(AvailabilityCalculator.Compute(Friday.AddDays(1), 30, [Period(8, 12)], NoBusy, now))[0]);
    }

    [Fact]
    public void JornadasTraslapadas_NoRepitenHoras()
    {
        var slots = AvailabilityCalculator.Compute(Friday, 30, [Period(8, 10), Period(9, 11)], NoBusy, DayBefore);

        Assert.Equal(Times(slots).Distinct().Count(), slots.Count);
        Assert.Equal("10:30", Times(slots)[^1]);
    }

    [Fact]
    public void DuracionInvalida_Falla()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AvailabilityCalculator.Compute(Friday, 0, [Period(8, 9)], NoBusy, DayBefore));
    }
}
