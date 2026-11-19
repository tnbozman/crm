namespace Crm.Platform.Errors;

/// <summary>
/// Thrown by a service when the requested record does not exist or the caller may not know it exists.
/// The platform exception filter turns it into a 404 problem details response.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
