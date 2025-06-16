using Microsoft.AspNetCore.Identity;

namespace DapperIdentity.Models.Identity;

public class ApplicationRoleClaim : IdentityRoleClaim<int>
{
    public virtual ApplicationRole Role { get; set; }
}