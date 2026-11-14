using System.ComponentModel.DataAnnotations;

namespace clinica_dental_api_cloud.DTOs.Appointments;

public class RescheduleAppointmentRequest
{
    [Required]
    [Range(1, long.MaxValue)]
    public long? DentistId { get; set; }

    [Required]
    public DateTime? StartsAt { get; set; }

    [MaxLength(480)]
    public string? Remarks { get; set; }
}
