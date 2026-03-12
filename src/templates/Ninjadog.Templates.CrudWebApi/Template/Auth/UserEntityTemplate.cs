namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the User domain entity for authentication.
/// </summary>
public class UserEntityTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "UserEntity";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        const string fileName = "User.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public class User
              {
                  public string Id { get; init; } = Guid.NewGuid().ToString();
                  public string Email { get; init; } = default!;
                  public string PasswordHash { get; init; } = default!;
                  public string Roles { get; init; } = string.Empty;
                  public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
