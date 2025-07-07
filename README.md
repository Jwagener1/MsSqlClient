# MsSqlClient

A simple, modern, and testable SQL client library for .NET, built on top of `Microsoft.Data.SqlClient`.

## Features

*   **SOLID Principles**: Designed with SOLID principles in mind, making it easy to test and maintain.
*   **Dependency Injection**: Easily integrate with your favorite dependency injection container.
*   **Async First**: All database operations are asynchronous.
*   **Testable**: Easily test your data access layer without hitting a real database.

## How to Use

### 1. Configure `SqlConnectionOptions`

Create an instance of `SqlConnectionOptions` to configure your database connection:

```csharp
var options = new SqlConnectionOptions
{
    Server = "localhost",
    Database = "MyDatabase",
    IntegratedSecurity = true
};
```

### 2. Create Factories

Create instances of `SqlConnectionFactory` and `SqlCommandFactory`:

```csharp
var connectionFactory = new SqlConnectionFactory(options);
var commandFactory = new SqlCommandFactory();
```

### 3. Create the `SqlClient`

Create an instance of `SqlClient`, passing in the factories:

```csharp
var sqlClient = new SqlClient(connectionFactory, commandFactory);
```

### 4. Execute Queries

Now you can use the `SqlClient` to execute queries against your database.

**Execute a non-query command:**

```csharp
var parameters = new Dictionary<string, object>
{
    { "@name", "John Doe" },
    { "@email", "john.doe@example.com" }
};

int rowsAffected = await sqlClient.ExecuteNonQueryAsync("INSERT INTO Users (Name, Email) VALUES (@name, @email)", parameters);
```

**Execute a scalar query:**

```csharp
object count = await sqlClient.ExecuteScalarAsync("SELECT COUNT(*) FROM Users");
```

**Execute a query and map the results:**

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

var users = await sqlClient.QueryAsync("SELECT Id, Name, Email FROM Users", reader => new User
{
    Id = reader.GetInt32(0),
    Name = reader.GetString(1),
    Email = reader.GetString(2)
});
```

## Testing

This solution includes two test projects:

### MsSqlClient.Tests

This project contains unit tests for the `MsSqlClient` library. The tests are written using xUnit and Moq.

-   **`GetConnectionString_WithIntegratedSecurity_BuildsCorrectly`**: Verifies that the connection string is correctly built for Windows Authentication.
-   **`GetConnectionString_WithSqlAuthentication_BuildsCorrectly`**: Verifies that the connection string is correctly built for SQL Server Authentication.
-   **`GetConnectionString_WithInstanceName_BuildsCorrectly`**: Verifies that the connection string correctly includes the SQL Server instance name.
-   **`GetConnectionString_WithPort_BuildsCorrectly`**: Verifies that the connection string correctly includes the port number.
-   **`GetConnectionString_WithAllOptions_BuildsCorrectly`**: Verifies that the connection string is correctly built when all possible options are specified.
-   **`ExecuteNonQueryAsync_ShouldReturnNumberOfRowsAffected`**: Tests that `ExecuteNonQueryAsync` correctly returns the number of rows affected by a command.
-   **`ExecuteScalarAsync_ShouldReturnScalarValue`**: Tests that `ExecuteScalarAsync` correctly returns a scalar value from a query.
-   **`QueryAsync_ShouldReturnMappedResults`**: Tests that `QueryAsync` correctly executes a query and maps the results to a collection of objects.
-   **`AddParameters_ShouldAddParametersToCommand`**: Tests that the `AddParameters` extension method correctly adds parameters to a `DbCommand`.
-   **`AddParameters_WithNullParameters_ShouldNotThrowException`**: Verifies that the `AddParameters` extension method does not throw an exception when passed a null dictionary of parameters.

### MsSqlClient.IntegrationTests

This project contains integration tests for the `SqlClient` class. These tests require a live SQL Server instance to be running.
