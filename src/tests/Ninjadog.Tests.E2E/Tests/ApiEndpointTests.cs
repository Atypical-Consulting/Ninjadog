using System.Net;
using System.Text;
using System.Text.Json;
using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for the backend API endpoints directly (no browser).
/// </summary>
[Collection("NinjadogUi")]
#pragma warning disable CS9113 // Parameter '_' is unread — required by xUnit collection fixture DI
public sealed class ApiEndpointTests(NinjadogUiFixture server, PlaywrightFixture _) : IAsyncLifetime
#pragma warning restore CS9113
{
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _client = new HttpClient { BaseAddress = new Uri(server.BaseUrl) };
        // Clean up config between tests
        if (File.Exists(server.ConfigPath))
        {
            File.Delete(server.ConfigPath);
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
        server.SeedConfigJson("""{"config":{"name":"Test"}}""");

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

        File.Exists(server.ConfigPath).ShouldBeTrue();
        var saved = File.ReadAllText(server.ConfigPath);
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
        server.SeedConfigJson("""{"config":{"name":"Build"}}""");

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
        Directory.CreateDirectory(Path.Combine(server.TempDir, "subdir"));

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
        // Get index.html first to discover the hashed CSS filename
        var indexResponse = await _client.GetAsync("/");
        var html = await indexResponse.Content.ReadAsStringAsync();
        var cssMatch = System.Text.RegularExpressions.Regex.Match(html, @"href=""\.?(/assets/index-[^""]+\.css)""");
        cssMatch.Success.ShouldBeTrue("index.html should reference a CSS asset");

        var response = await _client.GetAsync(cssMatch.Groups[1].Value);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StaticFiles_JsIsServed()
    {
        // Get index.html first to discover the hashed JS filename
        var indexResponse = await _client.GetAsync("/");
        var html = await indexResponse.Content.ReadAsStringAsync();
        var jsMatch = System.Text.RegularExpressions.Regex.Match(html, @"src=""\.?(/assets/index-[^""]+\.js)""");
        jsMatch.Success.ShouldBeTrue("index.html should reference a JS asset");

        var response = await _client.GetAsync(jsMatch.Groups[1].Value);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
