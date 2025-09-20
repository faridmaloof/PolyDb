using Microsoft.Data.SqlClient;

namespace PolyDb.Providers;

/// <summary>
/// Provides SQL Server-specific implementation for database operations.
/// </summary>
internal sealed class SqlProvider(string connectionString) 
    : BaseProvider<SqlConnection, SqlCommand, SqlParameter>(connectionString)
{
    /// <inheritdoc/>
    protected override SqlConnection CreateConnection(string connectionString)
        => new(connectionString);
}