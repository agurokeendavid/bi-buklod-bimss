using Bimss.Application.Membership;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bimss.Infrastructure.Membership;

public static class MemberDocumentStorageServiceCollectionExtensions
{
    public static IServiceCollection AddBimssMemberDocumentStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MemberDocumentStorageOptions>(configuration.GetSection(MemberDocumentStorageOptions.SectionName));
        services.TryAddSingleton<IMemberDocumentStorage, LocalFileMemberDocumentStorage>();

        return services;
    }
}
