using Ninjadog.Helpers;
using Ninjadog.Settings;

namespace Ninjadog.Evolution.Migrations;

/// <summary>
/// Generates provider-aware SQL migration scripts from a <see cref="SchemaDiff"/>.
/// Each migration is a standalone SQL file that can be executed against the target database.
/// </summary>
public static class MigrationSqlGenerator
{
    /// <summary>
    /// Generates SQL migration statements from a schema diff.
    /// </summary>
    /// <param name="diff">The schema diff to generate migrations from.</param>
    /// <param name="settings">The current settings (used for provider, enum names, table naming).</param>
    /// <returns>A list of migration operations, each with a description and SQL statement.</returns>
    public static IReadOnlyList<MigrationOperation> Generate(SchemaDiff diff, NinjadogSettings settings)
    {
        var provider = settings.Config.DatabaseProvider;
        var enumNames = settings.Enums?.Keys.ToHashSet();
        var operations = new List<MigrationOperation>();

        GenerateEntityOperations(diff, provider, enumNames, operations);
        GenerateConfigOperations(diff, operations);

        return operations;
    }

    private static void GenerateEntityOperations(
        SchemaDiff diff,
        string provider,
        HashSet<string>? enumNames,
        List<MigrationOperation> operations)
    {
        foreach (var entity in diff.EntityChanges)
        {
            switch (entity.Kind)
            {
                case ChangeKind.Added:
                    operations.Add(GenerateCreateTable(entity, provider, enumNames));
                    break;

                case ChangeKind.Removed:
                    operations.Add(GenerateDropTable(entity));
                    break;

                case ChangeKind.Modified:
                    GenerateAlterTableOperations(entity, provider, enumNames, operations);
                    break;
            }
        }
    }

    private static MigrationOperation GenerateCreateTable(
        EntityDiff entity,
        string provider,
        HashSet<string>? enumNames)
    {
        var tableName = new StringTokens(entity.EntityKey).Models;
        var sb = new IndentedStringBuilder(0);

        sb.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (")
            .IncrementIndent();

        var properties = entity.PropertyChanges.ToList();
        for (var i = 0; i < properties.Count; i++)
        {
            var prop = properties[i];
            var propDef = prop.After!;
            var dbType = DbTypeMapper.MapToDbType(propDef.Type, provider, enumNames);
            var isLast = i == properties.Count - 1;

            if (propDef.IsKey)
            {
                sb.AppendLine($"{prop.PropertyName} {dbType} PRIMARY KEY{(isLast ? string.Empty : ",")}");
            }
            else
            {
                var nullConstraint = propDef.Required ? " NOT NULL" : string.Empty;
                sb.AppendLine($"{prop.PropertyName} {dbType}{nullConstraint}{(isLast ? string.Empty : ",")}");
            }
        }

        sb.DecrementIndent()
            .Append(");");

        return new MigrationOperation(
            $"Create table {tableName}",
            sb.ToString());
    }

    private static MigrationOperation GenerateDropTable(EntityDiff entity)
    {
        var tableName = new StringTokens(entity.EntityKey).Models;
        return new MigrationOperation(
            $"Drop table {tableName}",
            $"DROP TABLE IF EXISTS {tableName};");
    }

    private static void GenerateAlterTableOperations(
        EntityDiff entity,
        string provider,
        HashSet<string>? enumNames,
        List<MigrationOperation> operations)
    {
        var tableName = new StringTokens(entity.EntityKey).Models;

        foreach (var prop in entity.AddedProperties)
        {
            operations.Add(GenerateAddColumn(tableName, prop, provider, enumNames));
        }

        foreach (var prop in entity.RemovedProperties)
        {
            operations.Add(GenerateDropColumn(tableName, prop, provider));
        }

        foreach (var prop in entity.ModifiedProperties)
        {
            if (prop.TypeChanged)
            {
                operations.Add(GenerateAlterColumnType(tableName, prop, provider, enumNames));
            }
        }
    }

    private static MigrationOperation GenerateAddColumn(
        string tableName,
        PropertyDiff prop,
        string provider,
        HashSet<string>? enumNames)
    {
        var dbType = DbTypeMapper.MapToDbType(prop.After!.Type, provider, enumNames);
        var nullConstraint = prop.After.Required ? " NOT NULL" : string.Empty;

        var defaultClause = string.Empty;
        if (prop.After.Required)
        {
            defaultClause = GetDefaultValueClause(prop.After.Type, provider);
        }

        var sql = $"ALTER TABLE {tableName} ADD COLUMN {prop.PropertyName} {dbType}{nullConstraint}{defaultClause};";
        return new MigrationOperation(
            $"Add column {prop.PropertyName} to {tableName}",
            sql);
    }

    private static MigrationOperation GenerateDropColumn(
        string tableName,
        PropertyDiff prop,
        string provider)
    {
        if (provider == "sqlite")
        {
            var sql = $"-- SQLite does not support DROP COLUMN before version 3.35.0.\n-- ALTER TABLE {tableName} DROP COLUMN {prop.PropertyName};";
            return new MigrationOperation(
                $"Drop column {prop.PropertyName} from {tableName}",
                sql,
                IsWarning: true);
        }

        return new MigrationOperation(
            $"Drop column {prop.PropertyName} from {tableName}",
            $"ALTER TABLE {tableName} DROP COLUMN {prop.PropertyName};");
    }

    private static MigrationOperation GenerateAlterColumnType(
        string tableName,
        PropertyDiff prop,
        string provider,
        HashSet<string>? enumNames)
    {
        var newDbType = DbTypeMapper.MapToDbType(prop.After!.Type, provider, enumNames);

        var sql = provider switch
        {
            "postgresql" =>
                $"ALTER TABLE {tableName} ALTER COLUMN {prop.PropertyName} TYPE {newDbType};",
            "sqlserver" =>
                $"ALTER TABLE {tableName} ALTER COLUMN {prop.PropertyName} {newDbType};",
            _ =>
                $"-- SQLite does not support ALTER COLUMN. Table recreation required.\n-- Column: {prop.PropertyName} ({prop.Before!.Type} -> {prop.After.Type})"
        };

        var description = $"Change type of {prop.PropertyName} in {tableName} ({prop.Before!.Type} -> {prop.After.Type})";
        return new MigrationOperation(description, sql, IsWarning: provider == "sqlite");
    }

    private static void GenerateConfigOperations(
        SchemaDiff diff,
        List<MigrationOperation> operations)
    {
        if (diff.ConfigChanges.SoftDeleteChanged)
        {
            var enabled = diff.ConfigChanges.SoftDeleteEnabled == true;
            var description = enabled
                ? "Enable soft delete: add IsDeleted and DeletedAt columns to all tables"
                : "Disable soft delete: remove IsDeleted and DeletedAt columns from all tables";
            var sql = enabled
                ? BuildConfigCommentSql("Add soft delete columns", "ADD COLUMN IsDeleted INTEGER NOT NULL DEFAULT 0", "ADD COLUMN DeletedAt TEXT")
                : BuildConfigCommentSql("Remove soft delete columns", "DROP COLUMN IsDeleted", "DROP COLUMN DeletedAt");
            operations.Add(new MigrationOperation(description, sql, IsWarning: true));
        }

        if (diff.ConfigChanges.AuditingChanged)
        {
            var enabled = diff.ConfigChanges.AuditingEnabled == true;
            var description = enabled
                ? "Enable auditing: add CreatedAt and UpdatedAt columns to all tables"
                : "Disable auditing: remove CreatedAt and UpdatedAt columns from all tables";
            var sql = enabled
                ? BuildConfigCommentSql("Add audit columns", "ADD COLUMN CreatedAt TEXT NOT NULL DEFAULT ''", "ADD COLUMN UpdatedAt TEXT")
                : BuildConfigCommentSql("Remove audit columns", "DROP COLUMN CreatedAt", "DROP COLUMN UpdatedAt");
            operations.Add(new MigrationOperation(description, sql, IsWarning: true));
        }

        if (diff.ConfigChanges.DatabaseProviderChanged)
        {
            var description = $"Database provider changed ({diff.ConfigChanges.OldDatabaseProvider} -> {diff.ConfigChanges.NewDatabaseProvider})";
            var sql = $"-- Database provider changed from {diff.ConfigChanges.OldDatabaseProvider} to {diff.ConfigChanges.NewDatabaseProvider}.\n"
                + "-- This requires a full schema translation. Consider re-running 'ninjadog build' to regenerate\n"
                + "-- the project with the new provider, then migrate data manually.";
            operations.Add(new MigrationOperation(description, sql, IsWarning: true));
        }
    }

    private static string BuildConfigCommentSql(string action, string column1, string column2)
    {
        return $"-- {action} to/from each entity table.\n"
            + "-- Replace {{TABLE}} with each entity's table name.\n"
            + $"-- ALTER TABLE {{{{TABLE}}}} {column1};\n"
            + $"-- ALTER TABLE {{{{TABLE}}}} {column2};";
    }

    private static string GetDefaultValueClause(string typeName, string provider)
    {
        var normalized = typeName switch
        {
            "string" => "String",
            "int" => "Int32",
            "bool" => "Boolean",
            "decimal" => "Decimal",
            _ => typeName
        };

        return normalized switch
        {
            "String" => " DEFAULT ''",
            "Int32" => " DEFAULT 0",
            "Boolean" => provider == "postgresql" ? " DEFAULT FALSE" : " DEFAULT 0",
            "Decimal" => " DEFAULT 0",
            "DateTime" => $" DEFAULT {DbTypeMapper.GetNowFunction(provider)}",
            "Guid" => string.Empty,
            _ => " DEFAULT ''"
        };
    }
}
