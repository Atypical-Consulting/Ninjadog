namespace Ninjadog.Templates.CrudWebAPI.Template.Contracts.Requests;

/// <summary>
/// This template generates the Get request for a given entity.
/// </summary>
public sealed class GetRequestTemplate
    : KeyOnlyRequestTemplateBase
{
    /// <inheritdoc />
    public override string Name => "GetRequest";

    /// <inheritdoc />
    protected override string GetClassName(StringTokens st)
    {
        return st.ClassGetModelRequest;
    }

    /// <inheritdoc />
    protected override string GetActionVerb()
    {
        return "get";
    }
}
