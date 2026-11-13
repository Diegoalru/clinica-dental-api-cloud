namespace clinica_dental_api_cloud.Services;

/// <summary>
/// Conversions between UTC (how the database stores timestamptz) and the clinic's local time.
/// </summary>
public static class ClinicTime
{
    // La imagen chiseled del contenedor no trae tzdata, así que la zona puede no existir. Costa Rica no tiene
    // horario de verano desde 1992, por lo que UTC-6 fijo da la misma hora.
    private static readonly TimeZoneInfo Zone =
        TimeZoneInfo.TryFindSystemTimeZoneById("America/Costa_Rica", out var zone)
            ? zone
            : TimeZoneInfo.CreateCustomTimeZone("America/Costa_Rica", TimeSpan.FromHours(-6), "Costa Rica", "Costa Rica");

    /// <summary>
    /// A value with "Z" or an offset is converted as is; a value without zone is taken as clinic time.
    /// </summary>
    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => TimeZoneInfo.ConvertTimeToUtc(value, Zone)
    };

    public static DateTime ToClinic(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, Zone);

    /// <summary>
    /// ISO day of week, as in dentist_schedules: 1 = Monday ... 7 = Sunday.
    /// </summary>
    public static short IsoDayOfWeek(DateTime value) =>
        value.DayOfWeek == DayOfWeek.Sunday ? (short)7 : (short)value.DayOfWeek;
}
