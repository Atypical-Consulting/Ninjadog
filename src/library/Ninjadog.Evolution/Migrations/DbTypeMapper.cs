namespace Ninjadog.Evolution.Migrations;

/// <summary>
/// Maps CLR type names to database column types for each supported provider.
/// This mirrors the type mapping in <c>DatabaseProviderHelper</c> to ensure migration
/// SQL uses the same types as initial table creation.
/// </summary>
public static class DbTypeMapper
{
    /// <summary>
    /// Maps a CLR type name to the appropriate database column type for the given provider.
    /// </summary>
    /// <param name="typeName">The CLR type name (e.g. "String", "Int32", "Guid").</param>
    /// <param name="provider">The database provider ("sqlite", "postgresql", "sqlserver").</param>
    /// <param name="enumNames">Optional set of enum type names (mapped to INTEGER).</param>
    /// <returns>The provider-specific SQL column type.</returns>
    public static string MapToDbType(string typeName, string provider, HashSet<string>? enumNames = null)
    {
        var normalized = NormalizeTypeName(typeName);

        return enumNames?.Contains(typeName) == true
            ? "INTEGER"
            : (provider, normalized) switch
            {
                ("postgresql", "String") => "TEXT",
                ("postgresql", "Int32") => "INTEGER",
                ("postgresql", "Boolean") => "BOOLEAN",
                ("postgresql", "Decimal") => "NUMERIC",
                ("postgresql", "DateTime") => "TIMESTAMP",
                ("postgresql", "DateOnly") => "DATE",
                ("postgresql", "Guid") => "UUID",

                ("sqlserver", "String") => "NVARCHAR(MAX)",
                ("sqlserver", "Int32") => "INT",
                ("sqlserver", "Boolean") => "BIT",
                ("sqlserver", "Decimal") => "DECIMAL(18,2)",
                ("sqlserver", "DateTime") => "DATETIME2",
                ("sqlserver", "DateOnly") => "DATE",
                ("sqlserver", "Guid") => "UNIQUEIDENTIFIER",

                (_, "String") => "TEXT",
                (_, "Int32") => "INTEGER",
                (_, "Boolean") => "INTEGER",
                (_, "Decimal") => "REAL",
                (_, "DateTime") => "TEXT",
                (_, "DateOnly") => "TEXT",
                (_, "Guid") => "CHAR(36)",

                ("postgresql", _) => "TEXT",
                ("sqlserver", _) => "NVARCHAR(MAX)",
                _ => "TEXT"
            };
    }

    /// <summary>
    /// Returns the SQL function for getting the current timestamp for the given provider.
    /// </summary>
    /// <param name="provider">The database provider.</param>
    /// <returns>The provider-specific NOW function.</returns>
    public static string GetNowFunction(string provider)
    {
        return provider switch
        {
            "postgresql" => "NOW()",
            "sqlserver" => "GETUTCDATE()",
            _ => "datetime('now')"
        };
    }

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
