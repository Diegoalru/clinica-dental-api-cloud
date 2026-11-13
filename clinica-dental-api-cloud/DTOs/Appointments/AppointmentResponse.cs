namespace clinica_dental_api_cloud.DTOs.Appointments;

/// <summary>
/// Dates go out in Costa Rica time with their offset ("2026-11-20T09:00:00-06:00"), see ClinicDateTimeJsonConverter.
/// </summary>
public record AppointmentResponse(
    long Id,
    long PatientId,
    string PatientName,
    long DentistId,
    string DentistName,
    long ServiceId,
    string ServiceName,
    DateTime StartsAt,
    DateTime EndsAt,
    string Status,
    decimal PriceSnapshot,
    DateTime CreatedAt);
