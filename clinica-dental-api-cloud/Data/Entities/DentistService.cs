namespace clinica_dental_api_cloud.Data.Entities;

public class DentistService
{
    public long IdDentist { get; set; }

    public long IdService { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Dentist IdDentistNavigation { get; set; } = null!;

    public virtual Service IdServiceNavigation { get; set; } = null!;
}