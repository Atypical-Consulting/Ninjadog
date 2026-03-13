namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// This template generates the summary for the create endpoint of a given entity.
/// </summary>
public sealed class CreateSummaryTemplate
    : SummaryTemplateBase
{
    /// <inheritdoc />
    public override string Name => "CreateSummary";

    /// <inheritdoc />
    protected override string GetSummaryClassName(StringTokens st)
    {
        return st.ClassCreateModelSummary;
    }

    /// <inheritdoc />
    protected override string GetEndpointClassName(StringTokens st)
    {
        return st.ClassCreateModelEndpoint;
    }

    /// <inheritdoc />
    protected override string GetSummaryText(StringTokens st)
    {
        return $"Creates a new {st.ModelHumanized} in the system";
    }

    /// <inheritdoc />
    protected override string GenerateResponseLines(StringTokens st)
    {
        return $$"""
                         Response<{{st.ClassModelResponse}}>(201, "{{st.ModelHumanized}} was successfully created");
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
