using MySql.Data.MySqlClient;

namespace PolyDb.Providers;

/// <summary>
/// Provides MySQL-specific implementation for database operations.
/// </summary>
internal sealed class MySqlProvider(string connectionString)
    : BaseProvider<MySqlConnection, MySqlCommand, MySqlParameter>(connectionString)
{

    /// <inheritdoc/>
    protected override MySqlConnection CreateConnection(string connectionString)
        => new(connectionString);
}