using Microsoft.AspNetCore.Identity;

namespace DapperIdentity.Models.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
    public virtual ICollection<ApplicationRoleClaim>? RoleClaims { get; set; }
}