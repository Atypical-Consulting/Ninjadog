using System.Text.Json;
using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for the Config tab — form fields, validation, and state changes.
/// </summary>
public sealed class ConfigTabTests(NinjadogUiFixture server, PlaywrightFixture pw) : UiTestBase(server, pw)
{
    [Fact]
    public async Task ConfigForm_ShowsAllSections()
    {
        await NavigateToUiAsync();

        var sections = await Page.QuerySelectorAllAsync(".section-title");
        var sectionTexts = new List<string>();
        foreach (var s in sections)
        {
            sectionTexts.Add((await s.TextContentAsync())!.Trim());
        }

        sectionTexts.ShouldContain("General");
        sectionTexts.ShouldContain("Database");
        sectionTexts.ShouldContain("CORS");
        sectionTexts.ShouldContain("Features");
    }

    [Fact]
    public async Task ConfigForm_TypeInName_SetsDirty()
    {
        await NavigateToUiAsync();

        var isDirtyBefore = await IsDirtyAsync();
        isDirtyBefore.ShouldBeFalse();

        await FillFieldAsync("name", "TestProject");

        var isDirtyAfter = await IsDirtyAsync();
        isDirtyAfter.ShouldBeTrue();
    }

    [Fact]
    public async Task ConfigForm_FillAndSave_PersistsToFile()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "MyApi");
        await FillFieldAsync("version", "2.0.0");
        await FillFieldAsync("rootNamespace", "MyCompany.Api");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();

        using var doc = JsonDocument.Parse(saved);
        var root = doc.RootElement;
        root.GetProperty("config").GetProperty("name").GetString().ShouldBe("MyApi");
        root.GetProperty("config").GetProperty("version").GetString().ShouldBe("2.0.0");
        root.GetProperty("config").GetProperty("rootNamespace").GetString().ShouldBe("MyCompany.Api");
    }

    [Fact]
    public async Task ConfigForm_DatabaseProviderDropdown_Changes()
    {
        await NavigateToUiAsync();

        // Default should be sqlite
        var defaultVal = await Page.EvalOnSelectorAsync<string>(
            "[data-field='databaseProvider']", "el => el.value");
        defaultVal.ShouldBe("sqlite");

        // Change to postgres
        await Page.SelectOptionAsync("[data-field='databaseProvider']", "postgres");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(saved);
        doc.RootElement
            .GetProperty("config")
            .GetProperty("database")
            .GetProperty("provider")
            .GetString()
            .ShouldBe("postgres");
    }

    [Fact]
    public async Task ConfigForm_SqliteProvider_NotSavedAsDatabase()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "TestApi");
        // Ensure sqlite is selected (default)
        await Page.SelectOptionAsync("[data-field='databaseProvider']", "sqlite");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(saved);
        var root = doc.RootElement;

        // sqlite should not produce a "database" property (it's the default)
        root.GetProperty("config").TryGetProperty("database", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task ConfigForm_CorsOrigins_SavesAsArray()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("corsOrigins", "http://localhost:3000, http://localhost:5000");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(saved);
        var origins = doc.RootElement
            .GetProperty("config")
            .GetProperty("cors")
            .GetProperty("origins");

        origins.GetArrayLength().ShouldBe(2);
        origins[0].GetString().ShouldBe("http://localhost:3000");
        origins[1].GetString().ShouldBe("http://localhost:5000");
    }

    [Fact]
    public async Task ConfigForm_Features_Checkboxes()
    {
        await NavigateToUiAsync();

        // Check soft delete
        await Page.CheckAsync("[data-field='softDelete']");
        // Check auditing
        await Page.CheckAsync("[data-field='auditing']");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(saved);
        var features = doc.RootElement.GetProperty("config").GetProperty("features");

        features.GetProperty("softDelete").GetBoolean().ShouldBeTrue();
        features.GetProperty("auditing").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task ConfigForm_UncheckFeatures_RemovesFeaturesProperty()
    {
        await NavigateToUiAsync();

        // Check then uncheck
        await Page.CheckAsync("[data-field='softDelete']");
        await Page.UncheckAsync("[data-field='softDelete']");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(saved);
        // After unchecking, either config has no features or config itself is absent (empty object is stripped)
        if (doc.RootElement.TryGetProperty("config", out var configEl))
        {
            configEl.TryGetProperty("features", out _).ShouldBeFalse();
        }
    }

    [Fact]
    public async Task ConfigForm_NameValidation_ShowsErrorForInvalidChars()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "Invalid Name!");

        // Trigger blur to finalize
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(200);

        var errorMsg = await Page.QuerySelectorAsync("[data-error-for='name']");
        errorMsg.ShouldNotBeNull();
        var errorText = await errorMsg.TextContentAsync();
        errorText.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ConfigForm_NamespaceValidation_ShowsErrorForBadFormat()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("rootNamespace", "lowercase.bad");

        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(200);

        var errorMsg = await Page.QuerySelectorAsync("[data-error-for='rootNamespace']");
        errorMsg.ShouldNotBeNull();
        var errorText = await errorMsg.TextContentAsync();
        errorText.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ConfigForm_LoadsExistingConfig()
    {
        Server.SeedConfigJson("""
        {
            "config": {
                "name": "PreloadedApi",
                "version": "3.0.0",
                "rootNamespace": "Preloaded.Api"
            },
            "entities": {}
        }
        """);

        await NavigateToUiAsync();

        var nameValue = await Page.EvalOnSelectorAsync<string>(
            "[data-field='name']", "el => el.value");
        nameValue.ShouldBe("PreloadedApi");

        var versionValue = await Page.EvalOnSelectorAsync<string>(
            "[data-field='version']", "el => el.value");
        versionValue.ShouldBe("3.0.0");
    }
}
