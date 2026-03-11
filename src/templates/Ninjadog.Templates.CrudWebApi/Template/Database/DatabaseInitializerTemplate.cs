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
                      {{GenerateCreateTableSqlQueries(entities)}}
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateCreateTableSqlQueries(List<NinjadogEntityWithKey> entities)
    {
        IndentedStringBuilder stringBuilder = new(2);

        foreach (var entity in entities)
        {
            stringBuilder
                .AppendLine()
                .AppendLine($"await connection.ExecuteAsync(@\"{GenerateSqlCreateTableQuery(entity)}\");");
        }

        return stringBuilder.ToString();
    }

    private static string GenerateSqlCreateTableQuery(NinjadogEntityWithKey entity)
    {
        var st = entity.StringTokens;
        var entityKey = entity.Properties.GetEntityKey();
        IndentedStringBuilder stringBuilder = new(0);

        stringBuilder
            .AppendLine($"CREATE TABLE IF NOT EXISTS {st.Models} (")
            .IncrementIndent().IncrementIndent().IncrementIndent()
            .AppendLine($"{entityKey.Key} {MapToSqliteType(entityKey.Type)} PRIMARY KEY,");

        var nonKeyProperties = entity.Properties
            .Where(p => !p.Value.IsKey)
            .ToList();

        for (var i = 0; i < nonKeyProperties.Count; i++)
        {
            var p = nonKeyProperties[i];
            if (i < nonKeyProperties.Count - 1)
            {
                stringBuilder.AppendLine($"{p.Key} {MapToSqliteType(p.Value.Type)} NOT NULL,");
            }
            else
            {
                stringBuilder.Append($"{p.Key} {MapToSqliteType(p.Value.Type)} NOT NULL)");
            }
        }

        return stringBuilder.ToString();
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
}
