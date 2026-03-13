namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// This template generates the summary for the delete endpoint of a given entity.
/// </summary>
public sealed class DeleteSummaryTemplate
    : SummaryTemplateBase
{
    /// <inheritdoc />
    public override string Name => "DeleteSummary";

    /// <inheritdoc />
    protected override string GetSummaryClassName(StringTokens st)
    {
        return st.ClassDeleteModelSummary;
    }

    /// <inheritdoc />
    protected override string GetEndpointClassName(StringTokens st)
    {
        return st.ClassDeleteModelEndpoint;
    }

    /// <inheritdoc />
    protected override string GetSummaryText(StringTokens st)
    {
        return $"Delete a {st.ModelHumanized} in the system";
    }

    /// <inheritdoc />
    protected override string GenerateResponseLines(StringTokens st)
    {
        return $$"""
                         Response(204, "The {{st.ModelHumanized}} was deleted successfully");
                         Response(404, "The {{st.ModelHumanized}} was not found in the system");
                 """;
    }
}
