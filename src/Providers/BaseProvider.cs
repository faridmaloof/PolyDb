using System.Data;
using System.Data.Common;
using System.Reflection;

namespace PolyDb.Providers;

/// <summary>
/// Base class for database providers, offering common functionality for database interactions.
/// </summary>
/// <typeparam name="TConnection">The type of the database connection.</typeparam>
/// <typeparam name="TCommand">The type of the database command.</typeparam>
/// <typeparam name="TParameter">The type of the database parameter.</typeparam>
/// <param name="connectionString">The connection string for the database.</param>
internal abstract class BaseProvider<TConnection, TCommand, TParameter>(string connectionString) : IDbProvider
    where TConnection : DbConnection
    where TCommand : DbCommand
    where TParameter : DbParameter
{
    private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    private bool _disposed;

    /// <summary>
    /// Method that each provider must implement to create a specific connection.
    /// </summary>
    protected abstract TConnection CreateConnection(string connectionString);

    /// <inheritdoc/>
    public async Task<int> ExecuteAsync(string query, Dictionary<string, object>? parameters = null)
    {
        EnsureNotDisposed();

        await using var connection = CreateConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = (TCommand)connection.CreateCommand();
        command.CommandText = query;
        AddParameters(command, parameters);

        return await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<List<T>> QueryAsync<T>(string query, Dictionary<string, object>? parameters = null)
    {
        EnsureNotDisposed();

        await using var connection = CreateConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = (TCommand)connection.CreateCommand();
        command.CommandText = query;
        AddParameters(command, parameters);

        var result = new List<T>();
        await using var reader = await command.ExecuteReaderAsync();

        var isSimpleType = typeof(T).IsPrimitive ||
                           typeof(T) == typeof(string) ||
                           typeof(T) == typeof(decimal) ||
                           typeof(T) == typeof(DateTime) ||
                           typeof(T) == typeof(Guid) ||
                           (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>) && typeof(T).GetGenericArguments()[0].IsPrimitive);

        while (await reader.ReadAsync())
        {
            if (isSimpleType)
            {
                if (reader.FieldCount > 0)
                {
                    var value = reader.GetValue(0);
                    if (value == DBNull.Value)
                        result.Add(default!);
                    else
                        result.Add((T)Convert.ChangeType(value, typeof(T)));
                }
                else
                    result.Add(default!);
            }
            else
                result.Add(MapReader<T>(reader));
        }

        return result;
    }

    /// <summary>
    /// Maps the current row of the <see cref="IDataReader"/> to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to map to.</typeparam>
    /// <param name="reader">The data reader.</param>
    /// <returns>An object of type <typeparamref name="T"/> populated with data from the reader.</returns>
    protected virtual T MapReader<T>(IDataReader reader)
    {
        var item = Activator.CreateInstance<T>();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var value = reader.GetValue(i);
            if (value == DBNull.Value) continue;

            var property = typeof(T).GetProperty(reader.GetName(i), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null && property.CanWrite)
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (value.GetType() != targetType)
                    value = Convert.ChangeType(value, targetType);
                property.SetValue(item, value);
            }
        }

        return item;
    }

    /// <summary>
    /// Adds parameters to the database command.
    /// </summary>
    /// <param name="command">The database command.</param>
    /// <param name="parameters">The dictionary of parameters to add.</param>
    protected virtual void AddParameters(TCommand command, Dictionary<string, object>? parameters)
    {
        if (parameters == null) return;

        foreach (var param in parameters)
        {
            var dbParameter = command.CreateParameter();
            dbParameter.ParameterName = param.Key;
            dbParameter.Value = param.Value ?? DBNull.Value;
            command.Parameters.Add(dbParameter);
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (_disposed) return ValueTask.CompletedTask;

        _disposed = true;
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Ensures that the provider has not been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the provider has already been disposed.</exception>
    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name,
                "The provider has already been disposed and cannot be used again.");
    }
}