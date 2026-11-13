namespace clinica_dental_api_cloud.Data.Entities;

public class Service
{
    public long IdService { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public short DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<DentistService> DentistServices { get; set; } = new List<DentistService>();
}