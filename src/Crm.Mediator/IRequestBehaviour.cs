namespace Crm.Mediator;

/// <summary>
/// One step of the pipeline around every sent request (ADR-009): correlation, logging, validation, authorisation.
/// Behaviours run in registration order, the first outermost.
/// </summary>
public interface IRequestBehaviour
{
    Task<TResponse> Handle<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> proceed, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>;
}
