# DataBridge

<p align="center">
  <a href="https://dotnet.microsoft.com/">
    <img src="https://img.shields.io/badge/.NET-9.0-purple?logo=dotnet" alt=".NET 9.0"/>
  </a>
  <a href="https://learn.microsoft.com/sql/">
    <img src="https://img.shields.io/badge/SQL%20Server-Supported-red?logo=microsoftsqlserver" alt="SQL Server"/>
  </a>
  <a href="https://www.mysql.com/">
    <img src="https://img.shields.io/badge/MySQL-Supported-blue?logo=mysql" alt="MySQL"/>
  </a>
  <a href="https://www.postgresql.org/">
    <img src="https://img.shields.io/badge/PostgreSQL-Supported-blue?logo=postgresql" alt="PostgreSQL"/>
  </a>
  <a href="https://www.oracle.com/database/">
    <img src="https://img.shields.io/badge/Oracle-Supported-red?logo=oracle" alt="Oracle"/>
  </a>
  <a href="https://www.sqlite.org/">
    <img src="https://img.shields.io/badge/SQLite-Supported-lightgrey?logo=sqlite" alt="SQLite"/>
  </a>
  <a href="https://firebirdsql.org/">
    <img src="https://img.shields.io/badge/Firebird-Supported-orange?logo=firebird" alt="Firebird"/>
  </a>
</p>

A robust and flexible .NET library designed to simplify database interactions across various relational database management systems. DataBridge provides a unified interface for common CRUD operations, abstracting away the complexities of vendor-specific ADO.NET implementations.

## Table of Contents
- [DataBridge](#databridge)
  - [Table of Contents](#table-of-contents)
  - [Features](#features)
  - [Technologies Used](#technologies-used)
  - [Architecture](#architecture)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
    - [Configuration](#configuration)
  - [Usage Examples](#usage-examples)
    - [Connecting to a Database](#connecting-to-a-database)
    - [Performing CRUD Operations](#performing-crud-operations)
  - [Extensibility](#extensibility)
    - [Future Enhancements](#future-enhancements)
  - [Contributing](#contributing)
  - [License](#license)

## Features
- **Multi-Database Support:** Seamlessly connect and interact with SQL Server, MySQL, PostgreSQL, Oracle, SQLite, and Firebird.
- **Unified API:** A consistent interface for common database operations, reducing learning curve and code duplication.
- **Parameterized Queries:** Built-in support for secure parameterized queries to prevent SQL injection.
- **Asynchronous Operations:** Leverage async/await for non-blocking database calls, improving application responsiveness.
- **Provider-Based Architecture:** Easily extendable to support new database systems.
- **Lightweight and Efficient:** Designed for performance with minimal overhead.

## Technologies Used
- **C#**
- **.NET 9.0** (or later)
- **Supported Databases:**
  - SQL Server (via `Microsoft.Data.SqlClient`)
  - MySQL (via `MySql.Data`)
  - PostgreSQL (via `Npgsql`)
  - Oracle (via `Oracle.ManagedDataAccess.Client`)
  - SQLite (via `System.Data.SQLite`)
  - Firebird (via `FirebirdSql.Data.FirebirdClient`)

## Architecture
DataBridge employs a provider-based architecture. The core library defines an `IDbProvider` interface, which outlines the standard database operations. Each supported database (e.g., SQL Server, MySQL) has its own concrete implementation of this interface (e.g., `SqlProvider`, `MySqlProvider`).

A `DataBridgeProviderFactory` is responsible for creating and managing instances of these providers based on the specified `DatabaseType`. This design promotes loose coupling and makes the system highly extensible.

```
+---------------------+       +---------------------+ 
| IDbProvider         | <-----| DataBridgeProvider  |
| (Interface)         |       | (Generic Base)      |
+---------------------+       +---------------------+ 
      ^ ^ ^ ^ ^ ^
      | | | | | |
      | | | | | +-------------------+
      | | | | +-----------------+   |
      | | | +-----------------+ |   |
      | | +-----------------+ | |   |
      | +-----------------+ | | |   |
      +-----------------+ | | | |   |
                        | | | | |   |
+-----------------+   +-------+-------+   +-----------------+   +-----------------+   +-----------------+   +-----------------+   +-----------------+
| SqlProvider     |   | MySqlProvider |   | PostgresProvider|   | OracleProvider  |   | SqLiteProvider  |   | FirebirdProvider|
+-----------------+   +-------+-------+   +-----------------+   +-----------------+   +-----------------+   +-----------------+   +-----------------+
```

## Getting Started

### Prerequisites
- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) or higher installed.
- A compatible IDE like Visual Studio or VS Code.
- Access to the database servers you wish to connect to (e.g., running MySQL, PostgreSQL, SQL Server, Oracle, Firebird instances).

### Installation
1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-username/DataBridge.git
   cd DataBridge
   ```
2. **Build the solution:**
   ```bash
   dotnet build PolyDb.sln
   ```
   This will restore NuGet packages and build both the `PolyDb` library and `PolyDb.Tests` project.

### Configuration
Connection strings are crucial for DataBridge to connect to your databases. You'll typically define these in your application's configuration (e.g., `appsettings.json` in a .NET Core application, or directly in your code for simpler scenarios).

Here are examples of connection strings for different database types:

```csharp
// SQL Server
string sqlServerConnString = "Server=localhost;Database=YourDb;User Id=YourUser;Password=YourPassword;";

// MySQL
string mySqlConnString = "Server=localhost;Port=3306;Database=YourDb;Uid=YourUser;Pwd=YourPassword;";

// PostgreSQL
string postgresConnString = "Host=localhost;Port=5432;Database=YourDb;Username=YourUser;Password=YourPassword;";

// Oracle
// Ensure Oracle Client is installed and TNSNAMES.ORA is configured, or use EZCONNECT syntax
string oracleConnString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=YourService)));User Id=YourUser;Password=YourPassword;";

// SQLite
string sqLiteConnString = "Data Source=./YourDb.db;";

// Firebird
// Example for an embedded server or local .fdb file. Adjust path as needed.
string firebirdConnString = "DataSource=localhost;Database=C:\Path\To\Your\Database.fdb;User=SYSDBA;Password=masterkey;";
```

## Usage Examples

### Connecting to a Database

```csharp
using PolyDb.Connection;
using PolyDb.Enums;
using PolyDb.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Example
{
    public static async Task Main(string[] args)
    {
        // Example for SQL Server
        string sqlServerConnString = "Server=localhost;Database=YourDb;User Id=YourUser;Password=YourPassword;";
        IDbProvider sqlProvider = DataBridgeProviderFactory.CreateProvider(DatabaseType.SqlServer, sqlServerConnString);

        // Example for MySQL
        string mySqlConnString = "Server=localhost;Port=3306;Database=YourDb;Uid=YourUser;Pwd=YourPassword;";
        IDbProvider mySqlProvider = DataBridgeProviderFactory.CreateProvider(DatabaseType.MySql, mySqlConnString);

        // Example for Firebird
        string firebirdConnString = "DataSource=localhost;Database=C:\Path\To\Your\Database.fdb;User=SYSDBA;Password=masterkey;";
        IDbProvider firebirdProvider = DataBridgeProviderFactory.CreateProvider(DatabaseType.Firebird, firebirdConnString);

        // You can now use sqlProvider, mySqlProvider, or firebirdProvider to interact with the respective databases
        // ...
    }
}
```

### Performing CRUD Operations

Let's assume you have a table named `Users` with columns `Id` (INT, PK), `Name` (VARCHAR), `Email` (VARCHAR).

```csharp
using PolyDb.Connection;
using PolyDb.Enums;
using PolyDb.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CrudExample
{
    public static async Task RunCrudOperations(IDbProvider provider)
    {
        // 1. Create Table (if not exists)
        string createTableSql = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INT PRIMARY KEY AUTO_INCREMENT, -- Adjust for specific DB (e.g., IDENTITY for SQL Server)
                Name VARCHAR(255) NOT NULL,
                Email VARCHAR(255) UNIQUE NOT NULL
            );";
        // Note: AUTO_INCREMENT is for MySQL. Use IDENTITY(1,1) for SQL Server, SERIAL for PostgreSQL, etc.
        // For a truly generic solution, you might need DB-specific DDL or an ORM.
        // This example assumes you adapt the DDL for your target DB.

        // For SQL Server:
        // CREATE TABLE Users ( Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(255) NOT NULL, Email NVARCHAR(255) UNIQUE NOT NULL );
        // For PostgreSQL:
        // CREATE TABLE Users ( Id SERIAL PRIMARY KEY, Name VARCHAR(255) NOT NULL, Email VARCHAR(255) UNIQUE NOT NULL );
        // For Oracle:
        // CREATE TABLE Users ( Id NUMBER GENERATED BY DEFAULT ON NULL AS IDENTITY, Name VARCHAR2(255) NOT NULL, Email VARCHAR2(255) UNIQUE NOT NULL, PRIMARY KEY (Id) );
        // For SQLite:
        // CREATE TABLE Users ( Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Email TEXT UNIQUE NOT NULL );
        // For Firebird:
        // CREATE TABLE Users ( Id INTEGER NOT NULL PRIMARY KEY, Name VARCHAR(255), Email VARCHAR(255) );

        await provider.ExecuteAsync(createTableSql);
        Console.WriteLine("Table 'Users' ensured.");

        // 2. Insert Record
        string insertSql = "INSERT INTO Users (Name, Email) VALUES (@Name, @Email);";
        var insertParams = new Dictionary<string, object>
        {
            { "@Name", "John Doe" },
            { "@Email", "john.doe@example.com" }
        };
        await provider.ExecuteAsync(insertSql, insertParams);
        Console.WriteLine("Inserted John Doe.");

        // 3. Query Records
        string selectSql = "SELECT Id, Name, Email FROM Users WHERE Name = @Name;";
        var selectParams = new Dictionary<string, object> { { "@Name", "John Doe" } };
        var users = await provider.QueryAsync(selectSql, selectParams);

        Console.WriteLine("Queried Users:");
        foreach (var user in users)
        {
            Console.WriteLine($"- Id: {user["Id"]}, Name: {user["Name"]}, Email: {user["Email"]}");
        }

        // 4. Update Record
        string updateSql = "UPDATE Users SET Email = @NewEmail WHERE Name = @Name;";
        var updateParams = new Dictionary<string, object>
        {
            { "@NewEmail", "john.newemail@example.com" },
            { "@Name", "John Doe" }
        };
        await provider.ExecuteAsync(updateSql, updateParams);
        Console.WriteLine("Updated John Doe's email.");

        // 5. Delete Record
        string deleteSql = "DELETE FROM Users WHERE Name = @Name;";
        var deleteParams = new Dictionary<string, object> { { "@Name", "John Doe" } };
        await provider.ExecuteAsync(deleteSql, deleteParams);
        Console.WriteLine("Deleted John Doe.");
    }
}
```

## Extensibility

The provider-based architecture makes DataBridge highly extensible. To add support for a new database:
1.  Create a new class that implements the `IDbProvider` interface.
2.  Implement all the required methods (e.g., `ExecuteAsync`, `QueryAsync`, `QuerySingleAsync`).
3.  Register your new provider with the `DataBridgeProviderFactory` (or extend the factory to recognize a new `DatabaseType` enum value).

### Future Enhancements
- **NoSQL Support:** Extend the `IDbProvider` interface or create a separate `INoSqlProvider` to support NoSQL databases like MongoDB, Cassandra, or Azure Cosmos DB.
- **ORM Integration:** Provide helper methods or an optional layer for basic ORM functionalities.
- **Advanced Query Building:** Implement a fluent API or a query builder for more complex queries without writing raw SQL.
- **Connection Pooling Configuration:** Expose more fine-grained control over connection pooling settings.
- **Transaction Management:** Explicit transaction support.

## Contributing
Contributions are welcome! Please feel free to fork the repository, open issues, and submit pull requests.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
**Attribution Requirement:** When using, modifying, or distributing this software, you must provide clear attribution to the original author(s) by including the copyright notice and a link to the original repository (if applicable) in a prominent place.


A robust and flexible .NET library designed to simplify database interactions across various relational database management systems. DataBridge provides a unified interface for common CRUD operations, abstracting away the complexities of vendor-specific ADO.NET implementations.

## Table of Contents
- [DataBridge](#databridge)
  - [Table of Contents](#table-of-contents)
  - [Features](#features)
  - [Technologies Used](#technologies-used)
  - [Architecture](#architecture)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
    - [Configuration](#configuration)
  - [Usage Examples](#usage-examples)
    - [Connecting to a Database](#connecting-to-a-database)
    - [Performing CRUD Operations](#performing-crud-operations)
  - [Extensibility](#extensibility)
    - [Future Enhancements](#future-enhancements)
  - [Contributing](#contributing)
  - [License](#license)

## Features
- **Multi-Database Support:** Seamlessly connect and interact with SQL Server, MySQL, PostgreSQL, Oracle, and SQLite.
- **Unified API:** A consistent interface for common database operations, reducing learning curve and code duplication.
- **Parameterized Queries:** Built-in support for secure parameterized queries to prevent SQL injection.
- **Asynchronous Operations:** Leverage async/await for non-blocking database calls, improving application responsiveness.
- **Provider-Based Architecture:** Easily extendable to support new database systems.
- **Lightweight and Efficient:** Designed for performance with minimal overhead.

## Technologies Used
- **C#**
- **.NET 9.0** (or later)
- **Supported Databases:**
  - SQL Server (via `Microsoft.Data.SqlClient`)
  - MySQL (via `MySql.Data`)
  - PostgreSQL (via `Npgsql`)
  - Oracle (via `Oracle.ManagedDataAccess.Client`)
  - SQLite (via `System.Data.SQLite`)

## Architecture
DataBridge employs a provider-based architecture. The core library defines an `IDbProvider` interface, which outlines the standard database operations. Each supported database (e.g., SQL Server, MySQL) has its own concrete implementation of this interface (e.g., `SqlProvider`, `MySqlProvider`).

A `DataBridgeProviderFactory` is responsible for creating and managing instances of these providers based on the specified `DatabaseType`. This design promotes loose coupling and makes the system highly extensible.

```
+---------------------+       +---------------------+
| IDbProvider         | <-----| DataBridgeProvider  |
| (Interface)         |       | (Generic Base)      |
+---------------------+       +---------------------+
      ^ ^ ^ ^ ^
      | | | | |
      | | | | +-------------------+
      | | | +-----------------+   |
      | | +-----------------+ |   |
      | +-----------------+ | |   |
      +-----------------+ | | |   |
                        | | | |   |
+-----------------+   +-------+-------+   +-----------------+   +-----------------+   +-----------------+   +-----------------+
| SqlProvider     |   | MySqlProvider |   | PostgresProvider|   | OracleProvider  |   | SqLiteProvider  |
+-----------------+   +-------+-------+   +-----------------+   +-----------------+   +-----------------+   +-----------------+
```

## Getting Started

### Prerequisites
- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) or higher installed.
- A compatible IDE like Visual Studio or VS Code.
- Access to the database servers you wish to connect to (e.g., running MySQL, PostgreSQL, SQL Server, Oracle instances).

### Installation
1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-username/DataBridge.git
   cd DataBridge
   ```
2. **Build the solution:**
   ```bash
   dotnet build PolyDb.sln
   ```
   This will restore NuGet packages and build both the `PolyDb` library and `PolyDb.Tests` project.

### Configuration
Connection strings are crucial for DataBridge to connect to your databases. You'll typically define these in your application's configuration (e.g., `appsettings.json` in a .NET Core application, or directly in your code for simpler scenarios).

Here are examples of connection strings for different database types:

```csharp
// SQL Server
string sqlServerConnString = "Server=localhost;Database=YourDb;User Id=YourUser;Password=YourPassword;";

// MySQL
string mySqlConnString = "Server=localhost;Port=3306;Database=YourDb;Uid=YourUser;Pwd=YourPassword;";

// PostgreSQL
string postgresConnString = "Host=localhost;Port=5432;Database=YourDb;Username=YourUser;Password=YourPassword;";

// Oracle
// Ensure Oracle Client is installed and TNSNAMES.ORA is configured, or use EZCONNECT syntax
string oracleConnString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=YourService)));User Id=YourUser;Password=YourPassword;";

// SQLite
string sqLiteConnString = "Data Source=./YourDb.db;";
```

## Usage Examples

### Connecting to a Database

```csharp
using PolyDb.Connection;
using PolyDb.Enums;
using PolyDb.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Example
{
    public static async Task Main(string[] args)
    {
        // Example for SQL Server
        string sqlServerConnString = "Server=localhost;Database=YourDb;User Id=YourUser;Password=YourPassword;";
        IDbProvider sqlProvider = DataBridgeProviderFactory.GetProvider(DatabaseType.SqlServer, sqlServerConnString);

        // Example for MySQL
        string mySqlConnString = "Server=localhost;Port=3306;Database=YourDb;Uid=YourUser;Pwd=YourPassword;";
        IDbProvider mySqlProvider = DataBridgeProviderFactory.GetProvider(DatabaseType.MySql, mySqlConnString);

        // You can now use sqlProvider or mySqlProvider to interact with the respective databases
        // ...
    }
}
```

### Performing CRUD Operations

Let's assume you have a table named `Users` with columns `Id` (INT, PK), `Name` (VARCHAR), `Email` (VARCHAR).

```csharp
using PolyDb.Connection;
using PolyDb.Enums;
using PolyDb.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CrudExample
{
    public static async Task RunCrudOperations(IDbProvider provider)
    {
        // 1. Create Table (if not exists)
        string createTableSql = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INT PRIMARY KEY AUTO_INCREMENT, -- Adjust for specific DB (e.g., IDENTITY for SQL Server)
                Name VARCHAR(255) NOT NULL,
                Email VARCHAR(255) UNIQUE NOT NULL
            );";
        // Note: AUTO_INCREMENT is for MySQL. Use IDENTITY(1,1) for SQL Server, SERIAL for PostgreSQL, etc.
        // For a truly generic solution, you might need DB-specific DDL or an ORM.
        // This example assumes you adapt the DDL for your target DB.

        // For SQL Server:
        // CREATE TABLE Users ( Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(255) NOT NULL, Email NVARCHAR(255) UNIQUE NOT NULL );
        // For PostgreSQL:
        // CREATE TABLE Users ( Id SERIAL PRIMARY KEY, Name VARCHAR(255) NOT NULL, Email VARCHAR(255) UNIQUE NOT NULL );
        // For Oracle:
        // CREATE TABLE Users ( Id NUMBER GENERATED BY DEFAULT ON NULL AS IDENTITY, Name VARCHAR2(255) NOT NULL, Email VARCHAR2(255) UNIQUE NOT NULL, PRIMARY KEY (Id) );
        // For SQLite:
        // CREATE TABLE Users ( Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Email TEXT UNIQUE NOT NULL );

        await provider.ExecuteAsync(createTableSql);
        Console.WriteLine("Table 'Users' ensured.");

        // 2. Insert Record
        string insertSql = "INSERT INTO Users (Name, Email) VALUES (@Name, @Email);";
        var insertParams = new Dictionary<string, object>
        {
            { "@Name", "John Doe" },
            { "@Email", "john.doe@example.com" }
        };
        await provider.ExecuteAsync(insertSql, insertParams);
        Console.WriteLine("Inserted John Doe.");

        // 3. Query Records
        string selectSql = "SELECT Id, Name, Email FROM Users WHERE Name = @Name;";
        var selectParams = new Dictionary<string, object> { { "@Name", "John Doe" } };
        var users = await provider.QueryAsync(selectSql, selectParams);

        Console.WriteLine("Queried Users:");
        foreach (var user in users)
        {
            Console.WriteLine($"- Id: {user["Id"]}, Name: {user["Name"]}, Email: {user["Email"]}");
        }

        // 4. Update Record
        string updateSql = "UPDATE Users SET Email = @NewEmail WHERE Name = @Name;";
        var updateParams = new Dictionary<string, object>
        {
            { "@NewEmail", "john.newemail@example.com" },
            { "@Name", "John Doe" }
        };
        await provider.ExecuteAsync(updateSql, updateParams);
        Console.WriteLine("Updated John Doe's email.");

        // 5. Delete Record
        string deleteSql = "DELETE FROM Users WHERE Name = @Name;";
        var deleteParams = new Dictionary<string, object> { { "@Name", "John Doe" } };
        await provider.ExecuteAsync(deleteSql, deleteParams);
        Console.WriteLine("Deleted John Doe.");
    }
}
```

## Extensibility

The provider-based architecture makes DataBridge highly extensible. To add support for a new database:
1.  Create a new class that implements the `IDbProvider` interface.
2.  Implement all the required methods (e.g., `ExecuteAsync`, `QueryAsync`, `QuerySingleAsync`).
3.  Register your new provider with the `DataBridgeProviderFactory` (or extend the factory to recognize a new `DatabaseType` enum value).

### Future Enhancements
- **NoSQL Support:** Extend the `IDbProvider` interface or create a separate `INoSqlProvider` to support NoSQL databases like MongoDB, Cassandra, or Azure Cosmos DB.
- **ORM Integration:** Provide helper methods or an optional layer for basic ORM functionalities.
- **Advanced Query Building:** Implement a fluent API or a query builder for more complex queries without writing raw SQL.
- **Connection Pooling Configuration:** Expose more fine-grained control over connection pooling settings.
- **Transaction Management:** Explicit transaction support.

## Contributing
Contributions are welcome! Please feel free to fork the repository, open issues, and submit pull requests.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
**Attribution Requirement:** When using, modifying, or distributing this software, you must provide clear attribution to the original author(s) by including the copyright notice and a link to the original repository (if applicable) in a prominent place.
