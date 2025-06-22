using System.Data;

namespace DapperIdentity.Configuration.DapperConfiguration.Abstractions;

public interface IBaseDapperContext
{
    Task<bool> IsConnectionOk();
    TEntity? QueryFirstOrDefault<TEntity>(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    Task<TEntity?> QueryFirstOrDefaultAsync<TEntity>(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    TEntity? QuerySingleOrDefault<TEntity>(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    Task<TEntity?> QuerySingleOrDefaultAsync<TEntity>(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    IEnumerable<TEntity> Query<TEntity>(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    Task<IEnumerable<TEntity>> QueryAsync<TEntity>(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    int Execute(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    int Execute(List<CommandSql> commands);
    Task<int> ExecuteAsync(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    Task<int> ExecuteAsync(List<CommandSql> commands);
    int ExecuteScaler(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    Task<int> ExecuteScalerAsync(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);

}


public record CommandSql(string Sql, object Param, CommandType CommandType = CommandType.Text, int? CommandTimeout = null);