namespace Ninjadog.Templates.CrudWebAPI.Template.IntegrationTests;

/// <summary>
/// This template generates the CustomWebApplicationFactory for integration testing
/// using WebApplicationFactory with an in-memory SQLite database.
/// </summary>
public sealed class CustomWebApplicationFactoryTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "CustomWebApplicationFactory";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var projectName = ninjadogSettings.Config.Name;
        var testNamespace = $"{projectName}.IntegrationTests";
        const string fileName = "CustomWebApplicationFactory.cs";

        var content =
            $$"""

              using Microsoft.AspNetCore.Hosting;
              using Microsoft.AspNetCore.Mvc.Testing;
              using Microsoft.Extensions.DependencyInjection;
              using {{rootNamespace}}.Database;

              {{WriteFileScopedNamespace(testNamespace)}}

              /// <summary>
              /// Custom WebApplicationFactory that configures a shared in-memory SQLite database
              /// for integration testing. Uses a named cache so all connections share the same database.
              /// </summary>
              public class CustomWebApplicationFactory : WebApplicationFactory<Program>
              {
                  private const string TestConnectionString =
                      "Data Source=IntegrationTests;Mode=Memory;Cache=Shared";

                  protected override void ConfigureWebHost(IWebHostBuilder builder)
                  {
                      builder.UseEnvironment("Testing");

                      builder.ConfigureServices(services =>
                      {
                          // Replace the database connection factory with a shared in-memory SQLite instance
                          var descriptor = services.SingleOrDefault(
                              d => d.ServiceType == typeof(IDbConnectionFactory));

                          if (descriptor is not null)
                          {
                              services.Remove(descriptor);
                          }

                          services.AddSingleton<IDbConnectionFactory>(
                              _ => new SqliteConnectionFactory(TestConnectionString));
                      });
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
