using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for view mode toggling — Split, Form Only, JSON Only.
/// </summary>
public sealed class ViewModeTests(NinjadogUiFixture server, PlaywrightFixture pw) : UiTestBase(server, pw)
{
    [Fact]
    public async Task SplitView_BothPanelsVisible()
    {
        await NavigateToUiAsync();

        var leftPanel = await Page.QuerySelectorAsync("#left-panel");
        var jsonPanel = await Page.QuerySelectorAsync("#json-panel");
        var resizeHandle = await Page.QuerySelectorAsync("#resize-handle");

        leftPanel.ShouldNotBeNull();
        jsonPanel.ShouldNotBeNull();
        resizeHandle.ShouldNotBeNull();
    }

    [Fact]
    public async Task FormOnlyView_HidesJsonPanel()
    {
        await NavigateToUiAsync();

        await Page.ClickAsync("[data-view='form']");

        var leftPanel = await Page.QuerySelectorAsync("#left-panel");
        leftPanel.ShouldNotBeNull();

        var jsonPanel = await Page.QuerySelectorAsync("#json-panel");
        jsonPanel.ShouldBeNull();

        var resizeHandle = await Page.QuerySelectorAsync("#resize-handle");
        resizeHandle.ShouldBeNull();
    }

    [Fact]
    public async Task JsonOnlyView_HidesFormPanel()
    {
        await NavigateToUiAsync();

        await Page.ClickAsync("[data-view='json']");

        var leftPanel = await Page.QuerySelectorAsync("#left-panel");
        leftPanel.ShouldBeNull();

        var jsonPanel = await Page.QuerySelectorAsync("#json-panel");
        jsonPanel.ShouldNotBeNull();
    }

    [Fact]
    public async Task JsonOnlyView_PanelExpandsFullWidth()
    {
        await NavigateToUiAsync();

        await Page.ClickAsync("[data-view='json']");

        var width = await Page.EvalOnSelectorAsync<string>("#json-panel", "el => el.style.width");
        width.ShouldBe("100%");
    }

    [Fact]
    public async Task SwitchBackToSplit_RestoresBothPanels()
    {
        await NavigateToUiAsync();

        // Go to form only
        await Page.ClickAsync("[data-view='form']");

        // Back to split
        await Page.ClickAsync("[data-view='split']");

        var leftPanel = await Page.QuerySelectorAsync("#left-panel");
        leftPanel.ShouldNotBeNull();

        var jsonPanel = await Page.QuerySelectorAsync("#json-panel");
        jsonPanel.ShouldNotBeNull();

        var resizeHandle = await Page.QuerySelectorAsync("#resize-handle");
        resizeHandle.ShouldNotBeNull();
    }

    [Fact]
    public async Task ViewToggle_ActiveStateUpdates()
    {
        await NavigateToUiAsync();

        // Initially split is active
        var splitActive = await Page.EvalOnSelectorAsync<bool>("[data-view='split']", "el => el.classList.contains('active')");
        splitActive.ShouldBeTrue();

        // Click form
        await Page.ClickAsync("[data-view='form']");
        var formActive = await Page.EvalOnSelectorAsync<bool>("[data-view='form']", "el => el.classList.contains('active')");
        formActive.ShouldBeTrue();

        splitActive = await Page.EvalOnSelectorAsync<bool>("[data-view='split']", "el => el.classList.contains('active')");
        splitActive.ShouldBeFalse();
    }

    [Fact]
    public async Task ViewToggle_VisibleInAllTabs()
    {
        await NavigateToUiAsync();

        // View toggle should be visible regardless of active tab
        foreach (var tab in new[] { "config", "entities", "enums", "seed" })
        {
            await SwitchTabAsync(tab);
            var toggle = await Page.QuerySelectorAsync(".view-toggle");
            toggle.ShouldNotBeNull($"View toggle should be visible on {tab} tab");
        }
    }
}
