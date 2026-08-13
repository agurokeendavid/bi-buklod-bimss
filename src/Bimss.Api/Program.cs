using Bimss.Api.ExceptionHandling;
using Bimss.Application;
using Bimss.Infrastructure;
using Bimss.Infrastructure.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddBimssInfrastructure(builder.Configuration);
builder.Services.AddBimssApplication();
builder.Services.AddBimssAuthorization();
builder.Services.AddExceptionHandler<BimssExceptionHandler>();
builder.Services.AddProblemDetails();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed so Bimss.IntegrationTests can boot this host via WebApplicationFactory<Program>.
public partial class Program;
