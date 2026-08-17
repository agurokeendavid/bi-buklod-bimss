using Bimss.Application.Membership;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.Application;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers Bimss.Application-layer services — use cases/application
    /// services that depend only on ports (like IMemberRepository,
    /// IAuditLogger) implemented and registered by Bimss.Infrastructure.
    /// Kept as its own extension method, called from both hosts, so future
    /// use-case/service registrations have a single place to land without
    /// touching Program.cs again. No MediatR/CQRS framework — see AGENTS.md.
    /// </summary>
    public static IServiceCollection AddBimssApplication(this IServiceCollection services)
    {
        services.AddScoped<MemberCreationService>();
        services.AddScoped<MemberStatusTransitionService>();
        services.AddScoped<MemberProfileUpdateService>();
        services.AddScoped<MemberDocumentUploadService>();
        services.AddScoped<ImportBatchIngestionService>();
        services.AddScoped<ImportBatchValidationService>();
        services.AddScoped<ImportBatchMatchingService>();
        services.AddScoped<ImportBatchPromotionService>();
        services.AddScoped<MemberUpdateRequestSubmissionService>();
        services.AddScoped<MemberUpdateRequestReviewService>();

        return services;
    }
}
