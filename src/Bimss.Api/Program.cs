using Bimss.Api.ExceptionHandling;
using Bimss.Application;
using Bimss.Infrastructure;
using Bimss.Infrastructure.Authorization;
using Bimss.Infrastructure.Identity;
using Bimss.Infrastructure.Identity.Seeding;
using Bimss.Infrastructure.Membership.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

const string FrontendCorsPolicy = "Frontend";

builder.Services.AddControllers();
builder.Services.AddBimssInfrastructure(builder.Configuration);
builder.Services.AddBimssApplication();
builder.Services.AddBimssJwtAuthentication(builder.Configuration);
builder.Services.AddBimssAuthorization();
builder.Services.AddExceptionHandler<BimssExceptionHandler>();
builder.Services.AddProblemDetails();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DevelopmentIdentitySeeder.SeedAsync(app.Services);
    await DevelopmentMembershipSeeder.SeedAsync(app.Services);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed so Bimss.IntegrationTests can boot this host via WebApplicationFactory<Program>.
public partial class Program;
