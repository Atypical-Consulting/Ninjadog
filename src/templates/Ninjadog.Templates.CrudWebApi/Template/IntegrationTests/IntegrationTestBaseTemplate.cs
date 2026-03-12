namespace Ninjadog.Templates.CrudWebAPI.Template.IntegrationTests;

/// <summary>
/// This template generates the base class for integration tests,
/// providing shared HttpClient and JSON serialization helpers.
/// </summary>
public sealed class IntegrationTestBaseTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "IntegrationTestBase";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var projectName = ninjadogSettings.Config.Name;
        var testNamespace = $"{projectName}.IntegrationTests";
        const string fileName = "IntegrationTestBase.cs";

        var content =
            $$"""

              using System.Text.Json;

              {{WriteFileScopedNamespace(testNamespace)}}

              /// <summary>
              /// Base class for integration tests. Provides an HttpClient configured
              /// against the CustomWebApplicationFactory and JSON deserialization helpers.
              /// </summary>
              public abstract class IntegrationTestBase
                  : IClassFixture<CustomWebApplicationFactory>, IDisposable
              {
                  private static readonly JsonSerializerOptions JsonOptions = new()
                  {
                      PropertyNameCaseInsensitive = true
                  };

                  protected HttpClient Client { get; }

                  protected IntegrationTestBase(CustomWebApplicationFactory factory)
                  {
                      Client = factory.CreateClient();
                  }

                  protected static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
                  {
                      var stream = await response.Content.ReadAsStreamAsync();
                      return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
                  }

                  public void Dispose()
                  {
                      Client.Dispose();
                      GC.SuppressFinalize(this);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
