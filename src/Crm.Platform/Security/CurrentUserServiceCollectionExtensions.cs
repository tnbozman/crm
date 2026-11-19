using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Security;

public static class CurrentUserServiceCollectionExtensions
{
    /// <summary>Registers <see cref="ICurrentUser"/>, read once per scope from <see cref="IUserContext"/> (the request's user by default).</summary>
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HttpUserContext>();
        services.AddScoped<ICurrentUser>(sp => CurrentUser.FromPrincipal(sp.GetRequiredService<IUserContext>().Principal));
        return services;
    }
}
