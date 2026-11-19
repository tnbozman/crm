using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Messaging;

/// <summary>Resolves handlers from the caller's scope, so they see the caller's ICurrentUser.</summary>
internal sealed class Mediator(IServiceProvider services) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var invoker = (RequestInvoker<TResponse>)Activator.CreateInstance(
            typeof(RequestInvoker<,>).MakeGenericType(request.GetType(), typeof(TResponse)))!;
        return invoker.Invoke(request, services, cancellationToken);
    }
}

internal abstract class RequestInvoker<TResponse>
{
    public abstract Task<TResponse> Invoke(object request, IServiceProvider services, CancellationToken cancellationToken);
}

internal sealed class RequestInvoker<TRequest, TResponse> : RequestInvoker<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Invoke(object request, IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredService<IRequestHandler<TRequest, TResponse>>().Handle((TRequest)request, cancellationToken);
}
