namespace Ninjadog.Templates.CrudWebAPI.Template.Endpoints;

/// <summary>
/// This template generates the GetAll endpoint for a given entity.
/// </summary>
public sealed class GetAllEndpointTemplate
    : NinjadogTemplate
{
    private int? _apiVersion;

    /// <inheritdoc />
    public override string Name => "GetAllEndpoint";

    /// <inheritdoc />
    public override IEnumerable<NinjadogContentFile> GenerateMany(NinjadogSettings ninjadogSettings)
    {
        _apiVersion = ninjadogSettings.Config.Versioning?.DefaultVersion;
        return base.GenerateMany(ninjadogSettings);
    }

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Endpoints";
        var fileName = Path.Combine(st.Model, $"{st.ClassGetAllModelsEndpoint}.cs");

        var filterableProperties = entity.Properties
            .Where(p => !p.Value.IsKey)
            .Select(p => p.Key)
            .ToList();

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Mapping;
              using {{rootNamespace}}.Services;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassGetAllModelsEndpoint}}({{st.InterfaceModelService}} {{st.VarModelService}})
                  : EndpointWithoutRequest<{{st.ClassGetAllModelsResponse}}>
              {
                  private static readonly HashSet<string> FilterableProperties = new(StringComparer.OrdinalIgnoreCase)
                  {
                      {{string.Join(", ", filterableProperties.Select(p => $"\"{p}\""))}}
                  };

                  public override void Configure()
                  {
                      Get("{{st.ModelEndpoint}}");
                      AllowAnonymous();{{GenerateVersionCall(_apiVersion)}}
                  }

                  public override async Task HandleAsync(CancellationToken ct)
                  {
                      var page = int.TryParse(HttpContext.Request.Query["page"], out var p) && p > 0 ? p : 1;
                      var pageSize = int.TryParse(HttpContext.Request.Query["pageSize"], out var ps) && ps > 0 ? ps : 10;

                      // Parse filters from query parameters matching entity properties
                      var filters = new Dictionary<string, string>();
                      foreach (var key in HttpContext.Request.Query.Keys)
                      {
                          if (FilterableProperties.Contains(key))
                          {
                              var value = HttpContext.Request.Query[key].ToString();
                              if (!string.IsNullOrEmpty(value))
                              {
                                  filters[key] = value;
                              }
                          }
                      }

                      // Parse sort parameters
                      var sortBy = HttpContext.Request.Query["sortBy"].ToString();
                      if (!string.IsNullOrEmpty(sortBy) && !FilterableProperties.Contains(sortBy))
                      {
                          sortBy = null;
                      }

                      var sortDescending = string.Equals(
                          HttpContext.Request.Query["sortDir"].ToString(), "desc", StringComparison.OrdinalIgnoreCase);

                      var ({{st.VarModels}}, totalCount) = await {{st.VarModelService}}.GetAllAsync(
                          page, pageSize, filters, sortBy, sortDescending);
                      var {{st.VarModelsResponse}} = {{st.VarModels}}.{{st.MethodToModelsResponse}}(page, pageSize, totalCount);
                      await Send.OkAsync({{st.VarModelsResponse}}, ct);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
