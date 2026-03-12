namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// This template generates the summary for the get endpoint of a given entity.
/// </summary>
public sealed class GetSummaryTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "GetSummary";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Summaries";
        var fileName = $"{st.ClassGetModelSummary}.cs";

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Endpoints;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassGetModelSummary}} : Summary<{{st.ClassGetModelEndpoint}}>
              {
                  public {{st.ClassGetModelSummary}}()
                  {
                      Summary = "Returns a single {{st.ModelHumanized}} by id";
                      Description = "Returns a single {{st.ModelHumanized}} by id";
                      Response<{{st.ClassModelResponse}}>(200, "Successfully found and returned the {{st.ModelHumanized}}");
                      Response(404, "The {{st.ModelHumanized}} does not exist in the system");
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
