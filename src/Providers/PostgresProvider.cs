using Npgsql;

namespace PolyDb.Providers;

/// <summary>
/// Provides PostgreSQL-specific implementation for database operations.
/// </summary>
internal sealed class PostgresProvider(string connectionString)
    : BaseProvider<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>(connectionString)
{
    /// <inheritdoc/>
    protected override NpgsqlConnection CreateConnection(string connectionString)
        => new(connectionString);
}