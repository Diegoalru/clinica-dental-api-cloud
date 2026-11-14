using clinica_dental_api_cloud.DTOs.Catalog;
using clinica_dental_api_cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinica_dental_api_cloud.Controllers;

[ApiController]
[Authorize]
[Route("dentists")]
public class DentistsController(ICatalogService catalogService) : ControllerBase
{
    /// <summary>Listar odontólogos</summary>
    /// <remarks>
    /// Odontólogos activos (ficha y cuenta activas).
    /// </remarks>
    /// <param name="serviceId">Opcional: solo los habilitados para este servicio.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DentistResponse>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<DentistResponse>> GetAll([FromQuery] long? serviceId,
        CancellationToken cancellationToken)
    {
        return await catalogService.GetDentistsAsync(serviceId, cancellationToken);
    }

    /// <summary>Mi ficha de odontólogo</summary>
    /// <remarks>
    /// El odontólogo con sesión, con su horario semanal y sus bloqueos ("Mi agenda" y "Mi horario").
    /// </remarks>
    [HttpGet("me")]
    [Authorize(Roles = Roles.Dentist)]
    [ProducesResponseType<DentistProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<DentistProfileResponse> Me(CancellationToken cancellationToken)
    {
        return await catalogService.GetOwnDentistAsync(cancellationToken);
    }
}
