namespace clinica_dental_api_cloud.Services;

/// <summary>
/// Appointment status values allowed by the appointments_status_check constraint.
/// </summary>
public static class AppointmentStatus
{
    public const string Pending = "PENDING";
    public const string Confirmed = "CONFIRMED";
    public const string InProgress = "IN_PROGRESS";
    public const string Completed = "COMPLETED";
    public const string Cancelled = "CANCELLED";
    public const string NoShow = "NO_SHOW";

    public static readonly string[] All = [Pending, Confirmed, InProgress, Completed, Cancelled, NoShow];
}
