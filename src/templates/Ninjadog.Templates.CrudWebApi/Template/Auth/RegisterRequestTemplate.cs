namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the RegisterRequest record.
/// </summary>
public class RegisterRequestTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "RegisterRequest";

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
        const string fileName = "RegisterRequest.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public record RegisterRequest(string Email, string Password, string ConfirmPassword);
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
