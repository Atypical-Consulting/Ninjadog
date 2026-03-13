namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// This template generates the summary for the update endpoint of a given entity.
/// </summary>
public sealed class UpdateSummaryTemplate
    : SummaryTemplateBase
{
    /// <inheritdoc />
    public override string Name => "UpdateSummary";

    /// <inheritdoc />
    protected override string GetSummaryClassName(StringTokens st)
    {
        return st.ClassUpdateModelSummary;
    }

    /// <inheritdoc />
    protected override string GetEndpointClassName(StringTokens st)
    {
        return st.ClassUpdateModelEndpoint;
    }

    /// <inheritdoc />
    protected override string GetSummaryText(StringTokens st)
    {
        return $"Updates an existing {st.ModelHumanized} in the system";
    }

    /// <inheritdoc />
    protected override string GenerateResponseLines(StringTokens st)
    {
        return $$"""
                         Response<{{st.ClassModelResponse}}>(201, "{{st.ModelHumanized}} was successfully updated");
                         Response<Microsoft.AspNetCore.Mvc.ProblemDetails>(400, "The request did not pass validation checks");
                 """;
    }

    /// <inheritdoc />
    protected override string GenerateUsings(string rootNamespace)
    {
        return $"""
                using Microsoft.AspNetCore.Mvc;
                using {rootNamespace}.Contracts.Responses;
                """;
    }
}
