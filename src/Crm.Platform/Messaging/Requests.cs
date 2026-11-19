namespace Crm.Platform.Messaging;

/// <summary>A message sent to exactly one handler, in the module that owns it (ADR-009). Use IQuery or ICommand.</summary>
public interface IRequest<TResponse>;

/// <summary>Reads another module's data. The normal cross-module call.</summary>
public interface IQuery<TResponse> : IRequest<TResponse>;

/// <summary>Asks another module to act now. Prefer integration events; a command runs in its own transaction.</summary>
public interface ICommand<TResponse> : IRequest<TResponse>;

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
