namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the User domain entity for authentication.
/// </summary>
public class UserEntityTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "UserEntity";

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
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
