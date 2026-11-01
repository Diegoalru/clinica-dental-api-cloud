namespace clinica_dental_api_cloud.Data.Entities;

public class Notification
{
    public long IdNotification { get; set; }

    public long IdAppointment { get; set; }

    public string Type { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public string Recipient { get; set; } = null!;

    public string Status { get; set; } = null!;

    public short RetryCount { get; set; }

    public DateTime ScheduledFor { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Appointment IdAppointmentNavigation { get; set; } = null!;
}