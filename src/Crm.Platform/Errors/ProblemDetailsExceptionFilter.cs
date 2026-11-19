using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Errors;

/// <summary>
/// Converts the typed exceptions services (and the mediator pipeline) throw into RFC 9457 problem details:
/// not found 404, forbidden 403, validation 400 with field errors (CLAUDE.md layering rules:
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
            ForbiddenException => StatusCodes.Status403Forbidden,
            RequestValidationException => StatusCodes.Status400BadRequest,
            _ => null,
        };
        if (status is null)
        {
            return;
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = context.Exception is RequestValidationException invalid
            ? factory.CreateValidationProblemDetails(context.HttpContext, ToModelState(invalid), status.Value)
            : factory.CreateProblemDetails(context.HttpContext, status.Value);
        context.Result = new ObjectResult(problem)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };
        context.ExceptionHandled = true;
    }

    private static ModelStateDictionary ToModelState(RequestValidationException invalid)
    {
        var modelState = new ModelStateDictionary();
        foreach (var (field, messages) in invalid.Errors)
        {
            foreach (var message in messages)
            {
                modelState.AddModelError(field, message);
            }
        }

        return modelState;
    }
}
