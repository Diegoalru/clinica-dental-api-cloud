namespace clinica_dental_api_cloud.DTOs.Catalog;

/// <summary>
/// GET /dentists/me: el odontólogo con sesión, su horario semanal y sus bloqueos recientes y futuros.
/// </summary>
public record DentistProfileResponse(
    long Id,
    string FullName,
    string Specialty,
    string LicenseNumber,
    IReadOnlyList<DentistScheduleResponse> Schedules,
    IReadOnlyList<DentistBlockResponse> Blocks);

/// <summary>
/// Una jornada en hora de Costa Rica. DayOfWeek: 1 = lunes ... 7 = domingo; horas "HH:mm".
/// </summary>
public record DentistScheduleResponse(short DayOfWeek, string StartTime, string EndTime);

public record DentistBlockResponse(long Id, DateTime StartsAt, DateTime EndsAt, string? Reason);
