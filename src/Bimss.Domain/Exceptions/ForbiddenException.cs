namespace Bimss.Domain.Exceptions;

public class ForbiddenException : BimssException
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
