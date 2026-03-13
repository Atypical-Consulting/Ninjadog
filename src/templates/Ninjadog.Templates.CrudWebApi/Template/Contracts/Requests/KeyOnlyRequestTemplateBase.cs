namespace Ninjadog.Templates.CrudWebAPI.Template.Contracts.Requests;

/// <summary>
/// Base class for request templates that contain only the entity key property
/// (e.g., Get and Delete requests).
/// </summary>
public abstract class KeyOnlyRequestTemplateBase
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Contracts.Requests";
        var className = GetClassName(st);
        var fileName = $"{className}.cs";
        var entityKey = entity.Properties.GetEntityKey();
        var actionVerb = GetActionVerb();

        var content =
            $$"""

              {{WriteFileScopedNamespace(ns)}}

              /// <summary>
              ///     Request to {{actionVerb}} a {{st.Model}}.
              /// </summary>
              public partial class {{className}}
              {
                  public {{entityKey.Type}} {{entityKey.PascalKey}} { get; init; }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    /// <summary>
    /// Gets the request class name from the string tokens.
    /// </summary>
    /// <param name="st">The string tokens for the entity.</param>
    /// <returns>The request class name.</returns>
    protected abstract string GetClassName(StringTokens st);

    /// <summary>
    /// Gets the action verb for the XML doc comment (e.g., "get", "delete").
    /// </summary>
    /// <returns>The action verb.</returns>
    protected abstract string GetActionVerb();
}
