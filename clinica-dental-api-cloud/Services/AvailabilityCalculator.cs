namespace clinica_dental_api_cloud.Services;

/// <summary>
/// Calcula los espacios de un día sin tocar la base, para poder probarlo con xUnit.
/// </summary>
public static class AvailabilityCalculator
{
    /// <summary>
    /// Cada cuánto empieza un espacio dentro de la jornada (08:00, 08:15, 08:30...).
    /// </summary>
    public const int SlotStepMinutes = 15;

    /// <summary>
    /// Indica si el instante es el inicio de un espacio: en hora de Costa Rica, minutos múltiplos de 15
    /// y sin segundos (09:00, 09:15...). POST /appointments solo acepta estas horas, las mismas que ofrece
    /// la disponibilidad.
    /// </summary>
    public static bool IsSlotStart(DateTime utc)
    {
        var local = ClinicTime.ToClinic(utc);
        return local.Minute % SlotStepMinutes == 0 && local.TimeOfDay.Ticks % TimeSpan.TicksPerMinute == 0;
    }

    /// <param name="date">Día en hora de Costa Rica.</param>
    /// <param name="durationMinutes">Duración del servicio.</param>
    /// <param name="periods">Jornadas del odontólogo ese día, en hora de Costa Rica (dentist_schedules).</param>
    /// <param name="busy">Citas no canceladas y bloqueos, en UTC como vienen de la base.</param>
    /// <param name="nowUtc">Momento actual; los espacios que ya empezaron no se devuelven.</param>
    /// <returns>Los espacios en orden, con la hora de inicio en hora de Costa Rica.</returns>
    public static IReadOnlyList<(TimeOnly Time, bool Available)> Compute(
        DateOnly date,
        int durationMinutes,
        IEnumerable<(TimeOnly Start, TimeOnly End)> periods,
        IReadOnlyCollection<(DateTime StartsAt, DateTime EndsAt)> busy,
        DateTime nowUtc)
    {
        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));

        // SortedDictionary: si dos jornadas se traslapan, la misma hora sale una sola vez
        var slots = new SortedDictionary<TimeOnly, bool>();

        foreach (var (periodStart, periodEnd) in periods)
        {
            // En minutos desde la medianoche para no dar la vuelta al reloj con TimeOnly
            // Si la jornada no empieza en un múltiplo de 15 (08:10), el primer espacio es el siguiente (08:15)
            var startMinute = (periodStart.Hour * 60 + periodStart.Minute + SlotStepMinutes - 1)
                              / SlotStepMinutes * SlotStepMinutes;
            var endMinute = periodEnd.Hour * 60 + periodEnd.Minute;

            for (var minute = startMinute; minute + durationMinutes <= endMinute; minute += SlotStepMinutes)
            {
                var time = new TimeOnly(minute / 60, minute % 60);
                var startsAt = ClinicTime.ToUtc(date.ToDateTime(time));
                var endsAt = startsAt.AddMinutes(durationMinutes);

                if (startsAt <= nowUtc)
                    continue;

                // Mismo criterio que appointments_no_overlap: rangos [inicio, fin) que se cruzan
                slots[time] = !busy.Any(b => b.StartsAt < endsAt && startsAt < b.EndsAt);
            }
        }

        return slots.Select(pair => (pair.Key, pair.Value)).ToList();
    }
}
