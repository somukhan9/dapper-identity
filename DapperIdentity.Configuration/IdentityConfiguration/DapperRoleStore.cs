using DapperIdentity.Configuration.DapperConfiguration.Abstractions;
using DapperIdentity.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DapperIdentity.Configuration.IdentityConfiguration;

public class DapperRoleStore(IBaseDapperContext context, ILogger<DapperRoleStore> logger) : IRoleStore<ApplicationRole>
{
    public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (role == null) throw new ArgumentNullException(nameof(role));

        try
        {
            var sql = @"INSERT INTO [DapperIdentity].[dbo].[ApplicationRoles] ([Name],[NormalizedName],[ConcurrencyStamp])
                        VALUES ([Name],[NormalizedName],[ConcurrencyStamp]);
                        SELECT CAST(SCOPE_IDENTITY AS INT)";

            role.ConcurrencyStamp = Guid.NewGuid().ToString();

            var result = await context.ExecuteAsync(sql, param: role);
            return result > 0
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError { Description = "Error occured while creating role." });
        }
        catch (Exception ex)
        {
            logger.LogError($"EXCEPTION MESSAGE :: {ex.Message}");
            logger.LogError($"EXCEPTION :: {ex}");
            return IdentityResult.Failed(new IdentityError { Description = "Error occured while creating role." });
        }
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (role == null) throw new ArgumentNullException(nameof(role));

        try
        {
            var sql = @"UPDATE [DapperIdentity].[dbo].[ApplicationRoles]
                           SET [Name] = @Name
                              ,[NormalizedName] = @NormalizedName
                              ,[ConcurrencyStamp] = @ConcurrencyStamp
                         WHERE [Id] = @Id";


            var result = await context.ExecuteAsync(sql, role);
            return result > 0
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError { Description = "Error occured while updating role." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION MESSAGE :: {ex.Message}");
            Console.WriteLine($"EXCEPTION :: {ex}");
            return IdentityResult.Failed(new IdentityError { Description = "Error occured while updating role." });
        }
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (role == null) throw new ArgumentNullException(nameof(role));

        try
        {
            var sql = @"DELETE FROM [DapperIdentity].[dbo].[ApplicationRoles]
                        WHERE [Id] = @Id";

            var result = await context.ExecuteAsync(sql, role);
            return result > 0
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError { Description = "Error occured while deleting role." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION MESSAGE :: {ex.Message}");
            Console.WriteLine($"EXCEPTION :: {ex}");
            return IdentityResult.Failed(new IdentityError { Description = "Error occured while updating role." });
        }
    }

    public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(role.Id.ToString());
    }

    public Task<string?> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(ApplicationRole role, string? roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(ApplicationRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<ApplicationRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = @"SELECT * FROM [DapperIdentity].[dbo].[ApplicationRoles] WHERE [Id] = @Id";

        return await context.QueryFirstOrDefaultAsync<ApplicationRole>(sql, new { Id = roleId });
    }

    public async Task<ApplicationRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var sql = @"SELECT * FROM [DapperIdentity].[dbo].[ApplicationRoles] WHERE [NormalizedName] = @NormalizedName";

        return await context.QuerySingleOrDefaultAsync<ApplicationRole>(sql,
            new { NormalizedName = normalizedRoleName });
    }

    public void Dispose()
    {
    }
}