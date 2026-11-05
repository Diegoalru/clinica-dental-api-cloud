using clinica_dental_api_cloud.DTOs.Auth;

namespace clinica_dental_api_cloud.Services;

public interface IAuthService
{
    /// <summary>
    /// Returns null when the credentials are wrong or the user is inactive.
    /// </summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Returns null when the user no longer exists or is inactive.
    /// </summary>
    Task<UserResponse?> GetUserAsync(long userId, CancellationToken cancellationToken);
}