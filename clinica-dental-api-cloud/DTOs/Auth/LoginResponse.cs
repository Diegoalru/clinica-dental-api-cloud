namespace clinica_dental_api_cloud.DTOs.Auth;

public record LoginResponse(string AccessToken, DateTime ExpiresAt, UserResponse User);