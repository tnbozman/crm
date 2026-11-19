using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Errors;

public static class ErrorHandlingServiceCollectionExtensions
{
    /// <summary>
    /// Registers RFC 9457 problem details and the global <see cref="CrmExceptionHandler"/>.
    /// The host must call <c>app.UseExceptionHandler()</c> early in its pipeline.
    /// </summary>
    public static IServiceCollection AddCrmErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<CrmExceptionHandler>();
        return services;
    }
}
