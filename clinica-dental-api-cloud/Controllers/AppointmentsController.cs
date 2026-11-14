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

    [HttpGet("{id:long}")]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<AppointmentResponse> Get(long id, CancellationToken cancellationToken)
    {
        return await appointmentService.GetAsync(id, cancellationToken);
    }

    [HttpGet("{id:long}/details")]
    [Authorize(Roles = Roles.StaffOrDentist)]
    [ProducesResponseType<AppointmentDetailResponse>(StatusCodes.Status200OK)]
    public async Task<AppointmentDetailResponse> Details(long id, CancellationToken cancellationToken)
    {
        return await appointmentService.GetDetailsAsync(id, cancellationToken);
    }

    [HttpPatch("{id:long}/status")]
    [Authorize(Roles = Roles.StaffOrDentist)]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<AppointmentResponse> UpdateStatus(long id, UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        return await appointmentService.UpdateStatusAsync(id, request, cancellationToken);
    }

    [HttpPatch("{id:long}/reschedule")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<AppointmentResponse> Reschedule(long id, RescheduleAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        return await appointmentService.RescheduleAsync(id, request, cancellationToken);
    }

    [HttpPost("{id:long}/notifications/resend")]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ResendConfirmation(long id, CancellationToken cancellationToken)
    {
        await appointmentService.ResendConfirmationAsync(id, cancellationToken);
        return Accepted();
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
