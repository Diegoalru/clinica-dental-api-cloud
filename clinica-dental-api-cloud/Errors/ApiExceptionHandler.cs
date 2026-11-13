using Microsoft.AspNetCore.Diagnostics;

namespace clinica_dental_api_cloud.Errors;

/// <summary>
/// Writes an ApiException as ProblemDetails. Any other exception falls through to the default 500.
/// </summary>
public class ApiExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ApiException apiException)
            return false;

        httpContext.Response.StatusCode = apiException.StatusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = apiException.StatusCode,
                Title = apiException.Message
            }
        });
    }
}
