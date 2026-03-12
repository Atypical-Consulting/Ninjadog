namespace Ninjadog.Templates.CrudWebAPI.Template.IntegrationTests;

/// <summary>
/// This template generates a per-entity integration test class that exercises
/// the full CRUD lifecycle (Create, GetAll, Get, Update, Delete) via HTTP.
/// </summary>
public sealed class EntityIntegrationTestTemplate : NinjadogTemplate
{
    private string _testNamespace = string.Empty;

    /// <inheritdoc />
    public override string Name => "EntityIntegrationTest";

    /// <inheritdoc />
    public override IEnumerable<NinjadogContentFile> GenerateMany(NinjadogSettings ninjadogSettings)
    {
        var projectName = ninjadogSettings.Config.Name;
        _testNamespace = $"{projectName}.IntegrationTests";
        return base.GenerateMany(ninjadogSettings);
    }

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var entityKey = entity.Properties.GetEntityKey();
        var fileName = $"{st.Model}EndpointTests.cs";

        // Derive namespace locally so standalone calls work correctly
        var testNamespace = string.IsNullOrEmpty(_testNamespace)
            ? $"{rootNamespace.Split('.')[0]}.IntegrationTests"
            : _testNamespace;

        var nonKeyProps = entity.Properties
            .Where(p => !p.Value.IsKey)
            .ToList();

        var content =
            $$"""

              using System.Net;
              using System.Net.Http.Json;
              using {{rootNamespace}}.Contracts.Requests;
              using {{rootNamespace}}.Contracts.Responses;
              using FluentAssertions;

              {{WriteFileScopedNamespace(testNamespace)}}

              /// <summary>
              /// Integration tests for the {{st.Model}} CRUD endpoints.
              /// Tests the full lifecycle: Create → GetAll → Get → Update → Delete.
              /// </summary>
              public class {{st.Model}}EndpointTests : IntegrationTestBase
              {
                  private const string BaseUrl = "{{st.ModelEndpoint}}";

                  public {{st.Model}}EndpointTests(CustomWebApplicationFactory factory)
                      : base(factory)
                  {
                  }

                  [Fact]
                  public async Task Create{{st.Model}}_WithValidRequest_ReturnsCreated()
                  {
                      // Arrange
                      var request = {{GenerateRequestObject(nonKeyProps, st.ClassCreateModelRequest, "create")}};

                      // Act
                      var response = await Client.PostAsJsonAsync(BaseUrl, request);

                      // Assert
                      response.StatusCode.Should().Be(HttpStatusCode.Created);
                      var created = await DeserializeAsync<{{st.ClassModelResponse}}>(response);
                      created.Should().NotBeNull();
              {{GenerateAssertions(nonKeyProps, "created", "create")}}
                  }

                  [Fact]
                  public async Task GetAll{{st.Models}}_AfterCreating_ReturnsCollection()
                  {
                      // Arrange
                      var request = {{GenerateRequestObject(nonKeyProps, st.ClassCreateModelRequest, "create")}};
                      await Client.PostAsJsonAsync(BaseUrl, request);

                      // Act
                      var response = await Client.GetAsync(BaseUrl);

                      // Assert
                      response.StatusCode.Should().Be(HttpStatusCode.OK);
                      var result = await DeserializeAsync<{{st.ClassGetAllModelsResponse}}>(response);
                      result.Should().NotBeNull();
                      result!.{{st.Models}}.Should().NotBeEmpty();
                  }

                  [Fact]
                  public async Task Get{{st.Model}}_WithValidId_ReturnsEntity()
                  {
                      // Arrange
                      var request = {{GenerateRequestObject(nonKeyProps, st.ClassCreateModelRequest, "create")}};
                      var createResponse = await Client.PostAsJsonAsync(BaseUrl, request);
                      var created = await DeserializeAsync<{{st.ClassModelResponse}}>(createResponse);

                      // Act
                      var response = await Client.GetAsync($"{BaseUrl}/{created!.{{entityKey.Key}}}");

                      // Assert
                      response.StatusCode.Should().Be(HttpStatusCode.OK);
                      var result = await DeserializeAsync<{{st.ClassModelResponse}}>(response);
                      result.Should().NotBeNull();
                      result!.{{entityKey.Key}}.Should().Be(created.{{entityKey.Key}});
                  }

                  [Fact]
                  public async Task Get{{st.Model}}_WithInvalidId_ReturnsNotFound()
                  {
                      // Act
                      var response = await Client.GetAsync($"{BaseUrl}/{{GenerateInvalidId(entityKey.Type)}}");

                      // Assert
                      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
                  }

                  [Fact]
                  public async Task Update{{st.Model}}_WithValidRequest_ReturnsOk()
                  {
                      // Arrange
                      var createRequest = {{GenerateRequestObject(nonKeyProps, st.ClassCreateModelRequest, "create")}};
                      var createResponse = await Client.PostAsJsonAsync(BaseUrl, createRequest);
                      var created = await DeserializeAsync<{{st.ClassModelResponse}}>(createResponse);

                      var updateRequest = {{GenerateRequestObject(nonKeyProps, st.ClassUpdateModelRequest, "update")}};

                      // Act
                      var response = await Client.PutAsJsonAsync(
                          $"{BaseUrl}/{created!.{{entityKey.Key}}}", updateRequest);

                      // Assert
                      response.StatusCode.Should().Be(HttpStatusCode.OK);
                      var updated = await DeserializeAsync<{{st.ClassModelResponse}}>(response);
                      updated.Should().NotBeNull();
              {{GenerateAssertions(nonKeyProps, "updated", "update")}}
                  }

                  [Fact]
                  public async Task Delete{{st.Model}}_WithValidId_ReturnsNoContent()
                  {
                      // Arrange
                      var request = {{GenerateRequestObject(nonKeyProps, st.ClassCreateModelRequest, "create")}};
                      var createResponse = await Client.PostAsJsonAsync(BaseUrl, request);
                      var created = await DeserializeAsync<{{st.ClassModelResponse}}>(createResponse);

                      // Act
                      var response = await Client.DeleteAsync($"{BaseUrl}/{created!.{{entityKey.Key}}}");

                      // Assert
                      response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                  }

                  [Fact]
                  public async Task Delete{{st.Model}}_WithInvalidId_ReturnsNotFound()
                  {
                      // Act
                      var response = await Client.DeleteAsync($"{BaseUrl}/{{GenerateInvalidId(entityKey.Type)}}");

                      // Assert
                      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
                  }

                  [Fact]
                  public async Task FullCrudLifecycle_{{st.Model}}_WorksEndToEnd()
                  {
                      // Create
                      var createRequest = {{GenerateRequestObject(nonKeyProps, st.ClassCreateModelRequest, "create")}};
                      var createResponse = await Client.PostAsJsonAsync(BaseUrl, createRequest);
                      createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
                      var created = await DeserializeAsync<{{st.ClassModelResponse}}>(createResponse);

                      // Read
                      var getResponse = await Client.GetAsync($"{BaseUrl}/{created!.{{entityKey.Key}}}");
                      getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                      // Update
                      var updateRequest = {{GenerateRequestObject(nonKeyProps, st.ClassUpdateModelRequest, "update")}};
                      var updateResponse = await Client.PutAsJsonAsync(
                          $"{BaseUrl}/{created.{{entityKey.Key}}}", updateRequest);
                      updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                      // Delete
                      var deleteResponse = await Client.DeleteAsync($"{BaseUrl}/{created.{{entityKey.Key}}}");
                      deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

                      // Verify deleted
                      var verifyResponse = await Client.GetAsync($"{BaseUrl}/{created.{{entityKey.Key}}}");
                      verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateRequestObject(
        List<KeyValuePair<string, NinjadogEntityProperty>> nonKeyProps,
        string className,
        string variant)
    {
        if (nonKeyProps.Count == 0)
        {
            return $"new {className}()";
        }

        IndentedStringBuilder sb = new(0);
        sb.Append($"new {className}")
            .AppendLine()
            .AppendLine("        {");

        for (var i = 0; i < nonKeyProps.Count; i++)
        {
            var (key, prop) = nonKeyProps[i];
            var value = GetSampleValue(prop.Type, key, variant);
            var comma = i < nonKeyProps.Count - 1 ? "," : string.Empty;
            sb.AppendLine($"            {key} = {value}{comma}");
        }

        sb.Append("        }");

        return sb.ToString();
    }

    private static string GenerateAssertions(
        List<KeyValuePair<string, NinjadogEntityProperty>> nonKeyProps,
        string targetVariable,
        string variant)
    {
        var assertProps = nonKeyProps.Take(2).ToList();

        if (assertProps.Count == 0)
        {
            return string.Empty;
        }

        IndentedStringBuilder sb = new(0);

        foreach (var (key, prop) in assertProps)
        {
            var value = GetSampleValue(prop.Type, key, variant);
            sb.AppendLine($"        {targetVariable}!.{key}.Should().Be({value});");
        }

        return sb.ToString();
    }

    private static string GenerateInvalidId(string keyType)
    {
        return keyType switch
        {
            "Guid" => "{Guid.Empty}",
            "Int32" => "999999",
            "Int64" => "999999",
            _ => "{\"nonexistent\"}"
        };
    }

    private static string GetSampleValue(string type, string propertyName, string variant)
    {
        var isUpdate = variant == "update";
        var suffix = isUpdate ? " Updated" : string.Empty;

        return type switch
        {
            "String" or "string" => $"\"Test {propertyName}{suffix}\"",
            "Bool" or "bool" or "Boolean" => isUpdate ? "true" : "false",
            "Int32" or "int" => isUpdate ? "42" : "1",
            "Int64" or "long" => isUpdate ? "42L" : "1L",
            "Decimal" or "decimal" => isUpdate ? "99.99m" : "19.99m",
            "DateTime" => isUpdate
                ? "DateTime.UtcNow.AddDays(7)"
                : "DateTime.UtcNow.AddDays(1)",
            "Guid" => "Guid.NewGuid()",
            _ => $"default({type})"
        };
    }
}
