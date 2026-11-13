namespace clinica_dental_api_cloud.DTOs.Appointments;

/// <summary>
/// Dates in UTC ("Z"); the frontend shows them in local time.
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
