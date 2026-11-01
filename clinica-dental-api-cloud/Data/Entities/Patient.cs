namespace clinica_dental_api_cloud.Data.Entities;

public class Patient
{
    public long IdPatient { get; set; }

    public long? IdUser { get; set; }

    public string FullName { get; set; } = null!;

    public string Identification { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? EmergencyContact { get; set; }

    public DateTime? DataConsentAt { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User? IdUserNavigation { get; set; }
}