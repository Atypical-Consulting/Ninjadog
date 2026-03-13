using Ninjadog.Templates.CrudWebAPI.Template.Database;

namespace Ninjadog.Templates.CrudWebAPI.Template.Auth;

/// <summary>
/// Generates the UserInitializer class that creates the Users table.
/// </summary>
public class UserInitializerTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "UserInitializer";

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
        const string fileName = "UserInitializer.cs";

        var textType = DatabaseProviderHelper.MapToDbType("String", provider);
        var dateTimeType = DatabaseProviderHelper.MapToDbType("DateTime", provider);
        var nowFunction = DatabaseProviderHelper.GetNowFunction(provider);

        // UserInitializer wraps the NOW function in parens for DEFAULT clause
        var defaultNow = nowFunction.StartsWith('(') ? nowFunction : $"({nowFunction})";

        var content =
            $$"""

              using Dapper;
              using {{rootNamespace}}.Database;

              {{WriteFileScopedNamespace($"{rootNamespace}.Auth")}}

              public class UserInitializer(IDbConnectionFactory connectionFactory)
              {
                  public async Task InitializeAsync()
                  {
                      using var connection = await connectionFactory.CreateConnectionAsync();
                      await connection.ExecuteAsync(
                          @"CREATE TABLE IF NOT EXISTS Users (
                              Id {{textType}} PRIMARY KEY,
                              Email {{textType}} NOT NULL UNIQUE,
                              PasswordHash {{textType}} NOT NULL,
                              Roles {{textType}} NOT NULL DEFAULT '',
                              CreatedAt {{dateTimeType}} NOT NULL DEFAULT {{defaultNow}}
                          )");
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
