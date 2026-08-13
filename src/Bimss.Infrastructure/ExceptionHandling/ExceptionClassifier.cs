using Bimss.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Bimss.Infrastructure.ExceptionHandling;

public static class ExceptionClassifier
{
    private const string UnexpectedErrorDetail =
        "An unexpected error occurred. Please try again later or contact support if the problem persists.";

    public static ExceptionClassification Classify(Exception exception) => exception switch
    {
        NotFoundException notFound => new ExceptionClassification(StatusCodes.Status404NotFound, "Not Found", notFound.Message),
        ConflictException conflict => new ExceptionClassification(StatusCodes.Status409Conflict, "Conflict", conflict.Message),
        ForbiddenException forbidden => new ExceptionClassification(StatusCodes.Status403Forbidden, "Forbidden", forbidden.Message),
        DomainValidationException validation => new ExceptionClassification(StatusCodes.Status400BadRequest, "Validation Failed", validation.Message),
        _ => new ExceptionClassification(StatusCodes.Status500InternalServerError, "An unexpected error occurred.", UnexpectedErrorDetail),
    };
}
