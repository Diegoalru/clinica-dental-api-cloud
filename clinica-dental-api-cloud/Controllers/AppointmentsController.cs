using clinica_dental_api_cloud.DTOs;
using clinica_dental_api_cloud.DTOs.Appointments;
using clinica_dental_api_cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinica_dental_api_cloud.Controllers;

[ApiController]
[Authorize]
[Route("appointments")]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    /// <summary>Listar citas</summary>
    /// <remarks>
    /// Citas paginadas, de la más reciente a la más antigua. El personal ve todas; el odontólogo y el paciente,
    /// solo las suyas. Filtros opcionales por fechas (hora de Costa Rica), odontólogo, paciente y estado.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<PagedResponse<AppointmentResponse>> List([FromQuery] AppointmentQuery query,
        CancellationToken cancellationToken)
    {
        return await appointmentService.ListAsync(query, cancellationToken);
    }

    /// <summary>Horarios disponibles</summary>
    /// <remarks>
    /// Espacios de un día (hora de Costa Rica) para un odontólogo y un servicio, cada 15 minutos.
    /// Un día sin jornada devuelve una lista vacía; los espacios que ya empezaron no se incluyen.
    /// Solo para los roles que pueden agendar.
    /// </remarks>
    [HttpGet("availability")]
    [Authorize(Roles = Roles.CanBook)]
    [ProducesResponseType<IReadOnlyList<AvailabilitySlotResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IReadOnlyList<AvailabilitySlotResponse>> Availability([FromQuery] AvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        return await appointmentService.GetAvailabilityAsync(query, cancellationToken);
    }

    /// <summary>Detalle de una cita</summary>
    /// <remarks>
    /// Detalle de la cita con el contacto del paciente, la bitácora y las notificaciones.
    /// </remarks>
    [HttpGet("{id:long}")]
    [ProducesResponseType<AppointmentDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<AppointmentDetailResponse> Get(long id, CancellationToken cancellationToken)
    {
        return await appointmentService.GetAsync(id, cancellationToken);
    }

    /// <summary>Cambiar el estado de una cita</summary>
    /// <remarks>
    /// Cambia el estado de la cita y lo registra en la bitácora. Recepción confirma, cancela o marca que no asistió;
    /// el odontólogo pone en atención o marca que no asistió en sus citas; el paciente cancela las suyas.
    /// </remarks>
    [HttpPatch("{id:long}/status")]
    [ProducesResponseType<AppointmentDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<AppointmentDetailResponse> ChangeStatus(long id, UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        return await appointmentService.ChangeStatusAsync(id, request, cancellationToken);
    }

    /// <summary>Reprogramar una cita</summary>
    /// <remarks>
    /// Cambia la fecha, la hora y opcionalmente el odontólogo de una cita pendiente o confirmada. Valida jornada,
    /// bloqueos y traslapes (hora de Costa Rica, cada 15 minutos); la cita vuelve a PENDING, queda en la bitácora
    /// y el recordatorio se programa de nuevo. Solo personal de la clínica.
    /// </remarks>
    [HttpPatch("{id:long}/reschedule")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType<AppointmentDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<AppointmentDetailResponse> Reschedule(long id, RescheduleAppointmentRequest request,
        CancellationToken cancellationToken) => appointmentService.RescheduleAsync(id, request, cancellationToken);

    /// <summary>Reenviar la confirmación</summary>
    /// <remarks>
    /// Programa de nuevo el correo de confirmación de una cita vigente y lo anota en la bitácora.
    /// Solo personal de la clínica.
    /// </remarks>
    [HttpPost("{id:long}/notifications/resend")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType<AppointmentDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<AppointmentDetailResponse> ResendConfirmation(long id, CancellationToken cancellationToken) =>
        appointmentService.ResendConfirmationAsync(id, cancellationToken);

    /// <summary>Agendar una cita</summary>
    /// <remarks>
    /// Crea una cita PENDING en una transacción: valida horario, bloqueos y traslapes, guarda la bitácora y
    /// programa la confirmación y el recordatorio. La hora sin zona es hora de Costa Rica, cada 15 minutos.
    /// </remarks>

    [HttpPost]
    [Authorize(Roles = Roles.CanBook)]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentResponse>> Create(CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = appointment.Id }, appointment);
    }
}
