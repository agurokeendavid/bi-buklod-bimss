using Bimss.Domain.Exceptions;
using Bimss.Infrastructure.ExceptionHandling;
using Microsoft.AspNetCore.Http;

namespace Bimss.UnitTests.ExceptionHandling;

public class ExceptionClassifierTests
{
    [Fact]
    public void Classify_MapsNotFoundException_To404()
    {
        var classification = ExceptionClassifier.Classify(new NotFoundException("Member 'm1' was not found."));

        Assert.Equal(StatusCodes.Status404NotFound, classification.StatusCode);
        Assert.Equal("Member 'm1' was not found.", classification.Detail);
    }

    [Fact]
    public void Classify_MapsConflictException_To409()
    {
        var classification = ExceptionClassifier.Classify(new ConflictException("Already verified."));

        Assert.Equal(StatusCodes.Status409Conflict, classification.StatusCode);
    }

    [Fact]
    public void Classify_MapsForbiddenException_To403()
    {
        var classification = ExceptionClassifier.Classify(new ForbiddenException("Not allowed."));

        Assert.Equal(StatusCodes.Status403Forbidden, classification.StatusCode);
    }

    [Fact]
    public void Classify_MapsDomainValidationException_To400()
    {
        var classification = ExceptionClassifier.Classify(new DomainValidationException("Invalid."));

        Assert.Equal(StatusCodes.Status400BadRequest, classification.StatusCode);
    }

    [Fact]
    public void Classify_MapsUnknownException_To500_WithoutLeakingTheOriginalMessage()
    {
        var classification = ExceptionClassifier.Classify(new InvalidOperationException("password=hunter2"));

        Assert.Equal(StatusCodes.Status500InternalServerError, classification.StatusCode);
        Assert.DoesNotContain("hunter2", classification.Detail);
    }
}
