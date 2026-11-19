namespace Crm.SharedKernel.Errors;

/// <summary>
/// The current user may not do this. The platform exception filter turns it into a 403 problem details response.
/// </summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException()
    {
    }

    public ForbiddenException(string message)
        : base(message)
    {
    }

    public ForbiddenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
