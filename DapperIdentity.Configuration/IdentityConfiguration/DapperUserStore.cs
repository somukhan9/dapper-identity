using DapperIdentity.Configuration.DapperConfiguration.Abstractions;
using DapperIdentity.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Data;
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

        user.UserName ??= user.Email;
        user.NormalizedUserName ??= user.NormalizedEmail;

        var sql = @"INSERT INTO ApplicationUsers 
                            (UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
                            PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, 
                            TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName)
                            VALUES (@UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed, 
                            @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed, 
                            @TwoFactorEnabled, @LockoutEnabled, @AccessFailedCount, @FirstName, @LastName);
                            SELECT CAST(SCOPE_IDENTITY() as INT)";

        var result = await context.ExecuteAsync(sql, param: user);
        return result > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError() { Description = "Error occured while creating a user." });
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null) throw new ArgumentNullException(nameof(user));

        var sql = @"UPDATE ApplicationUsers SET UserName = @UserName, NormalizedUserName = @NormalizedUserName, 
                        Email = @Email, NormalizedEmail = @NormalizedEmail, EmailConfirmed = @EmailConfirmed, 
                        PasswordHash = @PasswordHash, SecurityStamp = @SecurityStamp, 
                        ConcurrencyStamp = @ConcurrencyStamp, PhoneNumber = @PhoneNumber, 
                        PhoneNumberConfirmed = @PhoneNumberConfirmed, 
                        TwoFactorEnabled = @TwoFactorEnabled, LockoutEnd = @LockoutEnd, LockoutEnabled = @LockoutEnabled, 
                        AccessFailedCount = @AccessFailedCount, FirstName = @FirstName, LastName = @LastName
                        WHERE Id = @Id";

        var result = await context.ExecuteAsync(sql, param: user);
        return result > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError() { Description = "Error occured while updating a user." });
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null) throw new ArgumentNullException(nameof(user));

        var deleteUserCommand = @"DELETE FROM ApplicationUsers WHERE Id = @Id";
        var deleteUserRolesCommand =
            @"DELETE FROM [DapperIdentity].[dbo].[ApplicationUserRoles] WHERE [UserId] = @UserId";
        var deleteUserLoginsCommand = @"DELETE FROM [DapperIdentity].[dbo].[ApplicationUserLogins] WHERE [UserId] = @UserId";
        var deleteUserTokensCommand = @"DELETE FROM [DapperIdentity].[dbo].[ApplicationUserTokens] WHERE [UserId] = @UserId";
        var deleteUserClaimsCommand = @"DELETE FROM [DapperIdentity].[dbo].[ApplicationUserClaims] WHERE [UserId] = @UserId";

        var commands = new List<CommandSql>() {
                new CommandSql(deleteUserCommand, new { Id = user.Id}),
                new CommandSql(deleteUserRolesCommand, new { UserId = user.Id}),
                new CommandSql(deleteUserLoginsCommand, new { UserId = user.Id}),
                new CommandSql(deleteUserTokensCommand, new { UserId = user.Id}),
                new CommandSql(deleteUserClaimsCommand, new { UserId = user.Id}),
            };

        var result = await context.ExecuteAsync(commands);
        return result > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError() { Description = "Error occured while deleting a user." });
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = "SELECT * FROM ApplicationUsers WHERE Id = @Id";

        return await context.QuerySingleOrDefaultAsync<ApplicationUser>(sql, param: new { Id = userId });
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = "SELECT * FROM ApplicationUsers WHERE NormalizedUserName = @NormalizedUserName";

        return await context.QuerySingleOrDefaultAsync<ApplicationUser>(sql,
            param: new { NormalizedUserName = normalizedUserName });
    }

    public void Dispose()
    {
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

        return await context.QuerySingleOrDefaultAsync<ApplicationUser>(sql, param: new { NormalizedEmail = normalizedEmail });
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
    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user is null) throw new ArgumentNullException(nameof(user));

        var sql =
            @"SELECT [ClaimType], [ClaimValue] FROM [DapperIdentity].[dbo].[ApplicationUserClaims] WHERE [UserId] = @UserId";

        var userClaims = await context.QueryAsync<ApplicationUserClaim>(sql, new { UserId = user.Id });

        return userClaims.Select(uc => new Claim(uc.ClaimType!, uc.ClaimValue!)).ToList();
    }

    public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user is null) throw new ArgumentNullException(nameof(user));

        var commands = new List<CommandSql>();

        foreach (var claim in claims)
        {
            var sql = @"INSERT INTO [DapperIdentity].[dbo].[ApplicationUserClaims] ([UserId], [ClaimType], [ClaimValue])
                        VALUES (@UserId, @ClaimType, @ClaimValue)";

            commands.Add(new CommandSql(sql, new { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value }));
        }

        await context.ExecuteAsync(commands);
    }

    public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user is null) throw new ArgumentNullException(nameof(user));

        var sql = @"UPDATE [DapperIdentity].[dbo].[ApplicationUserClaims] 
                SET ClaimType = @NewType, ClaimValue = @NewValue 
                WHERE UserId = @UserId AND ClaimType = @OldType AND ClaimValue = @OldValue";

        await context.ExecuteAsync(sql,
            new
            {
                UserId = user.Id,
                OldType = claim.Type,
                OldValue = claim.Value,
                NewType = newClaim.Type,
                NewValue = claim.Value
            });
    }

    public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user is null) throw new ArgumentNullException(nameof(user));

        var commands = new List<CommandSql>();

        foreach (var claim in claims)
        {
            var sql = "DELETE FROM UserClaims WHERE UserId = @UserId AND ClaimType = @ClaimType AND ClaimValue = @ClaimValue";

            commands.Add(new CommandSql(sql, new { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value }));
        }

        await context.ExecuteAsync(commands);

    }

    public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (claim is null) throw new ArgumentNullException(nameof(claim));

        var sql = @"SELECT ui.*
                    FROM [DapperIdentity].[dbo].[ApplicationUsers] u
                    INNER JOIN [DapperIdentity].[dbo].[ApplicationUserClaims] uc ON uc.UserId = u.Id
                    WHERE uc.[ClaimType] = @ClaimType AND uc.[ClaimValue] = @ClaimValue";

        var users = await context.QueryAsync<ApplicationUser>(sql, new { ClaimType = claim.Type, ClaimValue = claim.Value });

        return users.ToList();

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
    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user is null) throw new ArgumentNullException(nameof(user));

        var roleSqlQuery =
            @"SELECT Id FROM [DapperIdentity].[dbo].[ApplicationRoles] WHERE [NormalizedName] = @NormalizedName";

        var roleId = await context.QuerySingleOrDefaultAsync<int>(roleSqlQuery, new { NormalizedName = roleName.ToUpper() });

        if (roleId <= 0) throw new InvalidOperationException($"Role with name ${roleName} is not found.");

        var userRoleSqlCommand =
                @"INSERT INTO [DapperIdentity].[dbo].[ApplicationUserRoles] ([UserId], [RoleId]) VALUE (@UserId, @RoleId)";

        await context.ExecuteAsync(userRoleSqlCommand, new { UserId = user.Id, RoleId = roleId });

    }

    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user is null) throw new ArgumentNullException(nameof(user));

        var roleSqlQuery =
            @"SELECT Id FROM [DapperIdentity].[dbo].[ApplicationRoles] WHERE [NormalizedName] = @NormalizedName";

        var roleId = await context.QuerySingleOrDefaultAsync<int>(roleSqlQuery, new { NormalizedName = roleName.ToUpper() });

        if (roleId <= 0) throw new InvalidOperationException($"Role with name ${roleName} is not found.");

        var userRoleSqlCommand =
            @"DELETE FROM [DapperIdentity].[dbo].[ApplicationUserRoles] WHERE [RoleId] = @RoleId";

        await context.ExecuteAsync(userRoleSqlCommand, new { UserId = user.Id, RoleId = roleId });
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = @"SELECT r.[Name]
                    FROM [DapperIdentity].[dbo].[ApplicationUserRoles] ur
                    INNER JOIN [DapperIdentity].[dbo].[ApplicationRoles] r ON r.[Id] = ur.[RoleId]
                    WHERE ur.[UserId] = @UserId";

        var result = await context.QueryAsync<string>(sql, new { UserId = user.Id });
        return result.ToList();
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = @"SELECT ur.[UserId]
                    FROM [DapperIdentity].[dbo].[ApplicationUserRoles] ur
                    INNER JOIN [DapperIdentity].[dbo].[ApplicationRoles] r ON r.[Id] = ur.[RoleId]
                    WHERE ur.[UserId] = @UserId AND r.[NormalizedName] = @NormalizedName";

        var result = await context.QueryFirstOrDefaultAsync<int>(sql, new { UserId = user.Id, NormalizedName = roleName.ToUpper() });

        return result > 0;
    }

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = @"SELECT u.*
                    FROM [DapperIdentity].[dbo].[ApplicationUsers] u
                    INNER JOIN [DapperIdentity].[dbo].[ApplicationUserRoles] ur ON ur.[UserId] ] = u.[Id]
                    INNER JOIN [DapperIdentity].[dbo].[ApplicationRoles] r ON r.[Id] = ur.[RoleId]
                    WHERE r.[NormalizedName] = @NormalizedName";

        var users = await context.QueryAsync<ApplicationUser>(sql, new { NormalizedName = roleName });

        return users.ToList();
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