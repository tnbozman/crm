using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.SharedKernel.Errors;

/// <summary>
/// The one place exceptions become HTTP responses, for every request (MVC, minimal endpoints, middleware).
/// Typed exceptions become RFC 9457 problem details; their messages are never copied into the response.
/// Anything else is left to the host's default: a 500 problem details with no detail.
/// </summary>
internal sealed class CrmExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int? status = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ForbiddenException => StatusCodes.Status403Forbidden,
            RequestValidationException => StatusCodes.Status400BadRequest,
            _ => null,
        };
        if (status is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = status.Value;
        ProblemDetails problem = exception is RequestValidationException invalid
            ? new HttpValidationProblemDetails(invalid.Errors.ToDictionary(e => e.Key, e => e.Value)) { Status = status }
            : new ProblemDetails { Status = status };

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        }).ConfigureAwait(false);
    }
}
