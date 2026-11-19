namespace Crm.Mediator;

/// <summary>The only way modules call each other (ADR-009). Inside a module, call services directly.</summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
