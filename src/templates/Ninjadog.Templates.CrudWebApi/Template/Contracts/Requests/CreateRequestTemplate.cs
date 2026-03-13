namespace Ninjadog.Templates.CrudWebAPI.Template.Contracts.Requests;

/// <summary>
/// This template generates the Create request for a given entity.
/// </summary>
public sealed class CreateRequestTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "CreateRequest";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Contracts.Requests";
        var fileName = $"{st.ClassCreateModelRequest}.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace(ns)}}

              /// <summary>
              ///     Request to create a {{st.Model}}.
              /// </summary>
              public partial class {{st.ClassCreateModelRequest}}
              {
              {{entity.GenerateMemberProperties(excludeAutoKey: true)}}
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
