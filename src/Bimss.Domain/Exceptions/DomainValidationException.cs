namespace Bimss.Domain.Exceptions;

public class DomainValidationException : BimssException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public DomainValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public DomainValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }
}
