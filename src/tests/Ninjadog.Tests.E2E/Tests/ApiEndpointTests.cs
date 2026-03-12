using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for the backend API endpoints directly (no browser).
/// </summary>
[Collection("NinjadogUi")]
public sealed class ApiEndpointTests : IAsyncLifetime
{
    private readonly NinjadogUiFixture _server;
    private HttpClient _client = null!;

    public ApiEndpointTests(NinjadogUiFixture server, PlaywrightFixture _)
    {
        _server = server;
    }

    public Task InitializeAsync()
    {
        _client = new HttpClient { BaseAddress = new Uri(_server.BaseUrl) };
        // Clean up config between tests
        if (File.Exists(_server.ConfigPath))
        {
            File.Delete(_server.ConfigPath);
        }

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetConfig_NoFile_ReturnsEmptyObject()
    {
        var response = await _client.GetAsync("/api/config");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldBe("{}");
    }

    [Fact]
    public async Task GetConfig_WithFile_ReturnsContent()
    {
        _server.SeedConfigJson("""{"config":{"name":"Test"}}""");

        var response = await _client.GetAsync("/api/config");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Test");
    }

    [Fact]
    public async Task PostConfig_ValidJson_Saves()
    {
        var json = """{"config":{"name":"Posted"}}""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/config", content);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        File.Exists(_server.ConfigPath).ShouldBeTrue();
        var saved = File.ReadAllText(_server.ConfigPath);
        saved.ShouldContain("Posted");
    }

    [Fact]
    public async Task PostConfig_InvalidJson_Returns400()
    {
        var content = new StringContent("not json{{{", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/config", content);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("error");
    }

    [Fact]
    public async Task PostValidate_ReturnsResult()
    {
        var json = """{"config":{"name":"Valid"}}""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/validate", content);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostBuild_NoConfig_Returns400()
    {
        var response = await _client.PostAsync("/api/build", null);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostBuild_WithConfig_ReturnsSuccess()
    {
        _server.SeedConfigJson("""{"config":{"name":"Build"}}""");

        var response = await _client.PostAsync("/api/build", null);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("success");
    }

    [Fact]
    public async Task GetSchema_ReturnsJsonSchema()
    {
        var response = await _client.GetAsync("/api/schema");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldNotBeNullOrEmpty();

        // Should be valid JSON
        var action = () => JsonDocument.Parse(body);
        action.ShouldNotThrow();
    }

    [Fact]
    public async Task GetDirectories_ReturnsDirectoryList()
    {
        // Create a subdirectory
        Directory.CreateDirectory(Path.Combine(_server.TempDir, "subdir"));

        var response = await _client.GetAsync("/api/directories?path=.");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("subdir");
    }

    [Fact]
    public async Task GetDirectories_InvalidPath_Returns404()
    {
        var response = await _client.GetAsync("/api/directories?path=nonexistent-folder");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRoot_ReturnsHtml()
    {
        var response = await _client.GetAsync("/");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.ShouldBe("text/html");

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Ninjadog Config Builder");
    }

    [Fact]
    public async Task StaticFiles_CssIsServed()
    {
        var response = await _client.GetAsync("/css/app.css");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StaticFiles_JsIsServed()
    {
        var response = await _client.GetAsync("/js/app.js");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
