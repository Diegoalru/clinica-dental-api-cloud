using clinica_dental_api_cloud.DTOs.Catalog;
using clinica_dental_api_cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinica_dental_api_cloud.Controllers;

[ApiController]
[Authorize]
[Route("services")]
public class ServicesController(ICatalogService catalogService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ServiceResponse>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<ServiceResponse>> GetAll(CancellationToken cancellationToken)
    {
        return await catalogService.GetServicesAsync(cancellationToken);
    }
}
