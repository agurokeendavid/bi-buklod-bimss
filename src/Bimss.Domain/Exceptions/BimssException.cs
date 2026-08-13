namespace Bimss.Domain.Exceptions;

public abstract class BimssException : Exception
{
    protected BimssException(string message)
        : base(message)
    {
    }

    protected BimssException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
