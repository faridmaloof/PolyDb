namespace PolyDb.Enums;

using System.ComponentModel;

/// <summary>
/// Tipos de motores de base de datos soportados por la aplicación.
/// </summary>
public enum DatabaseType
{
    /// <summary>
    /// Microsoft SQL Server.
    /// </summary>
    [Description("Microsoft SQL Server")]
    SqlServer,

    /// <summary>
    /// PostgreSQL.
    /// </summary>
    [Description("PostgreSQL")]
    Postgres,

    /// <summary>
    /// MySQL.
    /// </summary>
    [Description("MySQL")]
    MySql,

    /// <summary>
    /// MariaDB.
    /// </summary>
    [Description("MariaDB")]
    MariaDb,

    /// <summary>
    /// SQLite.
    /// </summary>
    [Description("SQLite")]
    SqLite,

    /// <summary>
    /// Oracle Database.
    /// </summary>
    [Description("Oracle Database")]
    Oracle
}
