// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Templates.CrudWebAPI.Template.Database;

/// <summary>
/// This template generates the DatabaseSeeder class for seeding initial data.
/// </summary>
public sealed class DatabaseSeederTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DatabaseSeeder";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var entities = ninjadogSettings.Entities.FromKeys();
        var ns = $"{rootNamespace}.Database";
        const string fileName = "DatabaseSeeder.cs";

        var entitiesWithSeed = entities.Where(e => e.SeedData is { Count: > 0 }).ToList();
        if (entitiesWithSeed.Count == 0)
        {
            return NinjadogContentFile.Empty;
        }

        var content =
            $$"""

              using Dapper;

              {{WriteFileScopedNamespace(ns)}}

              public partial class DatabaseSeeder(IDbConnectionFactory connectionFactory)
              {
                  public async Task SeedAsync()
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();
              {{GenerateSeedInserts(entitiesWithSeed)}}
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateSeedInserts(List<NinjadogEntityWithKey> entities)
    {
        IndentedStringBuilder sb = new(2);

        foreach (var entity in entities)
        {
            var st = entity.StringTokens;
            if (entity.SeedData == null)
            {
                continue;
            }

            foreach (var row in entity.SeedData)
            {
                var columns = string.Join(", ", row.Keys);
                var values = string.Join(", ", row.Values.Select(FormatSqlValue));

                sb.AppendLine()
                    .AppendLine($"await connection.ExecuteAsync(\"INSERT INTO {st.Models} ({columns}) VALUES ({values})\");");
            }
        }

        return sb.ToString();
    }

    private static string FormatSqlValue(object value)
    {
        return value switch
        {
            string s => $"'{s.Replace("'", "''")}'",
            bool b => b ? "1" : "0",
            _ => value.ToString()!
        };
    }
}
