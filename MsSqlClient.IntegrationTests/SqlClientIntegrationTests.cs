using System.Threading.Tasks;
using Xunit;

namespace MsSqlClient.IntegrationTests;

/// <summary>
/// Contains integration tests for the <see cref="SqlClient"/> class.
/// These tests require a live SQL Server instance to be running.
/// </summary>
public class SqlClientIntegrationTests
{
    /// <summary>
    /// Verifies that the <see cref="SqlClient"/> can connect to a SQL Server instance,
    /// execute a simple scalar query, and return the expected result.
    /// </summary>
    [Fact]
    public async Task Can_Connect_And_Query_Sql_Server()
    {
        // Arrange: Configure the connection options for the local SQL Server instance.
        // Using Windows Authentication (IntegratedSecurity = true).
        // TrustServerCertificate is set to true, which is often necessary for local
        // development environments with self-signed certificates.
        var options = new SqlConnectionOptions
        {
            Server = "JONATHANSYS1",
            Database = "master", // Connect to the 'master' database for this simple test.
            IntegratedSecurity = true,
            TrustServerCertificate = true // Often needed for local dev instances
        };

        // Instantiate the necessary factories and the SqlClient.
        var connectionFactory = new SqlConnectionFactory(options);
        var commandFactory = new SqlCommandFactory();
        var sqlClient = new SqlClient(connectionFactory, commandFactory);

        // Act: Execute a simple scalar query to verify connectivity.
        var result = await sqlClient.ExecuteScalarAsync("SELECT 1");

        // Assert: Check that the query returned the expected value (1).
        Assert.Equal(1, result);
    }
}
