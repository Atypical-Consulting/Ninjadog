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
                var values = string.Join(", ", row.Values.Select(FormatSqlValue));
                var sql = GenerateIdempotentInsert(provider, st.Models, columns, values, keyPropertyName, row);

                sb.AppendLine()
                    .AppendLine($"await connection.ExecuteAsync(\"{sql}\");");
            }
        }

        return sb.ToString();
    }

    private static string GenerateIdempotentInsert(
        string provider,
        string tableName,
        string columns,
        string values,
        string keyPropertyName,
        Dictionary<string, object> row)
    {
        switch (provider)
        {
            case "postgresql":
                return $"INSERT INTO {tableName} ({columns}) VALUES ({values}) ON CONFLICT DO NOTHING";

            case "sqlserver":
                var keyValue = row.TryGetValue(keyPropertyName, out var kv) ? FormatSqlValue(kv) : "NULL";
                return $"IF NOT EXISTS (SELECT 1 FROM {tableName} WHERE {keyPropertyName} = {keyValue}) INSERT INTO {tableName} ({columns}) VALUES ({values})";

            default:
                return $"INSERT OR IGNORE INTO {tableName} ({columns}) VALUES ({values})";
        }
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
