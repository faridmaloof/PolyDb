using PolyDb.Connection;
using PolyDb.Enums;
using PolyDb.Providers;

namespace PolyDb;

/// <summary>
/// Main entry point for interacting with the database.
/// Wraps a concrete <see cref="IDbProvider"/> (SQL Server, Postgres, MySQL, etc.)
/// and exposes common execution and query operations.
/// </summary>
/// <param name="provider">The underlying database provider.</param>
public sealed class PolyDb(IDbProvider provider) : IDbProvider
{
    private readonly IDbProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    private bool _disposed;

    /// <summary>
    /// Creates a new <see cref="PolyDb"/> instance for the specified database type.
    /// </summary>
    /// <param name="dbType">The type of database (SQL Server, Postgres, etc.).</param>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <returns>An instance ready to execute commands or queries.</returns>
    public static PolyDb Connect(DatabaseType dbType, string connectionString)
    {
        var provider = PolyDbProviderFactory.CreateProvider(dbType, connectionString);
        return new PolyDb(provider);
    }

    /// <inheritdoc/>
    public Task<int> ExecuteAsync(string query, Dictionary<string, object>? parameters = null)
    {
        EnsureNotDisposed();
        return _provider.ExecuteAsync(query, parameters);
    }

    /// <inheritdoc/>
    public Task<List<T>> QueryAsync<T>(string query, Dictionary<string, object>? parameters = null)
    {
        EnsureNotDisposed();
        return _provider.QueryAsync<T>(query, parameters);
    }

    /// <summary>
    /// Releases the resources associated with the underlying provider.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        try
        {
            if (_provider != null)
                await _provider.DisposeAsync();
        }
        finally
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PolyDb),
                "The PolyDb instance has already been disposed and cannot be used again.");
    }
}