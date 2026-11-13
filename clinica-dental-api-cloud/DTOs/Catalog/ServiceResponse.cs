namespace clinica_dental_api_cloud.DTOs.Catalog;

public record ServiceResponse(long Id, string Name, string? Description, short DurationMinutes, decimal Price);
