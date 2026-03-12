namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the ITokenService interface for JWT token generation.
/// </summary>
public class TokenServiceInterfaceTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "TokenServiceInterface";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        const string fileName = "ITokenService.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public interface ITokenService
              {
                  string GenerateToken(string userId, string email, IEnumerable<string> roles);
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
