using Microsoft.AspNetCore.Identity;

namespace DapperIdentity.Models.Identity;

public class ApplicationUserClaim : IdentityUserClaim<int>
{
    public virtual ApplicationUser? User { get; set; }
}