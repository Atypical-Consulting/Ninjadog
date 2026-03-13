namespace Ninjadog.Templates.CrudWebAPI.Template.Database;

/// <summary>
/// This template generates the DatabaseInitializer class.
/// </summary>
public sealed class DatabaseInitializerTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DatabaseInitializer";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var entities = ninjadogSettings.Entities.FromKeys();
        var enumNames = ninjadogSettings.Enums?.Keys.ToHashSet();
        var softDelete = ninjadogSettings.Config.SoftDelete;
        var auditing = ninjadogSettings.Config.Auditing;
        var provider = ninjadogSettings.Config.DatabaseProvider;
        var ns = $"{rootNamespace}.Database";
        const string fileName = "DatabaseInitializer.cs";

        var content =
            $$"""

              using Dapper;

              {{WriteFileScopedNamespace(ns)}}

              public partial class DatabaseInitializer(IDbConnectionFactory connectionFactory)
              {
                  public async Task InitializeAsync()
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();
                      {{GenerateCreateTableSqlQueries(entities, enumNames, softDelete, auditing, provider)}}
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateCreateTableSqlQueries(List<NinjadogEntityWithKey> entities, HashSet<string>? enumNames, bool softDelete, bool auditing, string provider)
    {
        IndentedStringBuilder stringBuilder = new(2);

        foreach (var entity in entities)
        {
            stringBuilder
                .AppendLine()
                .AppendLine($"await connection.ExecuteAsync(@\"{GenerateSqlCreateTableQuery(entity, entities, enumNames, softDelete, auditing, provider)}\");");
        }

        return stringBuilder.ToString();
    }

    private static string GenerateSqlCreateTableQuery(NinjadogEntityWithKey entity, List<NinjadogEntityWithKey> allEntities, HashSet<string>? enumNames, bool softDelete, bool auditing, string provider)
    {
        var st = entity.StringTokens;
        var entityKey = entity.Properties.GetEntityKey();
        var fkConstraints = GetForeignKeyConstraints(entity, allEntities);
        IndentedStringBuilder stringBuilder = new(0);

        stringBuilder
            .AppendLine($"CREATE TABLE IF NOT EXISTS {st.Models} (")
            .IncrementIndent().IncrementIndent().IncrementIndent()
            .AppendLine($"{entityKey.Key} {MapToDbType(entityKey.Type, provider, enumNames)} PRIMARY KEY,");

        var nonKeyProperties = entity.Properties
            .Where(p => !p.Value.IsKey)
            .ToList();

        for (var i = 0; i < nonKeyProperties.Count; i++)
        {
            var p = nonKeyProperties[i];
            var isLast = i == nonKeyProperties.Count - 1;
            var needsComma = !isLast || softDelete || auditing || fkConstraints.Count > 0;
            var nullConstraint = p.Value.Required ? " NOT NULL" : string.Empty;

            if (needsComma)
            {
                stringBuilder.AppendLine($"{p.Key} {MapToDbType(p.Value.Type, provider, enumNames)}{nullConstraint},");
            }
            else
            {
                stringBuilder.Append($"{p.Key} {MapToDbType(p.Value.Type, provider, enumNames)}{nullConstraint})");
            }
        }

        if (softDelete)
        {
            var needsComma = auditing || fkConstraints.Count > 0;
            stringBuilder
                .AppendLine("IsDeleted INTEGER NOT NULL DEFAULT 0,")
                .Append(needsComma ? "DeletedAt TEXT," : "DeletedAt TEXT)");
            if (needsComma)
            {
                stringBuilder.AppendLine();
            }
        }

        if (auditing)
        {
            var needsComma = fkConstraints.Count > 0;
            stringBuilder
                .AppendLine("CreatedAt TEXT NOT NULL,");
            if (needsComma)
            {
                stringBuilder.AppendLine("UpdatedAt TEXT,");
            }
            else
            {
                stringBuilder.Append("UpdatedAt TEXT)");
            }
        }

        for (var i = 0; i < fkConstraints.Count; i++)
        {
            var (fkColumn, parentTable, parentPk) = fkConstraints[i];
            var isLast = i == fkConstraints.Count - 1;
            var constraint = $"FOREIGN KEY ({fkColumn}) REFERENCES {parentTable}({parentPk})";

            if (isLast)
            {
                stringBuilder.Append($"{constraint})");
            }
            else
            {
                stringBuilder.AppendLine($"{constraint},");
            }
        }

        return stringBuilder.ToString();
    }

    private static List<(string FkColumn, string ParentTable, string ParentPk)> GetForeignKeyConstraints(
        NinjadogEntityWithKey entity, List<NinjadogEntityWithKey> allEntities)
    {
        var constraints = new List<(string FkColumn, string ParentTable, string ParentPk)>();

        foreach (var potentialParent in allEntities)
        {
            if (potentialParent.Relationships == null)
            {
                continue;
            }

            foreach (var (_, relationship) in potentialParent.Relationships)
            {
                if (relationship.RelatedEntity != entity.Key)
                {
                    continue;
                }

                if (relationship.RelationshipType is not (NinjadogEntityRelationshipType.OneToMany or NinjadogEntityRelationshipType.OneToOne))
                {
                    continue;
                }

                var parentPk = potentialParent.Properties.GetEntityKey();
                var fkColumnName = parentPk.Key == "Id"
                    ? $"{potentialParent.Key}Id"
                    : parentPk.Key;

                if (entity.Properties.ContainsKey(fkColumnName))
                {
                    constraints.Add((fkColumnName, potentialParent.StringTokens.Models, parentPk.Key));
                }
            }
        }

        return constraints;
    }

    private static string MapToDbType(string typeName, string provider, HashSet<string>? enumNames = null)
    {
        return enumNames?.Contains(typeName) == true
            ? "INTEGER"
            : provider switch
            {
                "postgresql" => MapToPostgresType(typeName),
                "sqlserver" => MapToSqlServerType(typeName),
                _ => MapToSqliteType(typeName)
            };
    }

    private static string MapToSqliteType(string typeName)
    {
        return typeName switch
        {
            "String" => "TEXT",
            "Int32" => "INTEGER",
            "Boolean" => "INTEGER",
            "Decimal" => "REAL",
            "DateTime" => "TEXT",
            "DateOnly" => "TEXT",
            "Guid" => "CHAR(36)",
            _ => "TEXT"
        };
    }

    private static string MapToPostgresType(string typeName)
    {
        return typeName switch
        {
            "String" => "TEXT",
            "Int32" => "INTEGER",
            "Boolean" => "BOOLEAN",
            "Decimal" => "NUMERIC",
            "DateTime" => "TIMESTAMP",
            "DateOnly" => "DATE",
            "Guid" => "UUID",
            _ => "TEXT"
        };
    }

    private static string MapToSqlServerType(string typeName)
    {
        return typeName switch
        {
            "String" => "NVARCHAR(MAX)",
            "Int32" => "INT",
            "Boolean" => "BIT",
            "Decimal" => "DECIMAL(18,2)",
            "DateTime" => "DATETIME2",
            "DateOnly" => "DATE",
            "Guid" => "UNIQUEIDENTIFIER",
            _ => "NVARCHAR(MAX)"
        };
    }
}
