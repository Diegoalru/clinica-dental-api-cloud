using System.ComponentModel.DataAnnotations;

namespace clinica_dental_api_cloud.DTOs.Appointments;

public class CreateAppointmentRequest
{
    /// <summary>
    /// Required for ADMIN and RECEPTIONIST. A PATIENT can omit it (their own record is used).
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "El paciente no es válido.")]
    public long? PatientId { get; set; }

    [Required(ErrorMessage = "El odontólogo es obligatorio.")]
    [Range(1, long.MaxValue, ErrorMessage = "El odontólogo no es válido.")]
    public long? DentistId { get; set; }

    [Required(ErrorMessage = "El servicio es obligatorio.")]
    [Range(1, long.MaxValue, ErrorMessage = "El servicio no es válido.")]
    public long? ServiceId { get; set; }

    /// <summary>
    /// ISO 8601. Without zone it is taken as clinic time (Costa Rica): "2026-11-20T09:00" is 9:00 a. m. in Costa Rica.
    /// With "Z" or an offset it is used as is. The end is calculated from the service duration.
    /// Debe caer en un intervalo de 15 minutos en hora de Costa Rica (09:00, 09:15...), sin segundos,
    /// como los espacios de GET /appointments/availability.
    /// </summary>
    [Required(ErrorMessage = "La fecha y hora son obligatorias.")]
    public DateTime? StartsAt { get; set; }
}
