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
    /// <summary>Listar servicios</summary>
    /// <remarks>
    /// Catálogo de servicios activos, con duración en minutos y precio de referencia.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ServiceResponse>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<ServiceResponse>> GetAll(CancellationToken cancellationToken)
    {
        return await catalogService.GetServicesAsync(cancellationToken);
    }
}
