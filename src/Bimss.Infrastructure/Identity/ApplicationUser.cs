using Microsoft.AspNetCore.Identity;

namespace Bimss.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? MemberId { get; set; }
}
