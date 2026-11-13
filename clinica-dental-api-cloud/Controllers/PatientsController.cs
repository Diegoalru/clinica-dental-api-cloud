using clinica_dental_api_cloud.DTOs.Patients;
using clinica_dental_api_cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinica_dental_api_cloud.Controllers;

[ApiController]
[Authorize(Roles = Roles.Staff)]
[Route("patients")]
public class PatientsController(IPatientService patientService) : ControllerBase
{
    /// <param name="search">Part of the name or the identification.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PatientResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IReadOnlyList<PatientResponse>> Search([FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        return await patientService.SearchAsync(search, cancellationToken);
    }
}
