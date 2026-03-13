namespace Ninjadog.Templates.CrudWebAPI.Template.Repositories;

/// <summary>
/// This template generates the repository for a given entity.
/// </summary>
public sealed class RepositoryTemplate
    : NinjadogTemplate
{
    private bool _softDelete;
    private bool _auditing;
    private bool _aot;
    private string _provider = "sqlite";

    /// <inheritdoc />
    public override string Name => "Repository";

    /// <inheritdoc />
    public override IEnumerable<NinjadogContentFile> GenerateMany(NinjadogSettings ninjadogSettings)
    {
        _softDelete = ninjadogSettings.Config.SoftDelete;
        _auditing = ninjadogSettings.Config.Auditing;
        _aot = ninjadogSettings.Config.Aot;
        _provider = ninjadogSettings.Config.DatabaseProvider;
        return base.GenerateMany(ninjadogSettings);
    }

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Repositories";
        var fileName = $"{st.ClassModelRepository}.cs";
        var entityKey = entity.Properties.GetEntityKey();

        var dapperAotAttribute = _aot ? "\n[DapperAot]" : string.Empty;

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Data;
              using {{rootNamespace}}.Database;
              using Dapper;

              {{WriteFileScopedNamespace(ns)}}
              {{dapperAotAttribute}}
              public partial class {{st.ClassModelRepository}}(IDbConnectionFactory connectionFactory)
                  : {{st.InterfaceModelRepository}}
              {
                  public async Task<bool> CreateAsync({{st.ClassModelDto}} {{st.VarModel}})
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();

                      var result = await connection.ExecuteAsync(
                          @"{{GenerateSqlInsertQuery(entity, _auditing, _provider)}}",
                          {{st.VarModel}});

                      return result > 0;
                  }

                  public async Task<{{st.ClassModelDto}}?> GetAsync({{entityKey.Type}} id)
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();

                      return await connection.QuerySingleOrDefaultAsync<{{st.ClassModelDto}}>(
                          "{{GenerateSqlSelectOneQuery(entity, _softDelete, _provider)}}",
                          new { {{entityKey.Key}} = id });
                  }

                  public async Task<IEnumerable<{{st.ClassModelDto}}>> GetAllAsync(
                      int page, int pageSize,
                      Dictionary<string, string>? filters = null,
                      string? sortBy = null,
                      bool sortDescending = false)
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();

                      var (whereClause, parameters) = BuildWhereClause(filters);
                      parameters.Add("PageSize", pageSize);
                      parameters.Add("Offset", (page - 1) * pageSize);

                      var orderBy = BuildOrderByClause(sortBy, sortDescending);
                      {{GenerateGetAllSqlBuilder(entity, _softDelete, _provider)}}

                      return await connection.QueryAsync<{{st.ClassModelDto}}>(sql, parameters);
                  }

                  public async Task<int> CountAsync(Dictionary<string, string>? filters = null)
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();

                      var (whereClause, parameters) = BuildWhereClause(filters);
                      {{GenerateCountSqlBuilder(entity, _softDelete)}}

                      return await connection.ExecuteScalarAsync<int>(sql, parameters);
                  }

                  public async Task<bool> UpdateAsync({{st.ClassModelDto}} {{st.VarModel}})
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();

                      var result = await connection.ExecuteAsync(
                          @"{{GenerateSqlUpdateQuery(entity, _auditing, _provider)}}",
                          {{st.VarModel}});

                          return result > 0;
                  }

                  public async Task<bool> DeleteAsync({{entityKey.Type}} id)
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();

                      var result = await connection.ExecuteAsync(
                          @"{{GenerateSqlDeleteQuery(entity, _softDelete, _provider)}}",
                          new { {{entityKey.Key}} = id });

                      return result > 0;
                  }

                  private static readonly HashSet<string> AllowedColumns = new(StringComparer.OrdinalIgnoreCase)
                  {
                      {{GenerateAllowedColumns(entity)}}
                  };

                  private static (string WhereClause, Dictionary<string, object> Parameters) BuildWhereClause(
                      Dictionary<string, string>? filters)
                  {
                      var parameters = new Dictionary<string, object>();
                      if (filters is null || filters.Count == 0)
                      {
                          return (string.Empty, parameters);
                      }

                      var conditions = new List<string>();
                      foreach (var (key, value) in filters)
                      {
                          if (AllowedColumns.Contains(key))
                          {
                              conditions.Add($"{key} = @Filter_{key}");
                              parameters[$"Filter_{key}"] = value;
                          }
                      }

                      var whereClause = conditions.Count > 0
                          ? string.Join(" AND ", conditions)
                          : string.Empty;

                      return (whereClause, parameters);
                  }

                  private static string BuildOrderByClause(string? sortBy, bool sortDescending)
                  {
                      if (string.IsNullOrEmpty(sortBy) || !AllowedColumns.Contains(sortBy))
                      {
                          return string.Empty;
                      }

                      var direction = sortDescending ? "DESC" : "ASC";
                      return $"{sortBy} {direction}";
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateAllowedColumns(NinjadogEntityWithKey entity)
    {
        var columns = entity.Properties
            .Where(p => !p.Value.IsKey)
            .Select(p => $"\"{p.Key}\"");
        return string.Join(", ", columns);
    }

    private static string GenerateSqlInsertQuery(NinjadogEntityWithKey entity, bool auditing, string provider)
    {
        var st = entity.StringTokens;
        var properties = entity.Properties;

        var columns = properties.Keys.ToList();
        var values = properties.Keys.Select(k => $"@{k}").ToList();

        if (auditing)
        {
            columns.Add("CreatedAt");
            columns.Add("UpdatedAt");
            var nowFunc = GetNowFunction(provider);
            values.Add(nowFunc);
            values.Add(nowFunc);
        }

        return new IndentedStringBuilder(0)
            .Append($"INSERT INTO {st.Models} (")
            .Append(string.Join(", ", columns))
            .Append(") ")
            .IncrementIndent(3)
            .Append("VALUES (")
            .Append(string.Join(", ", values))
            .Append(")")
            .ToString();
    }

    private static string GenerateSqlSelectOneQuery(NinjadogEntityWithKey entity, bool softDelete, string provider)
    {
        var st = entity.StringTokens;
        var entityKey = entity.Properties.GetEntityKey();
        var softDeleteFilter = softDelete ? " AND IsDeleted = 0" : string.Empty;
        var limit = provider == "sqlserver" ? string.Empty : " LIMIT 1";
        var top = provider == "sqlserver" ? "TOP 1 " : string.Empty;
        return $"SELECT {top}* FROM {st.Models} WHERE {entityKey.Key} = @{entityKey.Key}{softDeleteFilter}{limit}";
    }

    private static List<string> GenerateFilterableQueryLines(string baseQuery, bool softDelete)
    {
        var filterJoin = softDelete ? " AND " : " WHERE ";
        return
        [
            $"var sql = \"{baseQuery}\";",
            "if (!string.IsNullOrEmpty(whereClause))",
            "{",
            $"    sql += \"{filterJoin}\" + whereClause;",
            "}",
        ];
    }

    private static string GenerateGetAllSqlBuilder(NinjadogEntityWithKey entity, bool softDelete, string provider)
    {
        var st = entity.StringTokens;
        var baseQuery = softDelete
            ? $"SELECT * FROM {st.Models} WHERE IsDeleted = 0"
            : $"SELECT * FROM {st.Models}";
        var paginationClause = provider == "sqlserver"
            ? " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY"
            : " LIMIT @PageSize OFFSET @Offset";

        var lines = GenerateFilterableQueryLines(baseQuery, softDelete);
        lines.Add(string.Empty);

        lines.AddRange(provider == "sqlserver"
            ?
            [
                "sql += !string.IsNullOrEmpty(orderBy)",
                "    ? \" ORDER BY \" + orderBy",
                "    : \" ORDER BY (SELECT NULL)\";",
                string.Empty,
            ]
            :
            [
                "if (!string.IsNullOrEmpty(orderBy))",
                "{",
                "    sql += \" ORDER BY \" + orderBy;",
                "}",
                string.Empty,
            ]);

        lines.Add($"sql += \"{paginationClause}\";");

        return string.Join("\n        ", lines);
    }

    private static string GenerateCountSqlBuilder(NinjadogEntityWithKey entity, bool softDelete)
    {
        var st = entity.StringTokens;
        var baseQuery = softDelete
            ? $"SELECT COUNT(*) FROM {st.Models} WHERE IsDeleted = 0"
            : $"SELECT COUNT(*) FROM {st.Models}";

        var lines = GenerateFilterableQueryLines(baseQuery, softDelete);
        return string.Join("\n        ", lines);
    }

    private static string GenerateSqlUpdateQuery(NinjadogEntityWithKey entity, bool auditing, string provider)
    {
        var st = entity.StringTokens;
        var properties = entity.Properties;
        var entityKey = entity.Properties.GetEntityKey();

        IndentedStringBuilder stringBuilder = new(0);

        stringBuilder.Append($"UPDATE {st.Models} SET ");

        var updateClauses = properties
            .Where(p => !p.Value.IsKey)
            .Select(p => $"{p.Key} = @{p.Key}")
            .ToList();

        if (auditing)
        {
            updateClauses.Add($"UpdatedAt = {GetNowFunction(provider)}");
        }

        return stringBuilder
            .Append(string.Join(", ", updateClauses))
            .Append($" WHERE {entityKey.Key} = @{entityKey.Key}")
            .ToString();
    }

    private static string GenerateSqlDeleteQuery(NinjadogEntityWithKey entity, bool softDelete, string provider)
    {
        var st = entity.StringTokens;
        var entityKey = entity.Properties.GetEntityKey();
        return softDelete
            ? $"UPDATE {st.Models} SET IsDeleted = 1, DeletedAt = {GetNowFunction(provider)} WHERE {entityKey.Key} = @{entityKey.Key}"
            : $"DELETE FROM {st.Models} WHERE {entityKey.Key} = @{entityKey.Key}";
    }

    private static string GetNowFunction(string provider)
    {
        return provider switch
        {
            "postgresql" => "NOW()",
            "sqlserver" => "GETUTCDATE()",
            _ => "datetime('now')"
        };
    }
}
