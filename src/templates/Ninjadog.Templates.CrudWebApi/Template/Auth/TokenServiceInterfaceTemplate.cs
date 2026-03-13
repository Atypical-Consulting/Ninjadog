namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the ITokenService interface for JWT token generation.
/// </summary>
public class TokenServiceInterfaceTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "TokenServiceInterface";

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
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
