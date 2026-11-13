using clinica_dental_api_cloud.DTOs.Auth;

namespace clinica_dental_api_cloud.Services;

public interface IAuthService
{
    /// <summary>
    ///     Devuelve null cuando las credenciales son incorrectas o el usuario está inactivo.
    /// </summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    /// <summary>
    ///     Devuelve null cuando el usuario ya no existe o está inactivo.
    /// </summary>
    Task<UserResponse?> GetUserAsync(long userId, CancellationToken cancellationToken);

    /// <summary>
    ///     Crea la cuenta de un paciente y deja la sesión iniciada. Si recepción ya había registrado su cédula sin
    ///     cuenta, la vincula a esa ficha cuando el correo coincide.
    ///     Lanza ApiException 409 si el correo o la cédula ya están registrados.
    /// </summary>
    Task<LoginResponse> RegisterPatientAsync(RegisterPatientRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Crea una cuenta de administrador, recepcionista u odontólogo (con su perfil profesional).
    ///     Lanza ApiException 409 si el correo o el colegiado ya existen, y 400 si la especialidad no existe.
    /// </summary>
    Task<UserResponse> CreateStaffAsync(CreateStaffRequest request, CancellationToken cancellationToken);
}
