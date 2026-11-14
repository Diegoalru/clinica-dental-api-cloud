namespace clinica_dental_api_cloud.DTOs.Appointments;

public record AppointmentDetailResponse(
    AppointmentResponse Appointment,
    AppointmentPatientResponse Patient,
    IReadOnlyList<AppointmentLogResponse> Logs,
    IReadOnlyList<AppointmentNotificationResponse> Notifications);

public record AppointmentPatientResponse(
    long Id, string FullName, string Identification, string? Phone, string? Email);

public record AppointmentLogResponse(
    long Id, string? PreviousStatus, string NewStatus, string ChangedBy,
    DateTime ChangedAt, string? Remarks);

public record AppointmentNotificationResponse(
    long Id, string Type, string Status, DateTime ScheduledFor, DateTime? SentAt);
