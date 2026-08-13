namespace Bimss.Infrastructure.Identity;

public class RolePermission
{
    public Guid RoleId { get; set; }

    public string PermissionName { get; set; } = string.Empty;

    public ApplicationRole? Role { get; set; }
}
