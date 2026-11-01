namespace clinica_dental_api_cloud.Data.Entities;

public class AppointmentLog
{
    public long IdLog { get; set; }

    public long IdAppointment { get; set; }

    public long ChangedBy { get; set; }

    public string? PreviousStatus { get; set; }

    public string NewStatus { get; set; } = null!;

    public DateTime ChangedAt { get; set; }

    public string? Remarks { get; set; }

    public virtual User ChangedByNavigation { get; set; } = null!;

    public virtual Appointment IdAppointmentNavigation { get; set; } = null!;
}