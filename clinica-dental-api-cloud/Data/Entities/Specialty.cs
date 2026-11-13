namespace clinica_dental_api_cloud.Data.Entities;

public class Specialty
{
    public short IdSpecialty { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Dentist> Dentists { get; set; } = new List<Dentist>();
}