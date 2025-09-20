using System.Data.SQLite;

namespace PolyDb.Providers;

/// <summary>
/// Provides SQLite-specific implementation for database operations.
/// </summary>
internal sealed class SqLiteProvider(string connectionString)
    : BaseProvider<SQLiteConnection, SQLiteCommand, SQLiteParameter>(connectionString)
{
    /// <inheritdoc/>
    protected override SQLiteConnection CreateConnection(string connectionString)
        => new(connectionString);
}