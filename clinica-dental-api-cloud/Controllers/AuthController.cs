using clinica_dental_api_cloud.DTOs.Auth;
using clinica_dental_api_cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clinica_dental_api_cloud.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Iniciar sesión</summary>
    /// <remarks>
    /// Inicia sesión con correo y contraseña. Devuelve el token JWT, su vencimiento y los datos del usuario.
    /// </remarks>
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

    /// <summary>Registrar un paciente</summary>
    /// <remarks>
    /// Registro público de pacientes. Devuelve el token, así el paciente queda con la sesión iniciada.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoginResponse>> Register(RegisterPatientRequest request,
        CancellationToken cancellationToken)
    {
        // Los conflictos (correo o cédula ya registrados) llegan como ApiException y salen como 409
        var response = await authService.RegisterPatientAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Me), response);
    }

    /// <summary>Crear una cuenta del personal</summary>
    /// <remarks>
    /// Crea cuentas del personal. Solo un administrador puede hacerlo.
    /// </remarks>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("staff")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> CreateStaff(CreateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var user = await authService.CreateStaffAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, user);
    }

    /// <summary>Usuario de la sesión</summary>
    /// <remarks>
    /// Usuario de la sesión actual (nombre, correo, rol y, según el rol, su paciente u odontólogo).
    /// </remarks>
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
}
