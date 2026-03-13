namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the RegisterResponse record.
/// </summary>
public class RegisterResponseTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "RegisterResponse";

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
        const string fileName = "RegisterResponse.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public record RegisterResponse(string UserId, string Email);
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
