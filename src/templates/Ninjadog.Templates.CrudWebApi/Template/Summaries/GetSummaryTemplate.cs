namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// This template generates the summary for the get endpoint of a given entity.
/// </summary>
public sealed class GetSummaryTemplate
    : SummaryTemplateBase
{
    /// <inheritdoc />
    public override string Name => "GetSummary";

    /// <inheritdoc />
    protected override string GetSummaryClassName(StringTokens st)
    {
        return st.ClassGetModelSummary;
    }

    /// <inheritdoc />
    protected override string GetEndpointClassName(StringTokens st)
    {
        return st.ClassGetModelEndpoint;
    }

    /// <inheritdoc />
    protected override string GetSummaryText(StringTokens st)
    {
        return $"Returns a single {st.ModelHumanized} by id";
    }

    /// <inheritdoc />
    protected override string GenerateResponseLines(StringTokens st)
    {
        return $$"""
                         Response<{{st.ClassModelResponse}}>(200, "Successfully found and returned the {{st.ModelHumanized}}");
                         Response(404, "The {{st.ModelHumanized}} does not exist in the system");
                 """;
    }

    /// <inheritdoc />
    protected override string GenerateUsings(string rootNamespace)
    {
        return $"using {rootNamespace}.Contracts.Responses;";
    }
}
