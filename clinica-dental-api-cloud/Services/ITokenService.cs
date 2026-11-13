namespace clinica_dental_api_cloud.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(long userId, string role);
}