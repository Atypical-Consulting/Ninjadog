namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// This template generates the summary for the delete endpoint of a given entity.
/// </summary>
public sealed class DeleteSummaryTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DeleteSummary";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Summaries";
        var fileName = $"{st.ClassDeleteModelSummary}.cs";

        var content =
            $$"""

              using {{rootNamespace}}.Endpoints;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassDeleteModelSummary}} : Summary<{{st.ClassDeleteModelEndpoint}}>
              {
                  public {{st.ClassDeleteModelSummary}}()
                  {
                      Summary = "Delete a {{st.ModelHumanized}} in the system";
                      Description = "Delete a {{st.ModelHumanized}} in the system";
                      Response(204, "The {{st.ModelHumanized}} was deleted successfully");
                      Response(404, "The {{st.ModelHumanized}} was not found in the system");
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
