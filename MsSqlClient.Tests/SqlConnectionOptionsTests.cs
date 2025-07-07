using Xunit;
using MsSqlClient;

namespace MsSqlClient.Tests;

public class SqlConnectionOptionsTests
{
    /// <summary>
    /// Tests that the connection string is built correctly for Windows Authentication.
    /// Input: Server, Database, and IntegratedSecurity = true.
    /// Expected Result: A valid connection string with "Integrated Security=True".
    /// </summary>
    [Fact]
    public void GetConnectionString_WithIntegratedSecurity_BuildsCorrectly()
    {
        // Arrange
        var options = new SqlConnectionOptions
        {
            Server = "localhost",
            Database = "TestDb",
            IntegratedSecurity = true
        };

        // Act
        var connectionString = options.GetConnectionString();

        // Assert
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        Assert.Equal("localhost", builder.DataSource);
        Assert.Equal("TestDb", builder.InitialCatalog);
        Assert.True(builder.IntegratedSecurity);
        Assert.True(builder.Encrypt);
        Assert.False(builder.TrustServerCertificate);
        Assert.Equal(30, builder.ConnectTimeout);
        Assert.False(builder.MultipleActiveResultSets);
    }

    /// <summary>
    /// Tests that the connection string is built correctly for SQL Server Authentication.
    /// Input: Server, Database, IntegratedSecurity = false, UserId, and Password.
    /// Expected Result: A valid connection string with "Integrated Security=False" and the correct user ID and password.
    /// </summary>
    [Fact]
    public void GetConnectionString_WithSqlAuthentication_BuildsCorrectly()
    {
        // Arrange
        var options = new SqlConnectionOptions
        {
            Server = "localhost",
            Database = "TestDb",
            IntegratedSecurity = false,
            UserId = "user",
            Password = "password"
        };

        // Act
        var connectionString = options.GetConnectionString();

        // Assert
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        Assert.Equal("localhost", builder.DataSource);
        Assert.Equal("TestDb", builder.InitialCatalog);
        Assert.False(builder.IntegratedSecurity);
        Assert.Equal("user", builder.UserID);
        Assert.Equal("password", builder.Password);
        Assert.True(builder.Encrypt);
        Assert.False(builder.TrustServerCertificate);
        Assert.Equal(30, builder.ConnectTimeout);
        Assert.False(builder.MultipleActiveResultSets);
    }

    /// <summary>
    /// Tests that the connection string is built correctly when a named SQL Server instance is provided.
    /// Input: Server, InstanceName, Database, and IntegratedSecurity = true.
    /// Expected Result: A valid connection string with the data source in the format "server\instance".
    /// </summary>
    [Fact]
    public void GetConnectionString_WithInstanceName_BuildsCorrectly()
    {
        // Arrange
        var options = new SqlConnectionOptions
        {
            Server = "localhost",
            InstanceName = "SQLEXPRESS",
            Database = "TestDb",
            IntegratedSecurity = true
        };

        // Act
        var connectionString = options.GetConnectionString();

        // Assert
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        Assert.Equal("localhost\\SQLEXPRESS", builder.DataSource);
        Assert.Equal("TestDb", builder.InitialCatalog);
        Assert.True(builder.IntegratedSecurity);
        Assert.True(builder.Encrypt);
        Assert.False(builder.TrustServerCertificate);
        Assert.Equal(30, builder.ConnectTimeout);
        Assert.False(builder.MultipleActiveResultSets);
    }

    /// <summary>
    /// Tests that the connection string is built correctly when a specific port number is provided.
    /// Input: Server, Port, Database, and IntegratedSecurity = true.
    /// Expected Result: A valid connection string with the data source in the format "server,port".
    /// </summary>
    [Fact]
    public void GetConnectionString_WithPort_BuildsCorrectly()
    {
        // Arrange
        var options = new SqlConnectionOptions
        {
            Server = "localhost",
            Port = 1433,
            Database = "TestDb",
            IntegratedSecurity = true
        };

        // Act
        var connectionString = options.GetConnectionString();

        // Assert
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        Assert.Equal("localhost,1433", builder.DataSource);
        Assert.Equal("TestDb", builder.InitialCatalog);
        Assert.True(builder.IntegratedSecurity);
        Assert.True(builder.Encrypt);
        Assert.False(builder.TrustServerCertificate);
        Assert.Equal(30, builder.ConnectTimeout);
        Assert.False(builder.MultipleActiveResultSets);
    }

    /// <summary>
    /// Tests that the connection string is built correctly when all possible options are configured.
    /// Input: All properties of SqlConnectionOptions set to non-default values.
    /// Expected Result: A valid connection string that reflects all of the specified options.
    /// </summary>
    [Fact]
    public void GetConnectionString_WithAllOptions_BuildsCorrectly()
    {
        // Arrange
        var options = new SqlConnectionOptions
        {
            Server = "localhost",
            Port = 1433,
            Database = "TestDb",
            IntegratedSecurity = false,
            UserId = "user",
            Password = "password",
            Encrypt = false,
            TrustServerCertificate = true,
            ConnectTimeout = 60,
            MultipleActiveResultSets = true
        };

        // Act
        var connectionString = options.GetConnectionString();

        // Assert
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        Assert.Equal("localhost,1433", builder.DataSource);
        Assert.Equal("TestDb", builder.InitialCatalog);
        Assert.False(builder.IntegratedSecurity);
        Assert.Equal("user", builder.UserID);
        Assert.Equal("password", builder.Password);
        Assert.False(builder.Encrypt);
        Assert.True(builder.TrustServerCertificate);
        Assert.Equal(60, builder.ConnectTimeout);
        Assert.True(builder.MultipleActiveResultSets);
    }
}
