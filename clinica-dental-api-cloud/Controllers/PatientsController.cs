using clinica_dental_api_cloud.DTOs.Patients;
using clinica_dental_api_cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinica_dental_api_cloud.Controllers;

// El rol va en cada acción: dos [Authorize] con roles distintos (clase y acción) se tendrían que cumplir los dos
[ApiController]
[Authorize]
[Route("patients")]
public class PatientsController(IPatientService patientService) : ControllerBase
{
    /// <summary>Buscar pacientes</summary>
    /// <remarks>
    /// Búsqueda de pacientes activos por nombre o identificación (hasta 20). Solo personal de la clínica.
    /// </remarks>
    /// <param name="search">Parte del nombre o de la identificación; la cédula se encuentra con o sin guiones.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [Authorize(Roles = Roles.Staff)]
    [ProducesResponseType<IReadOnlyList<PatientResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IReadOnlyList<PatientResponse>> Search([FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        return await patientService.SearchAsync(search, cancellationToken);
    }

    /// <summary>Mi ficha de paciente</summary>
    /// <remarks>
    /// Ficha del paciente con sesión ("Mi perfil"). Solo para el rol PATIENT.
    /// </remarks>
    [HttpGet("me")]
    [Authorize(Roles = Roles.Patient)]
    [ProducesResponseType<PatientProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<PatientProfileResponse> Me(CancellationToken cancellationToken)
    {
        return await patientService.GetOwnProfileAsync(cancellationToken);
    }
}
