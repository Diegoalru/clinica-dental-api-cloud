namespace clinica_dental_api_cloud.Data.Entities;

public class DentistBlock
{
    public long IdBlock { get; set; }

    public long IdDentist { get; set; }

    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }

    public string? Reason { get; set; }

    public virtual Dentist IdDentistNavigation { get; set; } = null!;
}