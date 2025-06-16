using Microsoft.AspNetCore.Identity;

namespace DapperIdentity.Models.Identity;

public class ApplicationUserLogin : IdentityUserLogin<int>
{
    public virtual ApplicationUser User { get; set; }
}