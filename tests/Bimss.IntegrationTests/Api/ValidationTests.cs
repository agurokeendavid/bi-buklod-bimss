using System.Net;
using System.Net.Http.Json;
using Bimss.Contracts.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.IntegrationTests.Api;

public class ValidationTests : IClassFixture<DiagnosticsApiFactory>
{
    private readonly DiagnosticsApiFactory _factory;

    public ValidationTests(DiagnosticsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_ReturnsOk_WhenTheRequestIsValid()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/diagnostics/validate-sample",
            new ValidationCheckRequest { Name = "Juan Dela Cruz", Age = 30, Email = "juan@example.test" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_ReturnsValidationProblem_WhenRequiredFieldIsMissing()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/diagnostics/validate-sample",
            new ValidationCheckRequest { Name = string.Empty, Age = 30 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(problem.Errors, error => error.Key == nameof(ValidationCheckRequest.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(151)]
    public async Task Post_ReturnsValidationProblem_WhenAgeIsOutOfRange(int age)
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/diagnostics/validate-sample",
            new ValidationCheckRequest { Name = "Juan Dela Cruz", Age = age });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(problem.Errors, error => error.Key == nameof(ValidationCheckRequest.Age));
    }

    [Fact]
    public async Task Post_ReturnsValidationProblem_WhenEmailIsMalformed()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/diagnostics/validate-sample",
            new ValidationCheckRequest { Name = "Juan Dela Cruz", Age = 30, Email = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(problem.Errors, error => error.Key == nameof(ValidationCheckRequest.Email));
    }
}
