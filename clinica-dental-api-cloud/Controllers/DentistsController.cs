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
    /// <param name="serviceId">Optional: only dentists enabled for this service.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DentistResponse>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<DentistResponse>> GetAll([FromQuery] long? serviceId,
        CancellationToken cancellationToken)
    {
        return await catalogService.GetDentistsAsync(serviceId, cancellationToken);
    }
}
