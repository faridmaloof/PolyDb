using PolyDb.Enums;

namespace PolyDb.Tests.Providers;

/// <summary>
/// Test suite for MySQL using PolyDb.
/// </summary>
public class MySqlTests
{
    /// <summary>
    /// Defines a simple Customer class for testing.
    /// </summary>
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    private const string MySqlConnectionString = "Server=localhost;Port=3306;Database=testdb;Uid=root;Pwd=password;";

    private static async Task<PolyDb> GetConnectedPolyDb(string dbName = "PolyDb_test_db")
    {
        // Connect to a default database (e.g., 'mysql' or 'information_schema') to manage test databases
        await using var masterDb = PolyDb.Connect(DatabaseType.MySql, "Server=localhost;Port=3306;Database=mysql;Uid=root;Pwd=password;");
        
        // Drop existing database if it exists
        await masterDb.ExecuteAsync($"DROP DATABASE IF EXISTS {dbName};");
        // Create new database
        await masterDb.ExecuteAsync($"CREATE DATABASE {dbName};");

        // Connect to the newly created test database
        var connectionString = $"Server=localhost;Port=3306;Database={dbName};Uid=root;Pwd=password;";
        var db = PolyDb.Connect(DatabaseType.MySql, connectionString);
        
        // Create table for tests
        await db.ExecuteAsync("CREATE TABLE Customers (Id INT AUTO_INCREMENT PRIMARY KEY, Name VARCHAR(100), Email VARCHAR(100))");
        return db;
    }

    [Fact]
    public async Task Test_MySql_CreateTable()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("CreateTableDb_mysql");

        // Act & Assert (table created in GetConnectedPolyDb)
        var tableName = await db.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'customers'");
        Assert.Single(tableName);
        Assert.Equal("customers", tableName[0]);
    }

    [Fact]
    public async Task Test_MySql_InsertRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("InsertRecordDb_mysql");
        var uniqueName = "InsertCustomer_" + Guid.NewGuid().ToString()[..8];
        var uniqueEmail = uniqueName + "@example.com";

        // Act
        var rowsAffected = await db.ExecuteAsync(
            "INSERT INTO Customers (Name, Email) VALUES (@Name, @Email)",
            new Dictionary<string, object>
            {
                ["@Name"] = uniqueName,
                ["@Email"] = uniqueEmail
            }
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var customers = await db.QueryAsync<Customer>("SELECT Name FROM Customers WHERE Name = @Name", new Dictionary<string, object> { ["@Name"] = uniqueName });
        Assert.Single(customers);
        Assert.Equal(uniqueName, customers[0].Name);
    }

    [Fact]
    public async Task Test_MySql_QueryRecords()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("QueryRecordsDb_mysql");
        var uniqueName1 = "QueryCustomer1_" + Guid.NewGuid().ToString()[..8];
        var uniqueEmail1 = uniqueName1 + "@example.com";
        var uniqueName2 = "QueryCustomer2_" + Guid.NewGuid().ToString()[..8];
        var uniqueEmail2 = uniqueName2 + "@example.com";
        await db.ExecuteAsync("INSERT INTO Customers (Name, Email) VALUES (@Name, @Email)", new Dictionary<string, object> { ["@Name"] = uniqueName1, ["@Email"] = uniqueEmail1 });
        await db.ExecuteAsync("INSERT INTO Customers (Name, Email) VALUES (@Name, @Email)", new Dictionary<string, object> { ["@Name"] = uniqueName2, ["@Email"] = uniqueEmail2 });

        // Act
        var customers = await db.QueryAsync<Customer>(
            "SELECT Id, Name, Email FROM Customers WHERE Name LIKE 'QueryCustomer%' ORDER BY Name",
            new Dictionary<string, object>
            {
                ["@NamePattern"] = "QueryCustomer%"
            }
        );

        // Assert
        Assert.Equal(2, customers.Count);
        Assert.Equal(uniqueName1, customers[0].Name);
        Assert.Equal(uniqueEmail1, customers[0].Email);
        Assert.Equal(uniqueName2, customers[1].Name);
        Assert.Equal(uniqueEmail2, customers[1].Email);
    }

    [Fact]
    public async Task Test_MySql_UpdateRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("UpdateRecordDb_mysql");
        var uniqueName = "UpdateCustomer_" + Guid.NewGuid().ToString()[..8];
        var originalEmail = uniqueName + "@example.com";
        var newEmail = "new_" + originalEmail;
        await db.ExecuteAsync("INSERT INTO Customers (Name, Email) VALUES (@Name, @Email)", new Dictionary<string, object> { ["@Name"] = uniqueName, ["@Email"] = originalEmail });

        // Act
        var rowsAffected = await db.ExecuteAsync(
            "UPDATE Customers SET Email = @NewEmail WHERE Name = @Name",
            new Dictionary<string, object>
            {
                ["@NewEmail"] = newEmail,
                ["@Name"] = uniqueName
            }
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var updatedCustomers = await db.QueryAsync<Customer>("SELECT Email FROM Customers WHERE Name = @Name", new Dictionary<string, object> { ["@Name"] = uniqueName });
        Assert.Single(updatedCustomers);
        Assert.Equal(newEmail, updatedCustomers[0].Email);
    }

    [Fact]
    public async Task Test_MySql_DeleteRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("DeleteRecordDb_mysql");
        var uniqueName = "DeleteCustomer_" + Guid.NewGuid().ToString()[..8];
        var uniqueEmail = uniqueName + "@example.com";
        await db.ExecuteAsync("INSERT INTO Customers (Name, Email) VALUES (@Name, @Email)", new Dictionary<string, object> { ["@Name"] = uniqueName, ["@Email"] = uniqueEmail });

        // Act
        var rowsAffected = await db.ExecuteAsync(
            "DELETE FROM Customers WHERE Name = @Name",
            new Dictionary<string, object>
            {
                ["@Name"] = uniqueName
            }
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var remainingCustomers = await db.QueryAsync<Customer>("SELECT Id FROM Customers WHERE Name = @Name", new Dictionary<string, object> { ["@Name"] = uniqueName });
        Assert.Empty(remainingCustomers);
    }
}