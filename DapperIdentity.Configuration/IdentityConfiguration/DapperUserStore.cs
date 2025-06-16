using Configuration.DapperConfiguration.Abstractions;
using DapperIdentity.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DapperIdentity.Configuration.IdentityConfiguration;

public class DapperUserStore(IBaseDapperContext context) : IUserStore<ApplicationUser>
                                                           , IUserEmailStore<ApplicationUser>
                                                           , IUserPasswordStore<ApplicationUser>
                                                           , IUserPhoneNumberStore<ApplicationUser>
                                                           , IUserClaimStore<ApplicationUser>
                                                           , IUserLoginStore<ApplicationUser>
                                                           , IUserRoleStore<ApplicationUser>
                                                           , IUserSecurityStampStore<ApplicationUser>
{
    private IBaseDapperContext _context = context;

    #region IUserStore

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName,
        CancellationToken cancellationToken)
    {
        user.UserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null) throw new ArgumentNullException(nameof(user));

        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        user.SecurityStamp = Guid.NewGuid().ToString();

        try
        {
            var sql = @"INSERT INTO ApplicationUsers 
                            (UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
                            PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, 
                            TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName)
                            VALUES (@UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed, 
                            @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed, 
                            @TwoFactorEnabled, @LockoutEnabled, @AccessFailedCount, @FirstName, @LastName);
                            SELECT CAST(SCOPE_IDENTITY() as int)";

            var result = await _context.ExecuteAsync(sql, param: user);
            return result > 0
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError() { Description = "Error occured while creating a user." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION MESSAGE :: {ex.Message}");
            Console.WriteLine($"EXCEPTION :: {ex}");
            return IdentityResult.Failed(new IdentityError() { Description = "Error occured while creating a user." });
        }
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null) throw new ArgumentNullException(nameof(user));

        try
        {
            var sql = @"UPDATE ApplicationUsers SET UserName = @UserName, NormalizedUserName = @NormalizedUserName, 
                        Email = @Email, NormalizedEmail = @NormalizedEmail, EmailConfirmed = @EmailConfirmed, 
                        PasswordHash = @PasswordHash, SecurityStamp = @SecurityStamp, 
                        ConcurrencyStamp = @ConcurrencyStamp, PhoneNumber = @PhoneNumber, 
                        PhoneNumberConfirmed = @PhoneNumberConfirmed, 
                        TwoFactorEnabled = @TwoFactorEnabled, LockoutEnd = @LockoutEnd, LockoutEnabled = @LockoutEnabled, 
                        AccessFailedCount = @AccessFailedCount, FirstName = @FirstName, LastName = @LastName
                        WHERE Id = @Id";

            var result = await _context.ExecuteAsync(sql, param: user);
            return result > 0
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError() { Description = "Error occured while updating a user." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION MESSAGE :: {ex.Message}");
            Console.WriteLine($"EXCEPTION :: {ex}");
            return IdentityResult.Failed(new IdentityError() { Description = "Error occured while updating a user." });
        }
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null) throw new ArgumentNullException(nameof(user));

        try
        {
            var sql = "DELETE FROM ApplicationUsers WHERE Id = @Id";

            var result = await _context.ExecuteAsync(sql, param: new { Id = user.Id });
            return result > 0
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError() { Description = "Error occured while deleting a user." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION MESSAGE :: {ex.Message}");
            Console.WriteLine($"EXCEPTION :: {ex}");
            return IdentityResult.Failed(new IdentityError() { Description = "Error occured while deleting a user." });
        }
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = "SELECT * FROM ApplicationUsers WHERE Id = @Id";

        return await _context.QuerySingleOrDefaultAsync<ApplicationUser>(sql, param: new { Id = userId });
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = "SELECT * FROM ApplicationUsers WHERE NormalizedUserName = @NormalizedUserName";

        return await _context.QuerySingleOrDefaultAsync<ApplicationUser>(sql,
            param: new { NormalizedUserName = normalizedUserName });
    }

    public void Dispose()
    {
        _context = null!;
    }

    #endregion

    #region IUserEmailStore
    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = "SELECT * FROM ApplicationUsers WHERE NormalizedEmail = @NormalizedEmail";

        return await _context.QuerySingleOrDefaultAsync<ApplicationUser>(sql, param: new { NormalizedEmail = normalizedEmail });
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }
    #endregion

    #region IUserPasswordStore
    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }
    #endregion

    #region IUserPhoneNumberStore
    public Task SetPhoneNumberAsync(ApplicationUser user, string? phoneNumber, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.PhoneNumber = phoneNumber;
        return Task.CompletedTask;
    }

    public Task<string?> GetPhoneNumberAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PhoneNumber);
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PhoneNumberConfirmed);
    }

    public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.PhoneNumberConfirmed = confirmed;
        return Task.CompletedTask;
    }
    #endregion

    #region IUserClaimStore
    public Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region IUserLoginStore
    public Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ApplicationUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region IUserRoleStore
    public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region IUserSecurityStampStore
    public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp);
    }
    #endregion
}