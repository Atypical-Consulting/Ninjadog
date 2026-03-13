namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the UserRepository class with Dapper implementation.
/// </summary>
public class UserRepositoryTemplate : AuthTemplateBase
{
    /// <inheritdoc />
    public override string Name => "UserRepository";

    /// <inheritdoc />
    protected override NinjadogContentFile GenerateAuthContent(
        NinjadogSettings settings, NinjadogAuthConfiguration auth)
    {
        var rootNamespace = settings.Config.RootNamespace;
        var provider = settings.Config.DatabaseProvider;
        const string fileName = "UserRepository.cs";

        var content =
            $$"""

              using Dapper;
              using {{rootNamespace}}.Database;

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
              {
                  public async Task<User?> GetByEmailAsync(string email)
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();
                      return await connection.QuerySingleOrDefaultAsync<User>(
                          "SELECT Id, Email, PasswordHash, Roles, CreatedAt FROM Users WHERE Email = @Email{{GetLimitClause(provider)}}",
                          new { Email = email });
                  }

                  public async Task<bool> ExistsAsync(string email)
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();
                      return await connection.ExecuteScalarAsync<bool>(
                          "SELECT COUNT(1) FROM Users WHERE Email = @Email",
                          new { Email = email });
                  }

                  public async Task CreateAsync(User user)
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();
                      await connection.ExecuteAsync(
                          "INSERT INTO Users (Id, Email, PasswordHash, Roles, CreatedAt) VALUES (@Id, @Email, @PasswordHash, @Roles, @CreatedAt)",
                          user);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GetLimitClause(string provider)
    {
        return provider switch
        {
            "sqlserver" => " ORDER BY Email OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY",
            _ => " LIMIT 1"
        };
    }
}
