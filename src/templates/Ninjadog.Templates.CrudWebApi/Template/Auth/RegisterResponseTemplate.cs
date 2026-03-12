namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the RegisterResponse record.
/// </summary>
public class RegisterResponseTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "RegisterResponse";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        const string fileName = "RegisterResponse.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public record RegisterResponse(string UserId, string Email);
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
