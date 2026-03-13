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
        var provider = ninjadogSettings.Config.DatabaseProvider;
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
              {{GenerateSeedInserts(entitiesWithSeed, provider)}}
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateSeedInserts(List<NinjadogEntityWithKey> entities, string provider)
    {
        IndentedStringBuilder sb = new(2);

        foreach (var entity in entities)
        {
            var st = entity.StringTokens;
            if (entity.SeedData == null)
            {
                continue;
            }

            var keyPropertyName = entity.Properties
                .FirstOrDefault(x => x.Value.IsKey).Key;

            foreach (var row in entity.SeedData)
            {
                var columns = string.Join(", ", row.Keys);
                var paramNames = string.Join(", ", row.Keys.Select(k => $"@{k}"));
                var sql = GenerateIdempotentInsert(provider, st.Models, columns, paramNames, keyPropertyName);
                var paramObject = GenerateAnonymousObject(row);

                sb.AppendLine()
                    .AppendLine($"await connection.ExecuteAsync(")
                    .IncrementIndent()
                    .AppendLine($"\"{sql}\",")
                    .AppendLine($"{paramObject});")
                    .DecrementIndent();
            }
        }

        return sb.ToString();
    }

    private static string GenerateIdempotentInsert(
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

    private static string GenerateAnonymousObject(Dictionary<string, object> row)
    {
        var properties = row.Select(kvp => $"{kvp.Key} = {FormatCSharpValue(kvp.Value)}");
        return $"new {{ {string.Join(", ", properties)} }}";
    }

    private static string FormatCSharpValue(object value)
    {
        return value switch
        {
            string s => $"\"{s.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"",
            bool b => b ? "true" : "false",
            _ => value.ToString()!
        };
    }
}
