namespace clinica_dental_api_cloud.Data.Entities;

public class Dentist
{
    public long IdDentist { get; set; }

    public long IdUser { get; set; }

    public short IdSpecialty { get; set; }

    public string LicenseNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<DentistBlock> DentistBlocks { get; set; } = new List<DentistBlock>();

    public virtual ICollection<DentistSchedule> DentistSchedules { get; set; } = new List<DentistSchedule>();

    public virtual ICollection<DentistService> DentistServices { get; set; } = new List<DentistService>();

    public virtual Specialty IdSpecialtyNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}