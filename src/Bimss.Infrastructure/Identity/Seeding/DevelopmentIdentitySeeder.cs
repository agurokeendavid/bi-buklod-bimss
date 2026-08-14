using Bimss.Domain.Authorization;
using Bimss.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bimss.Infrastructure.Identity.Seeding;

/// <summary>
/// Seeds synthetic roles, role-permission assignments, and dev accounts for
/// local Development use. Callers must guard invocation with
/// <c>IHostEnvironment.IsDevelopment()</c> — this must never run against a
/// real/production database. Idempotent: safe to call on every app startup.
/// </summary>
public static class DevelopmentIdentitySeeder
{
    // Synthetic, local-Development-only password — never a real credential,
    // never used outside Development seeding.
    private const string DevPassword = "Dev-Only-Passw0rd!23";

    private static readonly IReadOnlyList<(string RoleName, IReadOnlyCollection<string> Permissions)> Roles =
    [
        ("Administrator", Permission.All),
        ("Member",
        [
            Permission.Membership.ViewSelf,
            Permission.Beneficiary.ManageSelf,
            Permission.Contribution.ViewSelf,
            Permission.Loan.Apply,
            Permission.Loan.ViewSelf,
            Permission.Election.Vote,
        ]),
        ("MembershipOfficer",
        [
            Permission.Membership.Manage,
            Permission.Membership.Verify,
            Permission.Beneficiary.Approve,
            Permission.Report.ViewMembership,
        ]),
        ("FinanceOfficer",
        [
            Permission.Contribution.Manage,
            Permission.Loan.Review,
            Permission.Loan.Approve,
            Permission.Loan.Release,
            Permission.Report.ViewFinance,
        ]),
        ("ElectionCommittee", [Permission.Election.Manage, Permission.Election.Finalize]),
        ("Auditor", [Permission.Audit.View]),
    ];

    private static readonly IReadOnlyList<(string UserName, string RoleName)> Users =
    [
        ("admin.dev", "Administrator"),
        ("member.dev", "Member"),
        ("membership.officer.dev", "MembershipOfficer"),
        ("finance.officer.dev", "FinanceOfficer"),
        ("election.committee.dev", "ElectionCommittee"),
        ("auditor.dev", "Auditor"),
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<BimssDbContext>();

        foreach (var (roleName, permissions) in Roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new ApplicationRole { Name = roleName };
                var createResult = await roleManager.CreateAsync(role);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{roleName}': {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            var existingPermissions = await dbContext.RolePermissions
                .Where(rolePermission => rolePermission.RoleId == role.Id)
                .Select(rolePermission => rolePermission.PermissionName)
                .ToListAsync(cancellationToken);

            foreach (var permission in permissions.Except(existingPermissions))
            {
                dbContext.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionName = permission });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var (userName, roleName) in Users)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = $"{userName}@bimss.local",
                    EmailConfirmed = true,
                };
                var createResult = await userManager.CreateAsync(user, DevPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create dev user '{userName}': {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
