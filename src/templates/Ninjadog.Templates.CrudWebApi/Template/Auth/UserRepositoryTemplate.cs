namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the UserRepository class with Dapper implementation.
/// </summary>
public class UserRepositoryTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "UserRepository";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var auth = ninjadogSettings.Config.Auth;
        if (auth is null)
        {
            return NinjadogContentFile.Empty;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var provider = ninjadogSettings.Config.DatabaseProvider;
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
