using System.ComponentModel.DataAnnotations;

namespace clinica_dental_api_cloud.DTOs.Appointments;

public class UpdateAppointmentStatusRequest
{
    /// <summary>
    /// CONFIRMED, IN_PROGRESS, CANCELLED o NO_SHOW. COMPLETED se pone al guardar el expediente.
    /// </summary>
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string? Status { get; set; }

    /// <summary>
    /// Observación que queda en la bitácora. Obligatoria al cancelar.
    /// </summary>
    [MaxLength(500, ErrorMessage = "La observación no puede superar 500 caracteres.")]
    public string? Remarks { get; set; }
}
