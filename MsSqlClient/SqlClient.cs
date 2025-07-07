using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace MsSqlClient
{
    /// <summary>
    /// Abstraction for creating SQL connections.
    /// </summary>
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    /// <summary>
    /// Default SQL Server connection factory using SqlConnectionOptions.
    /// </summary>
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly SqlConnectionOptions _options;

        public SqlConnectionFactory(SqlConnectionOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_options.GetConnectionString());
    }

    /// <summary>
    /// Abstraction for creating database commands.
    /// </summary>
    public interface ICommandFactory
    {
        DbCommand CreateCommand(string sql, IDbConnection connection);
    }

    /// <summary>
    /// Default command factory for SQL Server.
    /// </summary>
    public class SqlCommandFactory : ICommandFactory
    {
        public DbCommand CreateCommand(string sql, IDbConnection connection)
            => new SqlCommand(sql, (SqlConnection)connection);
    }

    /// <summary>
    /// Defines CRUD operations for a SQL database.
    /// </summary>
    public interface ISqlClient
    {
        Task<int> ExecuteNonQueryAsync(string sql, IDictionary<string, object>? parameters = null);
        Task<object> ExecuteScalarAsync(string sql, IDictionary<string, object>? parameters = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, Func<IDataReader, T> map, IDictionary<string, object>? parameters = null);
    }

    /// <summary>
    /// Default implementation of ISqlClient following SOLID principles.
    /// </summary>
    public class SqlClient : ISqlClient
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ICommandFactory _commandFactory;

        public SqlClient(IDbConnectionFactory connectionFactory, ICommandFactory commandFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, IDictionary<string, object>? parameters = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            await using var command = _commandFactory.CreateCommand(sql, connection);
            command.AddParameters(parameters);

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync(string sql, IDictionary<string, object>? parameters = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            await using var command = _commandFactory.CreateCommand(sql, connection);
            command.AddParameters(parameters);

            return (await command.ExecuteScalarAsync())!;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, Func<IDataReader, T> map, IDictionary<string, object>? parameters = null)
        {
            var results = new List<T>();

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            await using var command = _commandFactory.CreateCommand(sql, connection);
            command.AddParameters(parameters);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(map(reader));
            }

            return results;
        }
    }

    /// <summary>
    /// Extension methods for DbCommand to adhere to single responsibility.
    /// </summary>
    public static class DbCommandExtensions
    {
        public static void AddParameters(this DbCommand command, IDictionary<string, object>? parameters)
        {
            if (parameters == null) return;

            foreach (var kvp in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = kvp.Key;
                parameter.Value = kvp.Value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }
    }
}
