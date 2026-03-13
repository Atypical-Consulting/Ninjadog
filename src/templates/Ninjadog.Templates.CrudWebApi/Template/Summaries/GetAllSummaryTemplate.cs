namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// This template generates the summary for the get all endpoint of a given entity.
/// </summary>
public sealed class GetAllSummaryTemplate
    : SummaryTemplateBase
{
    /// <inheritdoc />
    public override string Name => "GetAllSummary";

    /// <inheritdoc />
    protected override string GetSummaryClassName(StringTokens st)
    {
        return st.ClassGetAllModelsSummary;
    }

    /// <inheritdoc />
    protected override string GetEndpointClassName(StringTokens st)
    {
        return st.ClassGetAllModelsEndpoint;
    }

    /// <inheritdoc />
    protected override string GetSummaryText(StringTokens st)
    {
        return $"Returns all the {st.ModelsHumanized} in the system";
    }

    /// <inheritdoc />
    protected override string GenerateDescriptionAssignment(StringTokens st)
    {
        return $"""
                Description = "Returns all the {st.ModelsHumanized} in the system. "
                            + "Supports pagination (page, pageSize), filtering by property values, "
                            + "and sorting (sortBy, sortDir=asc|desc).";
                """;
    }

    /// <inheritdoc />
    protected override string GenerateResponseLines(StringTokens st)
    {
        return $$"""
                         Response<{{st.ClassGetAllModelsResponse}}>(200, "All {{st.ModelsHumanized}} in the system are returned");
                 """;
    }

    /// <inheritdoc />
    protected override string GenerateUsings(string rootNamespace)
    {
        return $"using {rootNamespace}.Contracts.Responses;";
    }
}
