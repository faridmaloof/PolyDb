using FirebirdSql.Data.FirebirdClient;

namespace PolyDb.Providers;

/// <summary>
/// Provides Firebird-specific database operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FirebirdProvider"/> class.
/// </remarks>
/// <param name="connectionString">The connection string for the Firebird database.</param>
internal class FirebirdProvider(string connectionString) : BaseProvider<FbConnection, FbCommand, FbParameter>(connectionString)
{


    /// <summary>
    /// Creates a new Firebird connection.
    /// </summary>
    /// <returns>A new instance of <see cref="FbConnection"/>.</returns>
    protected override FbConnection CreateConnection(string connectionString) 
    => new(connectionString);
}
