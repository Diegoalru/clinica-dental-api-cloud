using clinica_dental_api_cloud.Errors;

namespace clinica_dental_api_cloud.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private const string InvalidSession = "La sesión ya no es válida.";

    public long UserId =>
        long.TryParse(httpContextAccessor.HttpContext?.User.FindFirst(AuthClaims.Subject)?.Value, out var userId)
            ? userId
            : throw new ApiException(StatusCodes.Status401Unauthorized, InvalidSession);

    public string Role =>
        httpContextAccessor.HttpContext?.User.FindFirst(AuthClaims.Role)?.Value
        ?? throw new ApiException(StatusCodes.Status401Unauthorized, InvalidSession);
}
