namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the LoginResponse record.
/// </summary>
public class LoginResponseTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "LoginResponse";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        const string fileName = "LoginResponse.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public record LoginResponse(string Token, DateTime ExpiresAt);
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
