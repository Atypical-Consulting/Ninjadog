namespace Ninjadog.Templates.CrudWebAPI.Template.Database;

/// <summary>
/// Shared utilities for database provider-specific SQL generation.
/// </summary>
internal static class DatabaseProviderHelper
{
    /// <summary>
    /// Returns the SQL function for getting the current timestamp for the given provider.
    /// </summary>
    internal static string GetNowFunction(string provider)
    {
        return provider switch
        {
            "postgresql" => "NOW()",
            "sqlserver" => "GETUTCDATE()",
            _ => "datetime('now')"
        };
    }

    /// <summary>
    /// Returns the provider-specific idempotent INSERT statement.
    /// </summary>
    internal static string GetIdempotentInsert(
        string provider,
        string tableName,
        string columns,
        string paramNames,
        string keyPropertyName)
    {
        return provider switch
        {
            "postgresql" => $"INSERT INTO {tableName} ({columns}) VALUES ({paramNames}) ON CONFLICT DO NOTHING",
            "sqlserver" => $"IF NOT EXISTS (SELECT 1 FROM {tableName} WHERE {keyPropertyName} = @{keyPropertyName}) INSERT INTO {tableName} ({columns}) VALUES ({paramNames})",
            _ => $"INSERT OR IGNORE INTO {tableName} ({columns}) VALUES ({paramNames})"
        };
    }

    /// <summary>
    /// Returns the provider-specific connection factory details (using directive, class name, connection type).
    /// </summary>
    internal static (string UsingDirective, string ClassName, string ConnectionType) GetConnectionFactoryDetails(string provider)
    {
        return provider switch
        {
            "postgresql" => ("using Npgsql;", "NpgsqlConnectionFactory", "NpgsqlConnection"),
            "sqlserver" => ("using Microsoft.Data.SqlClient;", "SqlServerConnectionFactory", "SqlConnection"),
            _ => ("using Microsoft.Data.Sqlite;", "SqliteConnectionFactory", "SqliteConnection")
        };
    }

    /// <summary>
    /// Maps a CLR type name to the appropriate database column type for the given provider.
    /// Enum types are mapped to INTEGER.
    /// </summary>
    internal static string MapToDbType(string typeName, string provider, HashSet<string>? enumNames = null)
    {
        var normalized = NormalizeTypeName(typeName);

        return enumNames?.Contains(typeName) == true
            ? "INTEGER"
            : (provider, normalized) switch
            {
                // PostgreSQL types
                ("postgresql", "String") => "TEXT",
                ("postgresql", "Int32") => "INTEGER",
                ("postgresql", "Boolean") => "BOOLEAN",
                ("postgresql", "Decimal") => "NUMERIC",
                ("postgresql", "DateTime") => "TIMESTAMP",
                ("postgresql", "DateOnly") => "DATE",
                ("postgresql", "Guid") => "UUID",

                // SQL Server types
                ("sqlserver", "String") => "NVARCHAR(MAX)",
                ("sqlserver", "Int32") => "INT",
                ("sqlserver", "Boolean") => "BIT",
                ("sqlserver", "Decimal") => "DECIMAL(18,2)",
                ("sqlserver", "DateTime") => "DATETIME2",
                ("sqlserver", "DateOnly") => "DATE",
                ("sqlserver", "Guid") => "UNIQUEIDENTIFIER",

                // SQLite types (default)
                (_, "String") => "TEXT",
                (_, "Int32") => "INTEGER",
                (_, "Boolean") => "INTEGER",
                (_, "Decimal") => "REAL",
                (_, "DateTime") => "TEXT",
                (_, "DateOnly") => "TEXT",
                (_, "Guid") => "CHAR(36)",

                // Fallback per provider
                ("postgresql", _) => "TEXT",
                ("sqlserver", _) => "NVARCHAR(MAX)",
                _ => "TEXT"
            };
    }

    /// <summary>
    /// Normalizes C# type aliases to their CLR type names.
    /// </summary>
    private static string NormalizeTypeName(string typeName)
    {
        return typeName switch
        {
            "string" => "String",
            "int" => "Int32",
            "bool" => "Boolean",
            "decimal" => "Decimal",
            _ => typeName
        };
    }
}
