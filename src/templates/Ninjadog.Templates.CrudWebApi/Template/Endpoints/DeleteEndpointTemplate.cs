namespace Ninjadog.Templates.CrudWebAPI.Template.Endpoints;

/// <summary>
/// This template generates the Delete endpoint for a given entity.
/// </summary>
public sealed class DeleteEndpointTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DeleteEndpoint";

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
                      Delete("{{st.ModelEndpoint}}/{id:{{GetRouteConstraint(entityKey.Type)}}}");
                      AllowAnonymous();
                  }

                  public override async Task HandleAsync({{st.ClassDeleteModelRequest}} req, CancellationToken ct)
                  {
                      var deleted = await {{st.VarModelService}}.DeleteAsync(req.{{entityKey.Key}});
                      if (!deleted)
                      {
                          await SendNotFoundAsync(ct);
                          return;
                      }

                      await SendNoContentAsync(ct);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
