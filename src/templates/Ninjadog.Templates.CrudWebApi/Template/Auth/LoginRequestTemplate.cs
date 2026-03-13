namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the LoginRequest record.
/// </summary>
public class LoginRequestTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "LoginRequest";

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
        const string fileName = "LoginRequest.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public record LoginRequest(string Email, string Password);
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
