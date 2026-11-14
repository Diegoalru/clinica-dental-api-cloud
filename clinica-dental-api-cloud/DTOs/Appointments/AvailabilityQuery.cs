using System.ComponentModel.DataAnnotations;

namespace clinica_dental_api_cloud.DTOs.Appointments;

/// <summary>
/// Filtros de GET /appointments/availability. Los tres son obligatorios.
/// </summary>
public class AvailabilityQuery
{
    [Required(ErrorMessage = "El odontólogo es obligatorio.")]
    [Range(1, long.MaxValue, ErrorMessage = "El odontólogo no es válido.")]
    public long? DentistId { get; set; }

    [Required(ErrorMessage = "El servicio es obligatorio.")]
    [Range(1, long.MaxValue, ErrorMessage = "El servicio no es válido.")]
    public long? ServiceId { get; set; }

    /// <summary>
    /// Día en hora de Costa Rica, con formato AAAA-MM-DD (por ejemplo 2026-11-20).
    /// </summary>
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly? Date { get; set; }
}
