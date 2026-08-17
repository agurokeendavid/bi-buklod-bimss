using Bimss.Application.Membership;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.Infrastructure.Membership;

public static class MembershipServiceCollectionExtensions
{
    public static IServiceCollection AddBimssMembership(this IServiceCollection services)
    {
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMemberQueryService, MemberQueryService>();
        services.AddScoped<IReferenceDataQueryService, ReferenceDataQueryService>();
        services.AddScoped<IMemberDocumentQueryService, MemberDocumentQueryService>();
        services.AddScoped<IImportBatchRepository, ImportBatchRepository>();
        services.AddScoped<IExcelWorkbookReader, ClosedXmlWorkbookReader>();
        services.AddScoped<IImportBatchQueryService, ImportBatchQueryService>();
        services.AddScoped<IMemberUpdateRequestRepository, MemberUpdateRequestRepository>();
        services.AddScoped<IMemberUpdateRequestQueryService, MemberUpdateRequestQueryService>();

        return services;
    }
}
