using Microsoft.AspNetCore.Identity;

namespace DapperIdentity.Models.Identity;

public sealed class ApplicationUser : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public ICollection<ApplicationUserRole>? UserRoles { get; set; }
    public ICollection<ApplicationUserClaim>? Claims { get; set; }
    public ICollection<ApplicationUserLogin>? Logins { get; set; }
    public ICollection<ApplicationUserToken>? Tokens { get; set; }
}