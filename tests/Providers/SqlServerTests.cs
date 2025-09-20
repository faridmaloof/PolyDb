using PolyDb.Enums;

namespace PolyDb.Tests.Providers;

/// <summary>
/// Test suite for SQL Server using PolyDb.
/// </summary>
public class SqlServerTests
{
    /// <summary>
    /// Defines a simple User class for testing.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private const string SqlServerConnectionString = "Server=.;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"; // Use master or a test DB

    private static async Task<PolyDb> GetConnectedPolyDb(string dbName = "PolyDbTestDb")
    {
        // Connect to master to create/drop the test database
        await using var masterDb = PolyDb.Connect(DatabaseType.SqlServer, "Server=.;Database=master;Trusted_Connection=True;TrustServerCertificate=True;");
        await masterDb.ExecuteAsync($"IF DB_ID('{dbName}') IS NOT NULL DROP DATABASE {dbName};");
        await masterDb.ExecuteAsync($"CREATE DATABASE {dbName};");

        // Connect to the newly created test database
        var connectionString = $"Server=.;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";
        var db = PolyDb.Connect(DatabaseType.SqlServer, connectionString);
        
        // Create table for tests
        await db.ExecuteAsync("CREATE TABLE Users (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100), Age INT)");
        return db;
    }

    [Fact]
    public async Task Test_SqlServer_CreateTable()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("CreateTableDb");

        // Act & Assert (table created in GetConnectedPolyDb)
        var tableName = await db.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'");
        Assert.Single(tableName);
        Assert.Equal("Users", tableName[0]);
    }

    [Fact]
    public async Task Test_SqlServer_InsertRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("InsertRecordDb");
        var uniqueName = $"InsertUser_{Guid.NewGuid().ToString()[..8]}";

        // Act
        var rowsAffected = await db.ExecuteAsync(
            "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)",
            new Dictionary<string, object>
            {
                ["@Name"] = uniqueName,
                ["@Age"] = 25
            }
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var users = await db.QueryAsync<User>("SELECT Name FROM Users WHERE Name = @Name", new Dictionary<string, object> { ["@Name"] = uniqueName });
        Assert.Single(users);
        Assert.Equal(uniqueName, users[0].Name);
    }

    [Fact]
    public async Task Test_SqlServer_QueryRecords()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("QueryRecordsDb");
        var uniqueName1 = $"QueryUser1_{Guid.NewGuid().ToString()[..8]}";
        var uniqueName2 = $"QueryUser2_{Guid.NewGuid().ToString()[..8]}";
        await db.ExecuteAsync("INSERT INTO Users (Name, Age) VALUES (@Name, @Age)", new Dictionary<string, object> { ["@Name"] = uniqueName1, ["@Age"] = 30 });
        await db.ExecuteAsync("INSERT INTO Users (Name, Age) VALUES (@Name, @Age)", new Dictionary<string, object> { ["@Name"] = uniqueName2, ["@Age"] = 35 });

        // Act
        var users = await db.QueryAsync<User>(
            "SELECT Id, Name, Age FROM Users WHERE Age > @MinAge ORDER BY Age",
            new Dictionary<string, object>
            {
                ["@MinAge"] = 28
            }
        );

        // Assert
        Assert.Equal(2, users.Count);
        Assert.Equal(uniqueName1, users[0].Name);
        Assert.Equal(30, users[0].Age);
        Assert.Equal(uniqueName2, users[1].Name);
        Assert.Equal(35, users[1].Age);
    }

    [Fact]
    public async Task Test_SqlServer_UpdateRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("UpdateRecordDb");
        var uniqueName = "UpdateUser_" + Guid.NewGuid().ToString()[..8];
        await db.ExecuteAsync("INSERT INTO Users (Name, Age) VALUES (@Name, @Age)", new Dictionary<string, object> { ["@Name"] = uniqueName, ["@Age"] = 40 });

        // Act
        var rowsAffected = await db.ExecuteAsync(
            "UPDATE Users SET Age = @NewAge WHERE Name = @Name",
            new Dictionary<string, object>
            {
                ["@NewAge"] = 41,
                ["@Name"] = uniqueName
            }
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var updatedUsers = await db.QueryAsync<User>("SELECT Age FROM Users WHERE Name = @Name", new Dictionary<string, object> { ["@Name"] = uniqueName });
        Assert.Single(updatedUsers);
        Assert.Equal(41, updatedUsers[0].Age);
    }

    [Fact]
    public async Task Test_SqlServer_DeleteRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("DeleteRecordDb");
        var uniqueName = "DeleteUser_" + Guid.NewGuid().ToString()[..8];
        await db.ExecuteAsync("INSERT INTO Users (Name, Age) VALUES (@Name, @Age)", new Dictionary<string, object> { ["@Name"] = uniqueName, ["@Age"] = 50 });

        // Act
        var rowsAffected = await db.ExecuteAsync(
            "DELETE FROM Users WHERE Name = @Name",
            new Dictionary<string, object>
            {
                ["@Name"] = uniqueName
            }
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var remainingUsers = await db.QueryAsync<User>("SELECT Id FROM Users WHERE Name = @Name", new Dictionary<string, object> { ["@Name"] = uniqueName });
        Assert.Empty(remainingUsers);
    }
}