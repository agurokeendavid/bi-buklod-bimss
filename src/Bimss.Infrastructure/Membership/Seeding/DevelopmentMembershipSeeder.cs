using Bimss.Application.Membership;
using Bimss.Domain.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Bimss.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.Infrastructure.Membership.Seeding;

/// <summary>
/// Seeds synthetic Membership reference data (deferred from BIMSS-014) and a
/// handful of sample Member records spanning the status lifecycle, for local
/// Development use. Callers must guard invocation with
/// <c>IHostEnvironment.IsDevelopment()</c> — this must never run against a
/// real/production database. Idempotent: safe to call on every app startup.
/// Uses only synthetic data — never real Buklod member data (AGENTS.md).
/// </summary>
public static class DevelopmentMembershipSeeder
{
    private static readonly IReadOnlyList<(string Code, string Name)> CivilStatuses =
    [
        ("SINGLE", "Single"),
        ("MARRIED", "Married"),
        ("WIDOWED", "Widowed"),
        ("SEPARATED", "Separated"),
    ];

    private static readonly IReadOnlyList<(string Code, string Name)> Suffixes =
    [
        ("JR", "Jr."),
        ("SR", "Sr."),
        ("III", "III"),
    ];

    private static readonly IReadOnlyList<(string Code, string Name)> OfficeUnits =
    [
        ("HEAD-OFFICE", "Head Office"),
        ("PORT-OPS", "Port Operations"),
        ("IMM-REG", "Immigration Regulation Division"),
    ];

    private static readonly IReadOnlyList<(string Code, string Name)> EducationalAttainments =
    [
        ("COLLEGE-GRAD", "College Graduate"),
        ("HS-GRAD", "High School Graduate"),
        ("VOCATIONAL", "Vocational"),
    ];

    private static readonly IReadOnlyList<(string Code, string Name)> EligibilityTypes =
    [
        ("CS-PROF", "Civil Service Professional"),
        ("PRC", "PRC License"),
    ];

    private static readonly IReadOnlyList<(string Code, string Name)> RelationshipTypes =
    [
        ("SPOUSE", "Spouse"),
        ("CHILD", "Child"),
        ("PARENT", "Parent"),
        ("SIBLING", "Sibling"),
    ];

    private static readonly IReadOnlyList<(string Code, string Name)> MemberStatusReasons =
    [
        ("RESIGNED", "Resigned"),
        ("RETIRED", "Retired"),
        ("TERMINATED", "Terminated"),
    ];

    private static readonly IReadOnlyList<SeedMemberSpec> Members =
    [
        new("DEV-00001", "Santos", "Ana", MemberStatus.PendingVerification),
        new("DEV-00002", "Reyes", "Ben", MemberStatus.Active),
        new("DEV-00003", "Cruz", "Carla", MemberStatus.Inactive),
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BimssDbContext>();

        var civilStatusIds = await SeedReferenceDataAsync(
            dbContext.CivilStatuses, CivilStatuses, (id, code, name) => new CivilStatus(id, code, name), cancellationToken);
        await SeedReferenceDataAsync(
            dbContext.Suffixes, Suffixes, (id, code, name) => new Suffix(id, code, name), cancellationToken);
        var officeUnitIds = await SeedReferenceDataAsync(
            dbContext.OfficeUnits, OfficeUnits, (id, code, name) => new OfficeUnit(id, code, name), cancellationToken);
        await SeedReferenceDataAsync(
            dbContext.EducationalAttainments, EducationalAttainments, (id, code, name) => new EducationalAttainment(id, code, name), cancellationToken);
        await SeedReferenceDataAsync(
            dbContext.EligibilityTypes, EligibilityTypes, (id, code, name) => new EligibilityType(id, code, name), cancellationToken);
        await SeedReferenceDataAsync(
            dbContext.RelationshipTypes, RelationshipTypes, (id, code, name) => new RelationshipType(id, code, name), cancellationToken);
        var statusReasonIds = await SeedReferenceDataAsync(
            dbContext.MemberStatusReasons, MemberStatusReasons, (id, code, name) => new MemberStatusReason(id, code, name), cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        await SeedMembersAsync(
            scope.ServiceProvider, dbContext, civilStatusIds["SINGLE"], officeUnitIds["HEAD-OFFICE"], statusReasonIds["RESIGNED"], cancellationToken);
    }

    private static async Task<Dictionary<string, Guid>> SeedReferenceDataAsync<T>(
        DbSet<T> dbSet,
        IReadOnlyList<(string Code, string Name)> items,
        Func<Guid, string, string, T> factory,
        CancellationToken cancellationToken)
        where T : ReferenceDataItem
    {
        var result = new Dictionary<string, Guid>();

        foreach (var (code, name) in items)
        {
            var existing = await dbSet.FirstOrDefaultAsync(item => item.Code == code, cancellationToken);
            if (existing is not null)
            {
                result[code] = existing.Id;
                continue;
            }

            var id = Guid.NewGuid();
            dbSet.Add(factory(id, code, name));
            result[code] = id;
        }

        return result;
    }

    private static async Task SeedMembersAsync(
        IServiceProvider scopedServices,
        BimssDbContext dbContext,
        Guid civilStatusId,
        Guid officeUnitId,
        Guid statusReasonId,
        CancellationToken cancellationToken)
    {
        var creationService = scopedServices.GetRequiredService<MemberCreationService>();
        var transitionService = scopedServices.GetRequiredService<MemberStatusTransitionService>();

        foreach (var member in Members)
        {
            var alreadyExists = await dbContext.MemberEmployments
                .AnyAsync(employment => employment.EmployeeNumber == member.EmployeeNumber, cancellationToken);
            if (alreadyExists)
            {
                continue;
            }

            var command = new CreateMemberCommand(
                member.LastName,
                member.FirstName,
                MiddleName: null,
                SuffixId: null,
                new DateOnly(1990, 1, 1),
                "Manila",
                civilStatusId,
                "Synthetic Development seed data",
                member.EmployeeNumber,
                "Immigration Officer I",
                officeUnitId,
                new DateOnly(2020, 1, 1));

            var result = await creationService.CreateAsync(command, actorUserId: null, cancellationToken);

            if (member.TargetStatus is MemberStatus.Active or MemberStatus.Inactive)
            {
                await transitionService.VerifyAsync(result.MemberId, actorUserId: null, "Synthetic Development seed data", cancellationToken);
            }

            if (member.TargetStatus == MemberStatus.Inactive)
            {
                await transitionService.DeactivateAsync(
                    result.MemberId, statusReasonId, actorUserId: null, "Synthetic Development seed data", cancellationToken);
            }
        }
    }

    private sealed record SeedMemberSpec(string EmployeeNumber, string LastName, string FirstName, MemberStatus TargetStatus);
}
