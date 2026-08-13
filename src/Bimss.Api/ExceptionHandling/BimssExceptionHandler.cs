using Bimss.Domain.Exceptions;
using Bimss.Infrastructure.ExceptionHandling;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Api.ExceptionHandling;

public class BimssExceptionHandler(IProblemDetailsService problemDetailsService, IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var classification = ExceptionClassifier.Classify(exception);

        httpContext.Response.StatusCode = classification.StatusCode;

        ProblemDetails problemDetails = exception is DomainValidationException { Errors.Count: > 0 } validationException
            ? new ValidationProblemDetails(validationException.Errors.ToDictionary(error => error.Key, error => error.Value))
            {
                Status = classification.StatusCode,
                Title = classification.Title,
                Detail = classification.Detail,
            }
            : new ProblemDetails
            {
                Status = classification.StatusCode,
                Title = classification.Title,
                Detail = classification.Detail,
            };

        if (environment.IsDevelopment() && classification.StatusCode == StatusCodes.Status500InternalServerError)
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception,
        });
    }
}
