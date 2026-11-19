namespace Crm.Mediator;

/// <summary>Pipeline step 1: make sure the scope has a correlation ID.</summary>
internal sealed class CorrelationBehaviour(MessageContext context) : IRequestBehaviour
{
    public Task<TResponse> Handle<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> proceed, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(proceed);
        if (context.CorrelationId == Guid.Empty)
        {
            context.CorrelationId = Guid.NewGuid();
        }

        return proceed();
    }
}
