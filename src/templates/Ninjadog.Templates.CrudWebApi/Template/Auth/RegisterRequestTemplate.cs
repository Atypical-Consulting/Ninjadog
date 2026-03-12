namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the RegisterRequest record.
/// </summary>
public class RegisterRequestTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "RegisterRequest";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        const string fileName = "RegisterRequest.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public record RegisterRequest(string Email, string Password, string ConfirmPassword);
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
