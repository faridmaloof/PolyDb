namespace PolyDb.Providers;

/// <summary>
/// Contract for database providers.
/// Defines basic execution and query operations.
/// </summary>
public interface IDbProvider : IAsyncDisposable
{
    /// <summary>
    /// Executes a command (INSERT, UPDATE, DELETE) against the database asynchronously.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters for the query.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteAsync(string query, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Executes a SELECT query and maps the results to objects of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of object to map (must have a parameterless constructor).</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters for the query.</param>
    /// <returns>A list of objects of type <typeparamref name="T"/>.</returns>
    Task<List<T>> QueryAsync<T>(string query, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Executes a SELECT query and maps the first result to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of object to map (must have a parameterless constructor).</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters for the query.</param>
    /// <returns>The first object of type <typeparamref name="T"/>, or default if not found.</returns>
    Task<T?> QuerySingleAsync<T>(string query, Dictionary<string, object>? parameters = null);
}