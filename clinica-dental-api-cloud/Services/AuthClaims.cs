namespace clinica_dental_api_cloud.Services;

/// <summary>
///     Claim names in the token. Program.cs turns off the inbound mapping so they stay as written here.
/// </summary>
public static class AuthClaims
{
    public const string Subject = "sub";
    public const string Role = "role";
}