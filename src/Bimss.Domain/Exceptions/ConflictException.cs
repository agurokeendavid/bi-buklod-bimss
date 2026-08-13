namespace Bimss.Domain.Exceptions;

public class ConflictException : BimssException
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
