namespace clinica_dental_api_cloud.Data.Entities;

public class Appointment
{
    public long IdAppointment { get; set; }

    public long IdPatient { get; set; }

    public long IdDentist { get; set; }

    public long IdService { get; set; }

    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }

    public string Status { get; set; } = null!;

    public decimal PriceSnapshot { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AppointmentLog> AppointmentLogs { get; set; } = new List<AppointmentLog>();

    public virtual DentistService DentistService { get; set; } = null!;

    public virtual Patient IdPatientNavigation { get; set; } = null!;

    public virtual MedicalRecord? MedicalRecord { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}