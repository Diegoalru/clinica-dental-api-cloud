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
    [HttpGet]
    [ProducesResponseType<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<PagedResponse<AppointmentResponse>> List([FromQuery] AppointmentQuery query,
        CancellationToken cancellationToken)
    {
        return await appointmentService.ListAsync(query, cancellationToken);
    }

    /// <summary>
    /// Espacios de un día (hora de Costa Rica) para un odontólogo y un servicio, cada 15 minutos.
    /// Un día sin jornada devuelve una lista vacía; los espacios que ya empezaron no se incluyen.
    /// Solo para los roles que pueden agendar.
    /// </summary>
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

    /// <summary>
    /// Detalle de la cita con el contacto del paciente, la bitácora y las notificaciones.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType<AppointmentDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<AppointmentDetailResponse> Get(long id, CancellationToken cancellationToken)
    {
        return await appointmentService.GetAsync(id, cancellationToken);
    }

    /// <summary>
    /// Cambia el estado de la cita y lo registra en la bitácora. Recepción confirma, cancela o marca que no asistió;
    /// el odontólogo pone en atención o marca que no asistió en sus citas; el paciente cancela las suyas.
    /// </summary>
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
