namespace Ninjadog.Templates.CrudWebAPI.Template.Endpoints;

/// <summary>
/// This template generates the Update endpoint for a given entity.
/// </summary>
public sealed class UpdateEndpointTemplate
    : NinjadogTemplate
{
    private bool _hasAuth;
    private int? _apiVersion;

    /// <inheritdoc />
    public override string Name => "UpdateEndpoint";

    /// <inheritdoc />
    public override IEnumerable<NinjadogContentFile> GenerateMany(NinjadogSettings ninjadogSettings)
    {
        _hasAuth = ninjadogSettings.Config.Auth is not null;
        _apiVersion = ninjadogSettings.Config.Versioning?.DefaultVersion;
        return base.GenerateMany(ninjadogSettings);
    }

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Endpoints";
        var fileName = Path.Combine(st.Model, $"{st.ClassUpdateModelEndpoint}.cs");
        var entityKey = entity.Properties.GetEntityKey();

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Requests;
              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Mapping;
              using {{rootNamespace}}.Services;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassUpdateModelEndpoint}}({{st.InterfaceModelService}} {{st.VarModelService}})
                  : Endpoint<{{st.ClassUpdateModelRequest}}, {{st.ClassModelResponse}}>
              {
                  public override void Configure()
                  {
                      Put("{{st.ModelEndpoint}}/{id:{{GetRouteConstraint(entityKey.Type)}}}");{{(_hasAuth ? string.Empty : "\n        AllowAnonymous();")}}{{GenerateVersionCall(_apiVersion)}}
                  }

                  public override async Task HandleAsync({{st.ClassUpdateModelRequest}} req, CancellationToken ct)
                  {
                      var {{st.VarExistingModel}} = await {{st.VarModelService}}.GetAsync(req.{{entityKey.Key}});

                      if ({{st.VarExistingModel}} is null)
                      {
                          await Send.NotFoundAsync(ct);
                          return;
                      }

                      var {{st.VarModel}} = req.{{st.MethodToModel}}();
                      await {{st.VarModelService}}.UpdateAsync({{st.VarModel}});

                      var {{st.VarModelResponse}} = {{st.VarModel}}.{{st.MethodToModelResponse}}();
                      await Send.OkAsync({{st.VarModelResponse}}, ct);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
