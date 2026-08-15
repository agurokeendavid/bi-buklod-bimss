using Bimss.Domain.Membership;
using Bimss.Domain.Membership.ReferenceData;
using Bimss.Infrastructure.Auditing;
using Bimss.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Persistence;

public class BimssDbContext(DbContextOptions<BimssDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public DbSet<Member> Members => Set<Member>();

    public DbSet<MemberStatusHistory> MemberStatusHistories => Set<MemberStatusHistory>();

    public DbSet<MemberEmployment> MemberEmployments => Set<MemberEmployment>();

    public DbSet<MemberContact> MemberContacts => Set<MemberContact>();

    public DbSet<MemberAddress> MemberAddresses => Set<MemberAddress>();

    public DbSet<CivilStatus> CivilStatuses => Set<CivilStatus>();

    public DbSet<Suffix> Suffixes => Set<Suffix>();

    public DbSet<OfficeUnit> OfficeUnits => Set<OfficeUnit>();

    public DbSet<EducationalAttainment> EducationalAttainments => Set<EducationalAttainment>();

    public DbSet<EligibilityType> EligibilityTypes => Set<EligibilityType>();

    public DbSet<RelationshipType> RelationshipTypes => Set<RelationshipType>();

    public DbSet<MemberStatusReason> MemberStatusReasons => Set<MemberStatusReason>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BimssDbContext).Assembly);
    }
}
