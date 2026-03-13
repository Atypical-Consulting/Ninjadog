namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the LoginResponse record.
/// </summary>
public class LoginResponseTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "LoginResponse";

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
        const string fileName = "LoginResponse.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public record LoginResponse(string Token, DateTime ExpiresAt);
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
