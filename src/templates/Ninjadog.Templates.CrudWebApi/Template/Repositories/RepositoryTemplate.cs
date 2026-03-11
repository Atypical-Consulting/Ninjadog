// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Templates.CrudWebAPI.Template.Repositories;

/// <summary>
/// This template generates the repository for a given entity.
/// </summary>
public sealed class RepositoryTemplate
    : NinjadogTemplate
{
    private bool _softDelete;
    private bool _auditing;

    /// <inheritdoc />
    public override string Name => "Repository";

    /// <inheritdoc />
    public override IEnumerable<NinjadogContentFile> GenerateMany(NinjadogSettings ninjadogSettings)
    {
        _softDelete = ninjadogSettings.Config.SoftDelete;
        _auditing = ninjadogSettings.Config.Auditing;
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

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Data;
              using {{rootNamespace}}.Database;
              using Dapper;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassModelRepository}}(IDbConnectionFactory connectionFactory)
                  : {{st.InterfaceModelRepository}}
              {
                  public async Task<bool> CreateAsync({{st.ClassModelDto}} {{st.VarModel}})
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();

                      var result = await connection.ExecuteAsync(
                          @"{{GenerateSqlInsertQuery(entity, _auditing)}}",
                          {{st.VarModel}});

                      return result > 0;
                  }

                  public async Task<{{st.ClassModelDto}}?> GetAsync({{entityKey.Type}} id)
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();

                      return await connection.QuerySingleOrDefaultAsync<{{st.ClassModelDto}}>(
                          "{{GenerateSqlSelectOneQuery(entity, _softDelete)}}",
                          new { {{entityKey.Key}} = id.ToString() });
                  }

                  public async Task<IEnumerable<{{st.ClassModelDto}}>> GetAllAsync(int page, int pageSize)
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();
                      return await connection.QueryAsync<{{st.ClassModelDto}}>(
                          "{{GenerateSqlSelectAllQuery(entity, _softDelete)}}",
                          new { PageSize = pageSize, Offset = (page - 1) * pageSize });
                  }

                  public async Task<int> CountAsync()
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();
                      return await connection.ExecuteScalarAsync<int>("{{GenerateSqlCountQuery(entity, _softDelete)}}");
                  }

                  public async Task<bool> UpdateAsync({{st.ClassModelDto}} {{st.VarModel}})
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();

                      var result = await connection.ExecuteAsync(
                          @"{{GenerateSqlUpdateQuery(entity, _auditing)}}",
                          {{st.VarModel}});

                          return result > 0;
                  }

                  public async Task<bool> DeleteAsync({{entityKey.Type}} id)
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();

                      var result = await connection.ExecuteAsync(
                          @"{{GenerateSqlDeleteQuery(entity, _softDelete)}}",
                          new { {{entityKey.Key}} = id.ToString() });

                      return result > 0;
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateSqlInsertQuery(NinjadogEntityWithKey entity, bool auditing)
    {
        var st = entity.StringTokens;
        var properties = entity.Properties;

        var columns = properties.Keys.ToList();
        var values = properties.Keys.Select(k => $"@{k}").ToList();

        if (auditing)
        {
            columns.Add("CreatedAt");
            columns.Add("UpdatedAt");
            values.Add("datetime('now')");
            values.Add("datetime('now')");
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

    private static string GenerateSqlSelectOneQuery(NinjadogEntityWithKey entity, bool softDelete)
    {
        var st = entity.StringTokens;
        var entityKey = entity.Properties.GetEntityKey();
        var softDeleteFilter = softDelete ? " AND IsDeleted = 0" : string.Empty;
        return $"SELECT * FROM {st.Models} WHERE {entityKey.Key} = @{entityKey.Key}{softDeleteFilter} LIMIT 1";
    }

    private static string GenerateSqlSelectAllQuery(NinjadogEntityWithKey entity, bool softDelete)
    {
        var st = entity.StringTokens;
        var whereClause = softDelete ? " WHERE IsDeleted = 0" : string.Empty;
        return $"SELECT * FROM {st.Models}{whereClause} LIMIT @PageSize OFFSET @Offset";
    }

    private static string GenerateSqlCountQuery(NinjadogEntityWithKey entity, bool softDelete)
    {
        var st = entity.StringTokens;
        return softDelete
            ? $"SELECT COUNT(*) FROM {st.Models} WHERE IsDeleted = 0"
            : $"SELECT COUNT(*) FROM {st.Models}";
    }

    private static string GenerateSqlUpdateQuery(NinjadogEntityWithKey entity, bool auditing)
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
            updateClauses.Add("UpdatedAt = datetime('now')");
        }

        return stringBuilder
            .Append(string.Join(", ", updateClauses))
            .Append($" WHERE {entityKey.Key} = @{entityKey.Key}")
            .ToString();
    }

    private static string GenerateSqlDeleteQuery(NinjadogEntityWithKey entity, bool softDelete)
    {
        var st = entity.StringTokens;
        var entityKey = entity.Properties.GetEntityKey();
        return softDelete
            ? $"UPDATE {st.Models} SET IsDeleted = 1, DeletedAt = datetime('now') WHERE {entityKey.Key} = @{entityKey.Key}"
            : $"DELETE FROM {st.Models} WHERE {entityKey.Key} = @{entityKey.Key}";
    }
}
