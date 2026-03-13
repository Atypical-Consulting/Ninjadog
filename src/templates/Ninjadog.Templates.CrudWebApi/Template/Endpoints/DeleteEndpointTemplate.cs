namespace Ninjadog.Templates.CrudWebAPI.Template.Endpoints;

/// <summary>
/// This template generates the Delete endpoint for a given entity.
/// </summary>
public sealed class DeleteEndpointTemplate
    : NinjadogTemplate
{
    private bool _hasAuth;
    private int? _apiVersion;

    /// <inheritdoc />
    public override string Name => "DeleteEndpoint";

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
        var fileName = Path.Combine(st.Model, $"{st.ClassDeleteModelEndpoint}.cs");
        var entityKey = entity.Properties.GetEntityKey();

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Requests;
              using {{rootNamespace}}.Services;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassDeleteModelEndpoint}}({{st.InterfaceModelService}} {{st.VarModelService}})
                  : Endpoint<{{st.ClassDeleteModelRequest}}>
              {
                  public override void Configure()
                  {
                      Delete("{{st.ModelEndpoint}}/{id:{{GetRouteConstraint(entityKey.Type)}}}");{{(_hasAuth ? string.Empty : "\n        AllowAnonymous();")}}{{GenerateVersionCall(_apiVersion)}}
                  }

                  public override async Task HandleAsync({{st.ClassDeleteModelRequest}} req, CancellationToken ct)
                  {
                      var deleted = await {{st.VarModelService}}.DeleteAsync(req.{{entityKey.PascalKey}});
                      if (!deleted)
                      {
                          await Send.NotFoundAsync(ct);
                          return;
                      }

                      await Send.NoContentAsync(ct);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
