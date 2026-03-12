namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the IUserRepository interface.
/// </summary>
public class UserRepositoryInterfaceTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "UserRepositoryInterface";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
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
