using Xunit;
using Moq;
using MsSqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace MsSqlClient.Tests;

public class SqlClientTests
{
    private readonly Mock<IDbConnectionFactory> _mockConnectionFactory;
    private readonly Mock<ICommandFactory> _mockCommandFactory;
    private readonly FakeDbConnection _fakeConnection;
    private readonly FakeDbCommand _fakeCommand;
    private readonly ISqlClient _sqlClient;

    public SqlClientTests()
    {
        _mockConnectionFactory = new Mock<IDbConnectionFactory>();
        _mockCommandFactory = new Mock<ICommandFactory>();
        _fakeConnection = new FakeDbConnection();
        _fakeCommand = (FakeDbCommand)_fakeConnection.CreateCommand();

        _mockConnectionFactory.Setup(f => f.CreateConnection()).Returns(_fakeConnection);
        _mockCommandFactory.Setup(f => f.CreateCommand(It.IsAny<string>(), It.IsAny<IDbConnection>())).Returns(_fakeCommand);

        _sqlClient = new SqlClient(_mockConnectionFactory.Object, _mockCommandFactory.Object);
    }

    /// <summary>
    /// Tests that ExecuteNonQueryAsync correctly returns the number of rows affected.
    /// Input: A SQL non-query string.
    /// Expected Result: The integer value representing the rows affected (1 in this case).
    /// </summary>
    [Fact]
    public async Task ExecuteNonQueryAsync_ShouldReturnNumberOfRowsAffected()
    {
        // Arrange
        _fakeCommand.ExecuteNonQueryAsyncHandler = (token) => Task.FromResult(1);

        // Act
        var result = await _sqlClient.ExecuteNonQueryAsync("UPDATE Test SET Value = 1");

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(ConnectionState.Open, _fakeConnection.State);
    }

    /// <summary>
    /// Tests that ExecuteScalarAsync correctly returns a scalar value.
    /// Input: A SQL query that returns a single value.
    /// Expected Result: The object representing the scalar value (123 in this case).
    /// </summary>
    [Fact]
    public async Task ExecuteScalarAsync_ShouldReturnScalarValue()
    {
        // Arrange
        _fakeCommand.ExecuteScalarAsyncHandler = (token) => Task.FromResult<object?>(123);

        // Act
        var result = await _sqlClient.ExecuteScalarAsync("SELECT 123");

        // Assert
        Assert.Equal(123, result);
        Assert.Equal(ConnectionState.Open, _fakeConnection.State);
    }

    /// <summary>
    /// Tests that QueryAsync correctly executes a query and maps the results.
    /// Input: A SQL query and a mapping function.
    /// Expected Result: An enumerable collection of mapped objects.
    /// </summary>
    [Fact]
    public async Task QueryAsync_ShouldReturnMappedResults()
    {
        // Arrange
        var reader = new FakeDbDataReader(new List<object[]> { new object[] { 1 }, new object[] { 2 } });
        _fakeCommand.ExecuteReaderAsyncHandler = (token) => Task.FromResult<DbDataReader>(reader);

        // Act
        var result = await _sqlClient.QueryAsync("SELECT 1", r => r.GetInt32(0));

        // Assert
        Assert.Collection(result, item => Assert.Equal(1, item), item => Assert.Equal(2, item));
        Assert.Equal(ConnectionState.Open, _fakeConnection.State);
    }

    /// <summary>
    /// Tests that the AddParameters extension method correctly adds parameters to a command.
    /// Input: A DbCommand and a dictionary of parameters.
    /// Expected Result: The parameters are correctly added to the command's Parameters collection.
    /// </summary>
    [Fact]
    public void AddParameters_ShouldAddParametersToCommand()
    {
        // Arrange
        var command = new FakeDbCommand();
        var parameters = new Dictionary<string, object?>
        {
            { "@p1", 1 },
            { "@p2", "test" },
            { "@p3", null }
        };

        // Act
        command.AddParameters(parameters);

        // Assert
        Assert.Equal(3, command.Parameters.Count);
        Assert.Equal(1, command.Parameters["@p1"].Value);
        Assert.Equal("test", command.Parameters["@p2"].Value);
        Assert.Equal(System.DBNull.Value, command.Parameters["@p3"].Value);
    }

    /// <summary>
    /// Tests that the AddParameters extension method does not throw an exception when given null parameters.
    /// Input: A DbCommand and a null dictionary of parameters.
    /// Expected Result: No exception is thrown.
    /// </summary>
    [Fact]
    public void AddParameters_WithNullParameters_ShouldNotThrowException()
    {
        // Arrange
        var command = new FakeDbCommand();

        // Act
        var exception = Record.Exception(() => command.AddParameters(null));

        // Assert
        Assert.Null(exception);
    }
}