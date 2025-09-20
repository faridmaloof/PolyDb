using Oracle.ManagedDataAccess.Client;

namespace PolyDb.Providers;

/// <summary>
/// Provides Oracle-specific implementation for database operations.
/// </summary>
internal sealed class OracleProvider(string connectionString)
    : BaseProvider<OracleConnection, OracleCommand, OracleParameter>(connectionString)
{
    /// <inheritdoc/>
    protected override OracleConnection CreateConnection(string connectionString)
        => new(connectionString);


    /// <inheritdoc/>
    protected override void AddParameters(OracleCommand command, Dictionary<string, object>? parameters)
    {
        if (parameters == null) return;

        foreach (var param in parameters)
        {
            var dbParameter = command.CreateParameter();
            dbParameter.ParameterName = param.Key.StartsWith(':') ? param.Key : $":{param.Key}";
            dbParameter.Value = param.Value ?? DBNull.Value;
            command.Parameters.Add(dbParameter);
        }
    }
}