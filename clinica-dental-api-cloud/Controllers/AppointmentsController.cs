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

    [HttpGet("{id:long}")]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<AppointmentResponse> Get(long id, CancellationToken cancellationToken)
    {
        return await appointmentService.GetAsync(id, cancellationToken);
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
