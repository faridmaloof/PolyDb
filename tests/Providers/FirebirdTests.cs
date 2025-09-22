using PolyDb.Connection;
using PolyDb.Enums;
using PolyDb.Providers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PolyDb.Tests.Providers;

public class FirebirdTests
{
    private static IDbProvider GetConnectedPolyDb(string dbName)
    {
        // NOTE: For Firebird, you typically connect to a .fdb file.
        // Ensure the Firebird server is running and accessible, or the .fdb file exists.
        // Example connection string for an embedded server or local file:
        // "DataSource=localhost;Database=C:\Path\To\Your\Database.fdb;User=SYSDBA;Password=masterkey;"
        // For testing, you might want to create a temporary database file.
        string connectionString = $"DataSource=localhost;Database={dbName}.fdb;User=SYSDBA;Password=masterkey;";
        return PolyDbProviderFactory.CreateProvider(DatabaseType.Firebird, connectionString);
    }

    [Fact]
    public async Task Test_Firebird_CreateTable()
    {
        string dbName = "TestFirebirdDb_CreateTable";
        IDbProvider provider = GetConnectedPolyDb(dbName);

        string createTableSql = @"
                CREATE TABLE TestTable (
                    Id INTEGER NOT NULL PRIMARY KEY,
                    Name VARCHAR(50),
                    Value INTEGER
                );";

        await provider.ExecuteAsync(createTableSql);

        // Verify table creation (e.g., by trying to insert or query)
        string insertSql = "INSERT INTO TestTable (Id, Name, Value) VALUES (1, 'TestName', 100);";
        await provider.ExecuteAsync(insertSql);

        string dropTableSql = "DROP TABLE TestTable;";
        await provider.ExecuteAsync(dropTableSql);
    }

    [Fact]
    public async Task Test_Firebird_InsertRecord()
    {
        string dbName = "TestFirebirdDb_InsertRecord";
        IDbProvider provider = GetConnectedPolyDb(dbName);

        string createTableSql = @"
                CREATE TABLE TestTable (
                    Id INTEGER NOT NULL PRIMARY KEY,
                    Name VARCHAR(50),
                    Value INTEGER
                );";
        await provider.ExecuteAsync(createTableSql);

        string insertSql = "INSERT INTO TestTable (Id, Name, Value) VALUES (@Id, @Name, @Value);";
        var parameters = new Dictionary<string, object>
        {
            { "@Id", 2 },
            { "@Name", "InsertedName" },
            { "@Value", 200 }
        };
        await provider.ExecuteAsync(insertSql, parameters);

        string selectSql = "SELECT Name FROM TestTable WHERE Id = 2;";
        var result = await provider.QuerySingleAsync<Dictionary<string, object>>(selectSql);
        Assert.NotNull(result);
        Assert.Equal("InsertedName", result["Name"]);

        string dropTableSql = "DROP TABLE TestTable;";
        await provider.ExecuteAsync(dropTableSql);
    }

    [Fact]
    public async Task Test_Firebird_QueryRecords()
    {
        string dbName = "TestFirebirdDb_QueryRecords";
        IDbProvider provider = GetConnectedPolyDb(dbName);

        string createTableSql = @"
                CREATE TABLE TestTable (
                    Id INTEGER NOT NULL PRIMARY KEY,
                    Name VARCHAR(50),
                    Value INTEGER
                );";
        await provider.ExecuteAsync(createTableSql);

        string insertSql = "INSERT INTO TestTable (Id, Name, Value) VALUES (@Id, @Name, @Value);";
        await provider.ExecuteAsync(insertSql, new Dictionary<string, object> { { "@Id", 3 }, { "@Name", "QueryName1" }, { "@Value", 300 } });
        await provider.ExecuteAsync(insertSql, new Dictionary<string, object> { { "@Id", 4 }, { "@Name", "QueryName2" }, { "@Value", 400 } });

        string selectSql = "SELECT Id, Name, Value FROM TestTable WHERE Value > @MinValue;";
        var parameters = new Dictionary<string, object> { { "@MinValue", 250 } };
        var results = await provider.QueryAsync<Dictionary<string, object>>(selectSql, parameters);

        Assert.NotNull(results);
        Assert.Equal(2, results.Count());
        Assert.Contains(results, r => r["Name"].Equals("QueryName1"));
        Assert.Contains(results, r => r["Name"].Equals("QueryName2"));

        string dropTableSql = "DROP TABLE TestTable;";
        await provider.ExecuteAsync(dropTableSql);
    }

    [Fact]
    public async Task Test_Firebird_UpdateRecord()
    {
        string dbName = "TestFirebirdDb_UpdateRecord";
        IDbProvider provider = GetConnectedPolyDb(dbName);

        string createTableSql = @"
                CREATE TABLE TestTable (
                    Id INTEGER NOT NULL PRIMARY KEY,
                    Name VARCHAR(50),
                    Value INTEGER
                );";
        await provider.ExecuteAsync(createTableSql);

        string insertSql = "INSERT INTO TestTable (Id, Name, Value) VALUES (@Id, @Name, @Value);";
        await provider.ExecuteAsync(insertSql, new Dictionary<string, object> { { "@Id", 5 }, { "@Name", "OldName" }, { "@Value", 500 } });

        string updateSql = "UPDATE TestTable SET Name = @NewName WHERE Id = @Id;";
        var parameters = new Dictionary<string, object>
        {
            { "@NewName", "UpdatedName" },
            { "@Id", 5 }
        };
        await provider.ExecuteAsync(updateSql, parameters);

        string selectSql = "SELECT Name FROM TestTable WHERE Id = 5;";
        var result = await provider.QuerySingleAsync<Dictionary<string, object>>(selectSql);
        Assert.NotNull(result);
        Assert.Equal("UpdatedName", result["Name"]);

        string dropTableSql = "DROP TABLE TestTable;";
        await provider.ExecuteAsync(dropTableSql);
    }

    [Fact]
    public async Task Test_Firebird_DeleteRecord()
    {
        string dbName = "TestFirebirdDb_DeleteRecord";
        IDbProvider provider = GetConnectedPolyDb(dbName);

        string createTableSql = @"
                CREATE TABLE TestTable (
                    Id INTEGER NOT NULL PRIMARY KEY,
                    Name VARCHAR(50),
                    Value INTEGER
                );";
        await provider.ExecuteAsync(createTableSql);

        string insertSql = "INSERT INTO TestTable (Id, Name, Value) VALUES (@Id, @Name, @Value);";
        await provider.ExecuteAsync(insertSql, new Dictionary<string, object> { { "@Id", 6 }, { "@Name", "ToDelete" }, { "@Value", 600 } });

        string deleteSql = "DELETE FROM TestTable WHERE Id = @Id;";
        var parameters = new Dictionary<string, object> { { "@Id", 6 } };
        await provider.ExecuteAsync(deleteSql, parameters);

        string selectSql = "SELECT Name FROM TestTable WHERE Id = 6;";
        var result = await provider.QuerySingleAsync<Dictionary<string, object>>(selectSql);
        Assert.Null(result);

        string dropTableSql = "DROP TABLE TestTable;";
        await provider.ExecuteAsync(dropTableSql);
    }
}
