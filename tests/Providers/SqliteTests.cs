using PolyDb.Enums;

namespace PolyDb.Tests.Providers;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

/// <summary>
/// Test suite for SQLite using PolyDb.
/// Each instance of this class creates a clean database
/// and disposes of it after the tests are finished.
/// </summary>
public class SqliteTests : IDisposable
{
    private const string SqliteDbFileName = "D:\\db\\test.db";
    private const string SqliteConnectionString = "Data Source=" + SqliteDbFileName + ";";
    private bool _disposed;

    public SqliteTests()
    {
        ResetDatabaseFile();
    }

    /// <summary>
    /// Deletes the database file if it exists.
    /// </summary>
    private static void ResetDatabaseFile()
    {
        try
        {
            if (File.Exists(SqliteDbFileName))
                File.Delete(SqliteDbFileName);

            // Ensure the directory exists (in case it was accidentally deleted)
            var directory = Path.GetDirectoryName(SqliteDbFileName);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException(
                $"Failed to clean up the test database at {SqliteDbFileName}", ex);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        ResetDatabaseFile();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    private static async Task<PolyDb> GetConnectedPolyDb()
    {
        var db = PolyDb.Connect(DatabaseType.SqLite, SqliteConnectionString);
        await db.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Age INTEGER)");
        return db;
    }

    
    [Fact]
    public async Task Test_Sqlite_CreateTable()
    {
        await using var db = PolyDb.Connect(DatabaseType.SqLite, SqliteConnectionString);
        await db.ExecuteAsync("CREATE TABLE IF NOT EXISTS TestTable (Id INTEGER PRIMARY KEY)");

        var rowsAffected = await db.ExecuteAsync("INSERT INTO TestTable DEFAULT VALUES");
        Assert.Equal(1, rowsAffected);
    }

    [Fact]
    public async Task Test_Sqlite_InsertRecord()
    {
        await using var db = await GetConnectedPolyDb();
        var uniqueName = $"InsertUser_{Guid.NewGuid().ToString()[..8]}";

        var rowsAffected = await db.ExecuteAsync(
            "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)",
            new Dictionary<string, object>
            {
                ["@Name"] = uniqueName,
                ["@Age"] = 25
            }
        );

        Assert.Equal(1, rowsAffected);
        var users = await db.QueryAsync<User>(
            "SELECT Name FROM Users WHERE Name = @Name",
            new Dictionary<string, object> { ["@Name"] = uniqueName });

        Assert.Single(users);
        Assert.Equal(uniqueName, users[0].Name);
    }

    [Fact]
    public async Task Test_Sqlite_QueryRecords()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb();
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
    public async Task Test_Sqlite_UpdateRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb();
        var uniqueName = $"UpdateUser_{Guid.NewGuid().ToString()[..8]}";
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
    public async Task Test_Sqlite_DeleteRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb();
        var uniqueName = $"DeleteUser_{Guid.NewGuid().ToString()[..8]}";
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
