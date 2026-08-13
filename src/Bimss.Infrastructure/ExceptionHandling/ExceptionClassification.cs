namespace Bimss.Infrastructure.ExceptionHandling;

public sealed record ExceptionClassification(int StatusCode, string Title, string Detail);
