using System.Security.Claims;
using Bimss.Domain.Authorization;
using Bimss.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Bimss.Infrastructure.Identity;

public class PermissionClaimsTransformation(BimssDbContext dbContext) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identity)
        {
            return principal;
        }

        if (identity.HasClaim(claim => claim.Type == Permission.ClaimType))
        {
            return principal;
        }

        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return principal;
        }

        var permissions = await dbContext.UserRoles
            .Where(userRole => userRole.UserId == userId)
            .Join(
                dbContext.RolePermissions,
                userRole => userRole.RoleId,
                rolePermission => rolePermission.RoleId,
                (userRole, rolePermission) => rolePermission.PermissionName)
            .Distinct()
            .ToListAsync();

        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim(Permission.ClaimType, permission));
        }

        return principal;
    }
}
