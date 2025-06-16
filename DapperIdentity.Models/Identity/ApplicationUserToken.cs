using Microsoft.AspNetCore.Identity;

namespace DapperIdentity.Models.Identity;

public class ApplicationUserToken : IdentityUserToken<int>
{
    public virtual ApplicationUser User { get; set; }
}