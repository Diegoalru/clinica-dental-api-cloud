namespace clinica_dental_api_cloud.Services;

public enum ServiceError
{
    None,
    Conflict,
    Validation
}

/// <summary>
///     Resultado de una operación que puede fallar por una regla de negocio. El controlador convierte el error en un código HTTP.
/// </summary>
public record ServiceResult<T>(T? Value, ServiceError Error, string? Message)
{
    public bool Succeeded => Error == ServiceError.None;

    public static ServiceResult<T> Success(T value) => new(value, ServiceError.None, null);

    public static ServiceResult<T> Conflict(string message) => new(default, ServiceError.Conflict, message);

    public static ServiceResult<T> Invalid(string message) => new(default, ServiceError.Validation, message);
}
