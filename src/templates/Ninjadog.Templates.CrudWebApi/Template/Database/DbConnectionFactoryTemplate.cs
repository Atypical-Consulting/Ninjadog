namespace Ninjadog.Templates.CrudWebAPI.Template.Database;

/// <summary>
/// This template generates the DbConnectionFactory class.
/// </summary>
public sealed class DbConnectionFactoryTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DbConnectionFactory";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var provider = ninjadogSettings.Config.DatabaseProvider;
        var ns = $"{rootNamespace}.Database";
        const string fileName = "DbConnectionFactory.cs";

        var (usingDirective, className, connectionType) = DatabaseProviderHelper.GetConnectionFactoryDetails(provider);

        var content =
            $$"""

              using System.Data;
              {{usingDirective}}

              {{WriteFileScopedNamespace(ns)}}

              public interface IDbConnectionFactory
              {
                  public Task<IDbConnection> CreateConnectionAsync();
              }

              /// <summary>
              /// This class provides a factory for database connections.
              /// </summary>
              /// <param name="connectionString">The connection string to use.</param>
              public class {{className}}(string connectionString)
                  : IDbConnectionFactory
              {
                  public async Task<IDbConnection> CreateConnectionAsync()
                  {
                      var connection = new {{connectionType}}(connectionString);
                      await connection.OpenAsync();
                      return connection;
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
