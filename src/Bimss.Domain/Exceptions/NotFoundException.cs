namespace Bimss.Domain.Exceptions;

public class NotFoundException : BimssException
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string objectType, object objectId)
        : base($"{objectType} '{objectId}' was not found.")
    {
    }
}
