using System.ComponentModel.DataAnnotations;
using Crm.SharedKernel.Errors;

namespace Crm.Mediator;

/// <summary>Pipeline step 3: validate the request's data annotations before anything acts on it.</summary>
internal sealed class ValidationBehaviour : IRequestBehaviour
{
    public Task<TResponse> Handle<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> proceed, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(proceed);

        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true))
        {
            var errors = results
                .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : [string.Empty]).Select(member => (Member: member, r.ErrorMessage)))
                .GroupBy(x => x.Member, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage ?? "Invalid.").ToArray(), StringComparer.Ordinal);
            throw new RequestValidationException(errors);
        }

        return proceed();
    }
}
