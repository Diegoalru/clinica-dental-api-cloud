namespace clinica_dental_api_cloud.Services;

/// <summary>
/// The authenticated user of the current request, read from the JWT claims.
/// </summary>
public interface ICurrentUser
{
    long UserId { get; }

    string Role { get; }
}
