namespace clinica_dental_api_cloud.Data.Entities;

public class MedicalRecord
{
    public long IdRecord { get; set; }

    public long IdAppointment { get; set; }

    public string? Symptoms { get; set; }

    public string PhysicalExam { get; set; } = null!;

    public string Diagnosis { get; set; } = null!;

    public string TreatmentPlan { get; set; } = null!;

    public string? Prescriptions { get; set; }

    public string? ClinicalNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Appointment IdAppointmentNavigation { get; set; } = null!;
}