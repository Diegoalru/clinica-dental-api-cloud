namespace clinica_dental_api_cloud.DTOs.Appointments;

/// <summary>
/// GET /appointments/{id}: los datos de AppointmentResponse más el contacto del paciente, la bitácora y las
/// notificaciones. Las fechas salen en hora de Costa Rica con su desfase.
/// </summary>
public record AppointmentDetailResponse(
    long Id,
    long PatientId,
    string PatientName,
    string PatientIdentification,
    string? PatientPhone,
    string? PatientEmail,
    long DentistId,
    string DentistName,
    long ServiceId,
    string ServiceName,
    DateTime StartsAt,
    DateTime EndsAt,
    string Status,
    decimal PriceSnapshot,
    DateTime CreatedAt,
    IReadOnlyList<AppointmentLogResponse> Logs,
    IReadOnlyList<NotificationResponse> Notifications);

/// <summary>
/// Un cambio de estado. ChangedBy es el nombre de quien lo hizo.
/// </summary>
public record AppointmentLogResponse(
    long Id,
    string? PreviousStatus,
    string NewStatus,
    DateTime ChangedAt,
    string ChangedBy,
    string? Remarks);

public record NotificationResponse(
    long Id,
    string Type,
    string Channel,
    string Status,
    DateTime ScheduledFor,
    DateTime? SentAt);
