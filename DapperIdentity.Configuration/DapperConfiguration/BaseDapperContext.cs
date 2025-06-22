using Dapper;
using DapperIdentity.Configuration.DapperConfiguration.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace DapperIdentity.Configuration.DapperConfiguration;


public class BaseDapperContext(IConfiguration config) : IBaseDapperContext
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection")!;

    public async Task<bool> IsConnectionOk()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand("SELECT 1", connection: connection);
            var result = await command.ExecuteScalarAsync();
            return result != null && Convert.ToUInt16(result) == 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION MESSAGE :: {ex.Message}");
            Console.WriteLine($"EXCEPTION :: {ex}");
            throw;
        }
    }

    public TEntity? QueryFirstOrDefault<TEntity>(string sql, object param, CommandType commandType = CommandType.Text,
        int? commandTimeout = null)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        return connection.QueryFirstOrDefault<TEntity>(sql, param, commandType: commandType, commandTimeout: commandTimeout);
    }

    public async Task<TEntity?> QueryFirstOrDefaultAsync<TEntity>(string sql, object param, CommandType commandType = CommandType.Text,
        int? commandTimeout = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, param, commandType: commandType, commandTimeout: commandTimeout);
    }

    public TEntity? QuerySingleOrDefault<TEntity>(string sql, object param, CommandType commandType = CommandType.Text,
        int? commandTimeout = null)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        return connection.QuerySingleOrDefault<TEntity>(sql, param, commandType: commandType, commandTimeout: commandTimeout);
    }

    public async Task<TEntity?> QuerySingleOrDefaultAsync<TEntity>(string sql, object param, CommandType commandType = CommandType.Text,
        int? commandTimeout = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QuerySingleOrDefaultAsync<TEntity>(sql, param, commandType: commandType, commandTimeout: commandTimeout);
    }

    public IEnumerable<TEntity> Query<TEntity>(string sql, object param, CommandType commandType = CommandType.Text,
        int? commandTimeout = null)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        return connection.Query<TEntity>(sql, param, commandType: commandType, commandTimeout: commandTimeout);
    }

    public async Task<IEnumerable<TEntity>> QueryAsync<TEntity>(string sql, object param, CommandType commandType = CommandType.Text,
        int? commandTimeout = null)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        return await connection.QueryAsync<TEntity>(sql, param, commandType: commandType, commandTimeout: commandTimeout);
    }

    public int Execute(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        return connection.Execute(sql, param, commandType: commandType,
            commandTimeout: commandTimeout);
    }

    public int Execute(List<CommandSql> commands)
    {
        using var connection = new SqlConnection(_connectionString);
        using var transaction = connection.BeginTransaction();

        var totalAffectedRow = 0;

        try
        {
            foreach (var cmd in commands)
            {
                totalAffectedRow += connection.Execute(cmd.Sql, param: cmd.Param, transaction: transaction, commandType: cmd.CommandType, commandTimeout: cmd.CommandTimeout);
            }

            transaction.Commit();
            commands.Clear();

            return totalAffectedRow;
        }
        catch (Exception ex)
        {
            transaction?.Rollback();
            throw;
        }
    }

    public async Task<int> ExecuteAsync(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.ExecuteAsync(sql, param, commandType: commandType,
            commandTimeout: commandTimeout);
    }

    public async Task<int> ExecuteAsync(List<CommandSql> commands)
    {
        using var connection = new SqlConnection(_connectionString);
        using var transaction = connection.BeginTransaction();

        var totalAffectedRow = 0;

        try
        {
            foreach (var cmd in commands)
            {
                totalAffectedRow += await connection.ExecuteAsync(cmd.Sql, param: cmd.Param, transaction: transaction, commandType: cmd.CommandType, commandTimeout: cmd.CommandTimeout);
            }

            transaction.Commit();
            commands.Clear();

            return totalAffectedRow;
        }
        catch (Exception ex)
        {
            transaction?.Rollback();
            return 0;
        }
    }

    public int ExecuteScaler(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null)
    {
        throw new NotImplementedException();
    }

    public Task<int> ExecuteScalerAsync(string sql, object param, CommandType commandType = CommandType.Text,
        int? commandTimeout = null)
    {
        throw new NotImplementedException();
    }
}