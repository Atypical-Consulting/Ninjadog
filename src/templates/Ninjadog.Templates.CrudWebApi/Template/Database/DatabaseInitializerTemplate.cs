// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

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
                      {{GenerateCreateTableSqlQueries(entities, enumNames, softDelete, auditing)}}
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateCreateTableSqlQueries(List<NinjadogEntityWithKey> entities, HashSet<string>? enumNames, bool softDelete, bool auditing)
    {
        IndentedStringBuilder stringBuilder = new(2);

        foreach (var entity in entities)
        {
            stringBuilder
                .AppendLine()
                .AppendLine($"await connection.ExecuteAsync(@\"{GenerateSqlCreateTableQuery(entity, enumNames, softDelete, auditing)}\");");
        }

        return stringBuilder.ToString();
    }

    private static string GenerateSqlCreateTableQuery(NinjadogEntityWithKey entity, HashSet<string>? enumNames, bool softDelete, bool auditing)
    {
        var st = entity.StringTokens;
        var entityKey = entity.Properties.GetEntityKey();
        IndentedStringBuilder stringBuilder = new(0);

        stringBuilder
            .AppendLine($"CREATE TABLE IF NOT EXISTS {st.Models} (")
            .IncrementIndent().IncrementIndent().IncrementIndent()
            .AppendLine($"{entityKey.Key} {MapToSqliteType(entityKey.Type, enumNames)} PRIMARY KEY,");

        var nonKeyProperties = entity.Properties
            .Where(p => !p.Value.IsKey)
            .ToList();

        for (var i = 0; i < nonKeyProperties.Count; i++)
        {
            var p = nonKeyProperties[i];
            var isLast = i == nonKeyProperties.Count - 1;
            var needsComma = !isLast || softDelete || auditing;

            if (needsComma)
            {
                stringBuilder.AppendLine($"{p.Key} {MapToSqliteType(p.Value.Type, enumNames)} NOT NULL,");
            }
            else
            {
                stringBuilder.Append($"{p.Key} {MapToSqliteType(p.Value.Type, enumNames)} NOT NULL)");
            }
        }

        if (softDelete)
        {
            var needsComma = auditing;
            stringBuilder
                .AppendLine("IsDeleted INTEGER NOT NULL DEFAULT 0,")
                .Append(needsComma ? "DeletedAt TEXT," : "DeletedAt TEXT)");
            if (needsComma) stringBuilder.AppendLine();
        }

        if (auditing)
        {
            stringBuilder
                .AppendLine("CreatedAt TEXT NOT NULL,")
                .Append("UpdatedAt TEXT)");
        }

        return stringBuilder.ToString();
    }

    private static string MapToSqliteType(string typeName, HashSet<string>? enumNames = null)
    {
        if (enumNames?.Contains(typeName) == true)
        {
            return "INTEGER";
        }

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
}
