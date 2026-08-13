using System.ComponentModel.DataAnnotations;
using Bimss.Contracts.Diagnostics;

namespace Bimss.UnitTests.Contracts;

public class ValidationCheckRequestTests
{
    [Fact]
    public void Validate_Succeeds_ForAWellFormedRequest()
    {
        var request = new ValidationCheckRequest { Name = "Juan Dela Cruz", Age = 30, Email = "juan@example.test" };

        var results = Validate(request);

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_Fails_WhenNameIsMissing()
    {
        var request = new ValidationCheckRequest { Name = string.Empty, Age = 30 };

        var results = Validate(request);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ValidationCheckRequest.Name)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(151)]
    public void Validate_Fails_WhenAgeIsOutOfRange(int age)
    {
        var request = new ValidationCheckRequest { Name = "Juan Dela Cruz", Age = age };

        var results = Validate(request);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ValidationCheckRequest.Age)));
    }

    [Fact]
    public void Validate_Fails_WhenEmailIsMalformed()
    {
        var request = new ValidationCheckRequest { Name = "Juan Dela Cruz", Age = 30, Email = "not-an-email" };

        var results = Validate(request);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ValidationCheckRequest.Email)));
    }

    private static List<ValidationResult> Validate(ValidationCheckRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }
}
