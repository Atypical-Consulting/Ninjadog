namespace Ninjadog.Templates.CrudWebAPI.Template.Endpoints;

/// <summary>
/// This template generates the Get endpoint for a given entity.
/// </summary>
public sealed class GetEndpointTemplate
    : NinjadogTemplate
{
    private int? _apiVersion;

    /// <inheritdoc />
    public override string Name => "GetEndpoint";

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
        var fileName = Path.Combine(st.Model, $"{st.ClassGetModelEndpoint}.cs");
        var entityKey = entity.Properties.GetEntityKey();

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Requests;
              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Mapping;
              using {{rootNamespace}}.Services;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassGetModelEndpoint}}({{st.InterfaceModelService}} {{st.VarModelService}})
                  : Endpoint<{{st.ClassGetModelRequest}}, {{st.ClassModelResponse}}>
              {
                  public override void Configure()
                  {
                      Get("{{st.ModelEndpoint}}/{id:{{GetRouteConstraint(entityKey.Type)}}}");
                      AllowAnonymous();{{GenerateVersionCall(_apiVersion)}}
                  }

                  public override async Task HandleAsync({{st.ClassGetModelRequest}} req, CancellationToken ct)
                  {
                      var {{st.VarModel}} = await {{st.VarModelService}}.GetAsync(req.{{entityKey.Key}});

                      if ({{st.VarModel}} is null)
                      {
                          await Send.NotFoundAsync(ct);
                          return;
                      }

                      var {{st.VarModelResponse}} = {{st.VarModel}}.{{st.MethodToModelResponse}}();
                      await Send.OkAsync({{st.VarModelResponse}}, ct);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
