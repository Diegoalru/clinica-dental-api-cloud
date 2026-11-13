namespace clinica_dental_api_cloud.DTOs.Auth;

public record UserResponse(long Id, string FullName, string Email, string Role);