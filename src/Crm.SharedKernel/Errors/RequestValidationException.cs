namespace Crm.SharedKernel.Errors;

/// <summary>
/// A request failed validation at a boundary (HTTP or the mediator). The platform exception filter turns it into
/// a 400 validation problem details response listing <see cref="Errors"/> by field.
/// </summary>
public sealed class RequestValidationException : Exception
{
    public RequestValidationException()
        : this(new Dictionary<string, string[]>())
    {
    }

    public RequestValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public RequestValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("The request is not valid.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
