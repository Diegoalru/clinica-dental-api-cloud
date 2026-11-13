using System.ComponentModel.DataAnnotations;

namespace clinica_dental_api_cloud.DTOs.Appointments;

/// <summary>
/// Filters for GET /appointments. All optional.
/// </summary>
public class AppointmentQuery
{
    /// <summary>
    /// Appointments starting at or after this moment.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Appointments starting before this moment.
    /// </summary>
    public DateTime? To { get; set; }

    public long? DentistId { get; set; }

    public long? PatientId { get; set; }

    /// <summary>
    /// PENDING, CONFIRMED, IN_PROGRESS, COMPLETED, CANCELLED or NO_SHOW.
    /// </summary>
    public string? Status { get; set; }

    // Con un límite, (Page - 1) * PageSize no se desborda
    [Range(1, 10_000, ErrorMessage = "La página debe estar entre 1 y 10000.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int PageSize { get; set; } = 20;
}
