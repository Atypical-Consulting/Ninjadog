namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the IUserRepository interface.
/// </summary>
public class UserRepositoryInterfaceTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "UserRepositoryInterface";

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
        const string fileName = "IUserRepository.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public interface IUserRepository
              {
                  Task<User?> GetByEmailAsync(string email);
                  Task<bool> ExistsAsync(string email);
                  Task CreateAsync(User user);
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
