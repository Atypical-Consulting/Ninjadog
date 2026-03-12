namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// This template generates the summary for the update endpoint of a given entity.
/// </summary>
public sealed class UpdateSummaryTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "UpdateSummary";

    /// <inheritdoc/>
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Summaries";
        var fileName = $"{st.ClassUpdateModelSummary}.cs";

        var content =
            $$"""

              using Microsoft.AspNetCore.Mvc;
              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Endpoints;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassUpdateModelSummary}} : Summary<{{st.ClassUpdateModelEndpoint}}>
              {
                  public {{st.ClassUpdateModelSummary}}()
                  {
                      Summary = "Updates an existing {{st.ModelHumanized}} in the system";
                      Description = "Updates an existing {{st.ModelHumanized}} in the system";
                      Response<{{st.ClassModelResponse}}>(201, "{{st.ModelHumanized}} was successfully updated");
                      Response<ProblemDetails>(400, "The request did not pass validation checks");
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
