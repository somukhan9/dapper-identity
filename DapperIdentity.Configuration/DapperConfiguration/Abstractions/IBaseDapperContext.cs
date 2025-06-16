using Microsoft.Data.SqlClient;
using System.Data;

namespace Configuration.DapperConfiguration.Abstractions;

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
    int Execute(Dictionary<string, object> dmlObjects, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    Task<int> ExecuteAsync(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    Task<int> ExecuteAsync(Dictionary<string, object> dmlObjects, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    int ExecuteScaler(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);
    Task<int> ExecuteScalerAsync(string sql, object param, CommandType commandType = CommandType.Text, int? commandTimeout = null);

}
