namespace clinica_dental_api_cloud.Errors;

/// <summary>
/// Expected error that ApiExceptionHandler turns into a ProblemDetails response with its status code.
/// </summary>
public class ApiException(int statusCode, string title) : Exception(title)
{
    public int StatusCode { get; } = statusCode;

    public static ApiException BadRequest(string title) => new(StatusCodes.Status400BadRequest, title);
    public static ApiException Forbidden(string title) => new(StatusCodes.Status403Forbidden, title);
    public static ApiException NotFound(string title) => new(StatusCodes.Status404NotFound, title);
    public static ApiException Conflict(string title) => new(StatusCodes.Status409Conflict, title);
}
