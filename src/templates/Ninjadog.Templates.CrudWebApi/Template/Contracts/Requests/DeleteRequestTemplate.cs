namespace Ninjadog.Templates.CrudWebAPI.Template.Contracts.Requests;

/// <summary>
/// This template generates the Delete request for a given entity.
/// </summary>
public sealed class DeleteRequestTemplate
    : KeyOnlyRequestTemplateBase
{
    /// <inheritdoc />
    public override string Name => "DeleteRequest";

    /// <inheritdoc />
    protected override string GetClassName(StringTokens st)
    {
        return st.ClassDeleteModelRequest;
    }

    /// <inheritdoc />
    protected override string GetActionVerb()
    {
        return "delete";
    }
}
