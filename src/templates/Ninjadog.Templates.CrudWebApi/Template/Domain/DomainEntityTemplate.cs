namespace Ninjadog.Templates.CrudWebAPI.Template.Domain;

/// <summary>
/// This template generates the domain entity class for a given entity.
/// </summary>
public sealed class DomainEntityTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DomainEntity";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Domain";
        var fileName = $"{st.Model}.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.Model}}
              {
              {{entity.GenerateMemberProperties()}}
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
