namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the LoginRequest record.
/// </summary>
public class LoginRequestTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "LoginRequest";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        const string fileName = "LoginRequest.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public record LoginRequest(string Email, string Password);
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
