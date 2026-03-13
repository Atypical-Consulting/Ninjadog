namespace Ninjadog.Templates.CrudWebAPI.Template.Summaries;

/// <summary>
/// Base class for summary templates that share the same structure.
/// Subclasses provide the summary metadata and response entries.
/// </summary>
public abstract class SummaryTemplateBase
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Summaries";
        var fileName = $"{GetSummaryClassName(st)}.cs";

        var usings = GenerateUsings(rootNamespace);
        var usingsBlock = string.IsNullOrEmpty(usings) ? string.Empty : $"{usings}\n";
        var summaryClassName = GetSummaryClassName(st);
        var endpointClassName = GetEndpointClassName(st);
        var summaryText = GetSummaryText(st);
        var descriptionAssignment = GenerateDescriptionAssignment(st);
        var responseLines = GenerateResponseLines(st);

        var content =
            $$"""

              {{usingsBlock}}using {{rootNamespace}}.Endpoints;
              using FastEndpoints;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{summaryClassName}} : Summary<{{endpointClassName}}>
              {
                  public {{summaryClassName}}()
                  {
                      Summary = "{{summaryText}}";
                      {{descriptionAssignment}}
              {{responseLines}}
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    /// <summary>
    /// Gets the summary class name from the string tokens.
    /// </summary>
    /// <param name="st">The string tokens for the entity.</param>
    /// <returns>The summary class name.</returns>
    protected abstract string GetSummaryClassName(StringTokens st);

    /// <summary>
    /// Gets the endpoint class name from the string tokens.
    /// </summary>
    /// <param name="st">The string tokens for the entity.</param>
    /// <returns>The endpoint class name.</returns>
    protected abstract string GetEndpointClassName(StringTokens st);

    /// <summary>
    /// Gets the summary text for the endpoint.
    /// </summary>
    /// <param name="st">The string tokens for the entity.</param>
    /// <returns>The summary text.</returns>
    protected abstract string GetSummaryText(StringTokens st);

    /// <summary>
    /// Generates the Description assignment statement.
    /// Override this to produce multiline C# string concatenation.
    /// </summary>
    /// <param name="st">The string tokens for the entity.</param>
    /// <returns>The full Description assignment statement.</returns>
    protected virtual string GenerateDescriptionAssignment(StringTokens st)
    {
        return $"Description = \"{GetSummaryText(st)}\";";
    }

    /// <summary>
    /// Generates the response registration lines for the summary constructor.
    /// </summary>
    /// <param name="st">The string tokens for the entity.</param>
    /// <returns>The response registration lines.</returns>
    protected abstract string GenerateResponseLines(StringTokens st);

    /// <summary>
    /// Generates additional using directives needed by the summary.
    /// Return empty string if no additional usings are needed.
    /// </summary>
    /// <param name="rootNamespace">The root namespace of the project.</param>
    /// <returns>The additional using directives.</returns>
    protected virtual string GenerateUsings(string rootNamespace)
    {
        return string.Empty;
    }
}
