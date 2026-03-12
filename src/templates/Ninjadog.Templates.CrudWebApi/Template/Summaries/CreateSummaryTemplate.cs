namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// This template generates the summary for the create endpoint of a given entity.
/// </summary>
public sealed class CreateSummaryTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "CreateSummary";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Summaries";
        var fileName = $"{st.ClassCreateModelSummary}.cs";

        var content =
            $$"""

              using Microsoft.AspNetCore.Mvc;
              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Endpoints;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassCreateModelSummary}} : Summary<{{st.ClassCreateModelEndpoint}}>
              {
                  public {{st.ClassCreateModelSummary}}()
                  {
                      Summary = "Creates a new {{st.ModelHumanized}} in the system";
                      Description = "Creates a new {{st.ModelHumanized}} in the system";
                      Response<{{st.ClassModelResponse}}>(201, "{{st.ModelHumanized}} was successfully created");
                      Response<ProblemDetails>(400, "The request did not pass validation checks");
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
