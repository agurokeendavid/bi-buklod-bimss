using Bimss.Domain.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bimss.Infrastructure.Identity;

// Additive to IdentityDbContext's own base configuration of ApplicationUser
// (applied via ApplyConfigurationsFromAssembly after base.OnModelCreating in
// BimssDbContext) — narrows just the MemberId link BIMSS-040 introduced a
// real use for.
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // A member has at most one login account and vice versa — enforced
        // with a unique index rather than application code alone (AGENTS.md:
        // "Use database constraints ... for invariants that must survive
        // concurrent requests"). SQL Server treats multiple NULLs as
        // distinct, so accounts with no linked member (most officer/admin
        // accounts today) are unaffected.
        builder.HasIndex(user => user.MemberId).IsUnique();

        // Restrict, not Cascade — deactivating or reassigning a member must
        // never silently delete its login account, and Members are never
        // hard-deleted anyway (AGENTS.md).
        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(user => user.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
