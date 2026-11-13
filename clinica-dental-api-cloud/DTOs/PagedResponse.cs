namespace clinica_dental_api_cloud.DTOs;

public record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
