namespace Ninjadog.Templates.CrudWebAPI.Template.Endpoints;

/// <summary>
/// This template generates the GetAll endpoint for a given entity.
/// </summary>
public sealed class GetAllEndpointTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "GetAllEndpoint";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Endpoints";
        var fileName = Path.Combine(st.Model, $"{st.ClassGetAllModelsEndpoint}.cs");

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
                  public override void Configure()
                  {
                      Get("{{st.ModelEndpoint}}");
                      AllowAnonymous();
                  }

                  public override async Task HandleAsync(CancellationToken ct)
                  {
                      var page = int.TryParse(HttpContext.Request.Query["page"], out var p) && p > 0 ? p : 1;
                      var pageSize = int.TryParse(HttpContext.Request.Query["pageSize"], out var ps) && ps > 0 ? ps : 10;

                      var ({{st.VarModels}}, totalCount) = await {{st.VarModelService}}.GetAllAsync(page, pageSize);
                      var {{st.VarModelsResponse}} = {{st.VarModels}}.{{st.MethodToModelsResponse}}(page, pageSize, totalCount);
                      await SendOkAsync({{st.VarModelsResponse}}, ct);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
