using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinica_dental_api_cloud.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    // No consulta la base: una pausa de Supabase no debe marcar el despliegue como fallido
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get() => Ok(new HealthResponse("ok"));
}

public record HealthResponse(string Status);
