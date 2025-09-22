using System.Data;
using PolyDb.Enums;
using PolyDb.Providers;
using Microsoft.Data.SqlClient;
using Npgsql;
using MySql.Data.MySqlClient;
using System.Data.SQLite;
using Oracle.ManagedDataAccess.Client;
using FirebirdSql.Data.FirebirdClient; // Added for Firebird support

namespace PolyDb.Connection;

/// <summary>
/// Factory for creating database providers and connections based on the database type.
/// </summary>
public static class PolyDbProviderFactory
{
    /// <summary>
    /// Creates an <see cref="IDbProvider"/> instance for the specified database type.
    /// </summary>
    /// <param name="dbType">The type of the database.</param>
    /// <param name="connectionString">The connection string for the database.</param>
    /// <returns>An instance of <see cref="IDbProvider"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the connection string is null or empty.</exception>
    /// <exception cref="NotSupportedException">Thrown when the database type is not supported.</exception>
    public static IDbProvider CreateProvider(DatabaseType dbType, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        return dbType switch
        {
            DatabaseType.SqlServer => new SqlProvider(connectionString),
            DatabaseType.Postgres => new PostgresProvider(connectionString),
            DatabaseType.MySql => new MySqlProvider(connectionString),
            DatabaseType.MariaDb => new MySqlProvider(connectionString), // MariaDB uses the same connector as MySQL
            DatabaseType.SqLite => new SqLiteProvider(connectionString),
            DatabaseType.Oracle => new OracleProvider(connectionString),
            DatabaseType.Firebird => new FirebirdProvider(connectionString), // Added for Firebird support
            _ => throw new NotSupportedException($"Database type '{dbType}' is not supported.")
        };
    }

    /// <summary>
    /// Creates an <see cref="IDbConnection"/> instance for the specified database type.
    /// </summary>
    /// <param name="dbType">The type of the database.</param>
    /// <param name="connectionString">The connection string for the database.</param>
    /// <returns>An instance of <see cref="IDbConnection"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the connection string is null or empty.</exception>
    /// <exception cref="NotSupportedException">Thrown when the database type is not supported.</exception>
    public static IDbConnection CreateConnection(DatabaseType dbType, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        return dbType switch
        {
            DatabaseType.SqlServer => new SqlConnection(connectionString),
            DatabaseType.Postgres => new NpgsqlConnection(connectionString),
            DatabaseType.MySql => new MySqlConnection(connectionString),
            DatabaseType.MariaDb => new MySqlConnection(connectionString),
            DatabaseType.SqLite => new SQLiteConnection(connectionString),
            DatabaseType.Oracle => new OracleConnection(connectionString),
            DatabaseType.Firebird => new FbConnection(connectionString), // Added for Firebird support
            _ => throw new NotSupportedException($"Database type '{dbType}' is not supported.")
        };
    }
}
