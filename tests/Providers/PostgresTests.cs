using PolyDb.Enums;

namespace PolyDb.Tests.Providers;

/// <summary>
/// Test suite for PostgreSQL using PolyDb.
/// </summary>
public class PostgresTests
{
    /// <summary>
    /// Defines a simple Product class for testing.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }

    private const string PostgresConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=password;";

    private static async Task<PolyDb> GetConnectedPolyDb(string dbName = "PolyDb_test_db")
    {
        // Connect to default 'postgres' database to create/drop the test database
        await using var masterDb = PolyDb.Connect(DatabaseType.Postgres, "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=password;");
        
        // Drop existing database if it exists
        await masterDb.ExecuteAsync($"DROP DATABASE IF EXISTS {dbName};");
        // Create new database
        await masterDb.ExecuteAsync($"CREATE DATABASE {dbName};");

        // Connect to the newly created test database
        var connectionString = $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=password;";
        var db = PolyDb.Connect(DatabaseType.Postgres, connectionString);
        
        // Create table for tests
        await db.ExecuteAsync("CREATE TABLE Products (Id SERIAL PRIMARY KEY, Name VARCHAR(100), Price NUMERIC)");
        return db;
    }

    [Fact]
    public async Task Test_Postgres_CreateTable()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("CreateTableDb_pg");

        // Act & Assert (table created in GetConnectedPolyDb)
        var tableName = await db.QueryAsync<string>("SELECT tablename FROM pg_tables WHERE schemaname = 'public' AND tablename = 'products'");
        Assert.Single(tableName);
        Assert.Equal("products", tableName[0]);
    }

    [Fact]
    public async Task Test_Postgres_InsertRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("InsertRecordDb_pg");
        var uniqueName = "InsertProduct_" + Guid.NewGuid().ToString()[..8];

        // Act
        var rowsAffected = await db.ExecuteAsync(
            "INSERT INTO Products (Name, Price) VALUES (@Name, @Price)",
            new Dictionary<string, object>
            {
                ["@Name"] = uniqueName,
                ["@Price"] = 100.50m
            }
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var products = await db.QueryAsync<Product>("SELECT Name FROM Products WHERE Name = @Name", new Dictionary<string, object> { ["@Name"] = uniqueName });
        Assert.Single(products);
        Assert.Equal(uniqueName, products[0].Name);
    }

    [Fact]
    public async Task Test_Postgres_QueryRecords()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("QueryRecordsDb_pg");
        var uniqueName1 = "QueryProduct1_" + Guid.NewGuid().ToString()[..8];
        var uniqueName2 = "QueryProduct2_" + Guid.NewGuid().ToString()[..8];
        await db.ExecuteAsync("INSERT INTO Products (Name, Price) VALUES (@Name, @Price)", new Dictionary<string, object> { ["@Name"] = uniqueName1, ["@Price"] = 200.00m });
        await db.ExecuteAsync("INSERT INTO Products (Name, Price) VALUES (@Name, @Price)", new Dictionary<string, object> { ["@Name"] = uniqueName2, ["@Price"] = 300.00m });

        // Act
        var products = await db.QueryAsync<Product>(
            "SELECT Id, Name, Price FROM Products WHERE Price > @MinPrice ORDER BY Price",
            new Dictionary<string, object>
            {
                ["@MinPrice"] = 150.00m
            }
        );

        // Assert
        Assert.Equal(2, products.Count);
        Assert.Equal(uniqueName1, products[0].Name);
        Assert.Equal(200.00m, products[0].Price);
        Assert.Equal(uniqueName2, products[1].Name);
        Assert.Equal(300.00m, products[1].Price);
    }

    [Fact]
    public async Task Test_Postgres_UpdateRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("UpdateRecordDb_pg");
        var uniqueName = "UpdateProduct_" + Guid.NewGuid().ToString()[..8];
        await db.ExecuteAsync("INSERT INTO Products (Name, Price) VALUES (@Name, @Price)", new Dictionary<string, object> { ["@Name"] = uniqueName, ["@Price"] = 400.00m });

        // Act
        var rowsAffected = await db.ExecuteAsync(
            "UPDATE Products SET Price = @NewPrice WHERE Name = @Name",
            new Dictionary<string, object>
            {
                ["@NewPrice"] = 450.00m,
                ["@Name"] = uniqueName
            }
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var updatedProducts = await db.QueryAsync<Product>("SELECT Price FROM Products WHERE Name = @Name", new Dictionary<string, object> { ["@Name"] = uniqueName });
        Assert.Single(updatedProducts);
        Assert.Equal(450.00m, updatedProducts[0].Price);
    }

    [Fact]
    public async Task Test_Postgres_DeleteRecord()
    {
        // Arrange
        await using var db = await GetConnectedPolyDb("DeleteRecordDb_pg");
        var uniqueName = "DeleteProduct_" + Guid.NewGuid().ToString()[..8];
        await db.ExecuteAsync("INSERT INTO Products (Name, Price) VALUES (@Name, @Price)", new Dictionary<string, object> { ["@Name"] = uniqueName, ["@Price"] = 500.00m });

        // Act
        var rowsAffected = await db.ExecuteAsync(
            "DELETE FROM Products WHERE Name = @Name",
            new Dictionary<string, object>
            {
                ["@Name"] = uniqueName
            }
        );

        // Assert
        Assert.Equal(1, rowsAffected);
        var remainingProducts = await db.QueryAsync<Product>("SELECT Id FROM Products WHERE Name = @Name", new Dictionary<string, object> { ["@Name"] = uniqueName });
        Assert.Empty(remainingProducts);
    }
}