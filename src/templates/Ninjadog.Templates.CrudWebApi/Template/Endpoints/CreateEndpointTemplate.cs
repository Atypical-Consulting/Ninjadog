namespace Ninjadog.Templates.CrudWebAPI.Template.Endpoints;

/// <summary>
/// This template generates the Create endpoint for a given entity.
/// </summary>
public sealed class CreateEndpointTemplate
    : NinjadogTemplate
{
    private bool _hasAuth;
    private int? _apiVersion;

    /// <inheritdoc />
    public override string Name => "CreateEndpoint";

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
        var fileName = Path.Combine(st.Model, $"{st.ClassCreateModelEndpoint}.cs");
        var entityKey = entity.Properties.GetEntityKey();

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Requests;
              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Mapping;
              using {{rootNamespace}}.Services;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassCreateModelEndpoint}}({{st.InterfaceModelService}} {{st.VarModelService}})
                  : Endpoint<{{st.ClassCreateModelRequest}}, {{st.ClassModelResponse}}>
              {
                  public override void Configure()
                  {
                      Post("{{st.ModelEndpoint}}");{{(_hasAuth ? string.Empty : "\n        AllowAnonymous();")}}{{GenerateVersionCall(_apiVersion)}}
                  }

                  public override async Task HandleAsync({{st.ClassCreateModelRequest}} req, CancellationToken ct)
                  {
                      var {{st.VarModel}} = req.{{st.MethodToModel}}();

                      await {{st.VarModelService}}.CreateAsync({{st.VarModel}});

                      var {{st.VarModelResponse}} = {{st.VarModel}}.{{st.MethodToModelResponse}}();
                      await Send.CreatedAtAsync<{{st.ClassGetModelEndpoint}}>(
                          new { Id = {{st.VarModel}}.{{entityKey.PascalKey}} }, {{st.VarModelResponse}}, generateAbsoluteUrl: true, cancellation: ct);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
