using Microsoft.AspNetCore.Identity;

namespace DapperIdentity.Models.Identity;

public class ApplicationUserRole : IdentityUserRole<int>
{
    public virtual ApplicationUser? User { get; set; }
    public virtual ApplicationUser? Role { get; set; }
}