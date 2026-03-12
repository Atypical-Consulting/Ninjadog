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
                              Id {{GetTextType(provider)}} PRIMARY KEY,
                              Email {{GetTextType(provider)}} NOT NULL UNIQUE,
                              PasswordHash {{GetTextType(provider)}} NOT NULL,
                              Roles {{GetTextType(provider)}} NOT NULL DEFAULT '',
                              CreatedAt {{GetDateTimeType(provider)}} NOT NULL DEFAULT {{GetNowFunction(provider)}}
                          )");
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GetTextType(string provider)
    {
        return provider switch
        {
            "postgresql" => "TEXT",
            "sqlserver" => "NVARCHAR(MAX)",
            _ => "TEXT"
        };
    }

    private static string GetDateTimeType(string provider)
    {
        return provider switch
        {
            "postgresql" => "TIMESTAMP",
            "sqlserver" => "DATETIME2",
            _ => "TEXT"
        };
    }

    private static string GetNowFunction(string provider)
    {
        return provider switch
        {
            "postgresql" => "NOW()",
            "sqlserver" => "GETUTCDATE()",
            _ => "(datetime('now'))"
        };
    }
}
