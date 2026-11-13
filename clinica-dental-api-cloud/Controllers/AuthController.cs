using clinica_dental_api_cloud.DTOs.Auth;
using clinica_dental_api_cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinica_dental_api_cloud.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);

        // La misma respuesta para cualquier fallo, para no revelar qué correos existen
        return response is null
            ? Problem(title: "Credenciales inválidas.", statusCode: StatusCodes.Status401Unauthorized)
            : Ok(response);
    }

    /// <summary>
    ///     Registro público de pacientes. Devuelve el token, así el paciente queda con la sesión iniciada.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoginResponse>> Register(RegisterPatientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterPatientAsync(request, cancellationToken);

        return result.Succeeded
            ? CreatedAtAction(nameof(Me), result.Value)
            : ErrorResult(result.Error, result.Message);
    }

    /// <summary>
    ///     Crea cuentas del personal. Solo un administrador puede hacerlo.
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost("staff")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> CreateStaff(CreateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.CreateStaffAsync(request, cancellationToken);

        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ErrorResult(result.Error, result.Message);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var user = long.TryParse(User.FindFirst(AuthClaims.Subject)?.Value, out var userId)
            ? await authService.GetUserAsync(userId, cancellationToken)
            : null;

        return user is null
            ? Problem(title: "La sesión ya no es válida.", statusCode: StatusCodes.Status401Unauthorized)
            : Ok(user);
    }

    private ObjectResult ErrorResult(ServiceError error, string? message) => error switch
    {
        ServiceError.Conflict => Problem(title: message, statusCode: StatusCodes.Status409Conflict),
        _ => Problem(title: message, statusCode: StatusCodes.Status400BadRequest)
    };
}
