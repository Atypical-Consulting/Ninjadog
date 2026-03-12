namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// This template generates the summary for the get all endpoint of a given entity.
/// </summary>
public sealed class GetAllSummaryTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "GetAllSummary";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Summaries";
        var fileName = $"{st.ClassGetAllModelsSummary}.cs";

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Endpoints;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassGetAllModelsSummary}} : Summary<{{st.ClassGetAllModelsEndpoint}}>
              {
                  public {{st.ClassGetAllModelsSummary}}()
                  {
                      Summary = "Returns all the {{st.ModelsHumanized}} in the system";
                      Description = "Returns all the {{st.ModelsHumanized}} in the system";
                      Response<{{st.ClassGetAllModelsResponse}}>(200, "All {{st.ModelsHumanized}} in the system are returned");
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
