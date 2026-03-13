namespace Ninjadog.Templates.CrudWebAPI.Template.Contracts.Requests;

/// <summary>
/// This template generates the Delete request for a given entity.
/// </summary>
public sealed class DeleteRequestTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DeleteRequest";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Contracts.Requests";
        var fileName = $"{st.ClassDeleteModelRequest}.cs";
        var entityKey = entity.Properties.GetEntityKey();

        var content =
            $$"""

              {{WriteFileScopedNamespace(ns)}}

              /// <summary>
              ///     Request to delete a {{st.Model}}.
              /// </summary>
              public partial class {{st.ClassDeleteModelRequest}}
              {
                  public {{entityKey.Type}} {{entityKey.PascalKey}} { get; init; }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
