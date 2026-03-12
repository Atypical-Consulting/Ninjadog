using System.Text.Json;
using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for save, build, and export functionality.
/// </summary>
public sealed class SaveBuildTests : UiTestBase
{
    public SaveBuildTests(NinjadogUiFixture server, PlaywrightFixture pw) : base(server, pw) { }

    [Fact]
    public async Task Save_WritesConfigToFile()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "SaveTest");
        await Page.ClickAsync("#btn-save");

        await WaitForToastAsync("Configuration saved");

        File.Exists(Server.ConfigPath).ShouldBeTrue();
        var content = File.ReadAllText(Server.ConfigPath);
        content.ShouldContain("SaveTest");
    }

    [Fact]
    public async Task Save_ClearsDirtyIndicator()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "DirtyTest");

        var isDirty = await IsDirtyAsync();
        isDirty.ShouldBeTrue();

        await SaveAndWaitAsync();

        isDirty = await IsDirtyAsync();
        isDirty.ShouldBeFalse();
    }

    [Fact]
    public async Task Build_NoConfig_ShowsBuildConsole()
    {
        // Delete config if it exists
        if (File.Exists(Server.ConfigPath))
        {
            File.Delete(Server.ConfigPath);
        }

        await NavigateToUiAsync();

        // Build should save first, then build
        await FillFieldAsync("name", "BuildTest");
        await Page.ClickAsync("#btn-build");

        // Build console should appear
        var console = await Page.WaitForSelectorAsync("#build-console:not(.hidden)");
        console.ShouldNotBeNull();
    }

    [Fact]
    public async Task Build_WithConfig_ShowsSuccess()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "BuildSuccess");
        await Page.ClickAsync("#btn-build");

        // Should save first, so config exists
        await Page.WaitForTimeoutAsync(500);

        // Build console should show
        var console = await Page.WaitForSelectorAsync("#build-console:not(.hidden)");
        console.ShouldNotBeNull();

        // Toast should show success
        await WaitForToastAsync("Build succeeded");
    }

    [Fact]
    public async Task BuildConsole_CloseButton()
    {
        await NavigateToUiAsync();
        await FillFieldAsync("name", "CloseTest");
        await Page.ClickAsync("#btn-build");

        await Page.WaitForSelectorAsync("#build-console:not(.hidden)");

        await Page.ClickAsync("#build-console-close");

        var hidden = await Page.EvalOnSelectorAsync<bool>("#build-console", "el => el.classList.contains('hidden')");
        hidden.ShouldBeTrue();
    }

    [Fact]
    public async Task Save_ValidJson_RoundTrips()
    {
        await NavigateToUiAsync();

        // Setup a full config through the template
        await Page.ClickAsync("#btn-start-template");
        await Page.ClickAsync("[data-template='todo']");
        await WaitForToastAsync("Template applied");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();

        // Verify it's valid JSON
        var action = () => JsonDocument.Parse(saved);
        action.ShouldNotThrow();
    }

    [Fact]
    public async Task MultipleSaves_OverwriteCorrectly()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "First");
        await SaveAndWaitAsync();

        var first = Server.ReadSavedConfig();
        first.ShouldContain("First");

        await FillFieldAsync("name", "Second");
        await SaveAndWaitAsync();

        var second = Server.ReadSavedConfig();
        second.ShouldContain("Second");
        second.ShouldNotContain("\"First\"");
    }
}
