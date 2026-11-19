using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Errors;

/// <summary>
/// Converts the typed exceptions services throw into RFC 9457 problem details (CLAUDE.md layering rules:
/// controllers have no try/catch). Unknown exceptions are left to the host's exception handler (500, no details).
/// The exception message is not copied into the response.
/// </summary>
public sealed class ProblemDetailsExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int? status = context.Exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            _ => null,
        };
        if (status is null)
        {
            return;
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = factory.CreateProblemDetails(context.HttpContext, status.Value);
        context.Result = new ObjectResult(problem)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };
        context.ExceptionHandled = true;
    }
}
