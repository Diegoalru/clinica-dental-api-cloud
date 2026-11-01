namespace clinica_dental_api_cloud.Data.Entities;

public class Role
{
    public short IdRole { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}