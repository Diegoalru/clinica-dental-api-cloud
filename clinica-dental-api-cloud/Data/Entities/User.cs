namespace clinica_dental_api_cloud.Data.Entities;

public class User
{
    public long IdUser { get; set; }

    public short IdRole { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AppointmentLog> AppointmentLogs { get; set; } = new List<AppointmentLog>();

    public virtual Dentist? Dentist { get; set; }

    public virtual Role IdRoleNavigation { get; set; } = null!;

    public virtual Patient? Patient { get; set; }
}