using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for initial app loading, navigation, and layout.
/// </summary>
public sealed class AppLoadingTests : UiTestBase
{
    public AppLoadingTests(NinjadogUiFixture server, PlaywrightFixture pw) : base(server, pw) { }

    [Fact]
    public async Task PageLoads_WithTitle()
    {
        await NavigateToUiAsync();

        var title = await Page.TitleAsync();
        title.ShouldBe("Ninjadog Config Builder");
    }

    [Fact]
    public async Task PageLoads_ShowsHeaderWithLogo()
    {
        await NavigateToUiAsync();

        var header = await Page.QuerySelectorAsync(".header-bar");
        header.ShouldNotBeNull();

        var ninjaText = await Page.TextContentAsync("h1");
        ninjaText.ShouldContain("NINJADOG");
    }

    [Fact]
    public async Task PageLoads_ShowsFourTabs()
    {
        await NavigateToUiAsync();

        var tabs = await Page.QuerySelectorAllAsync(".tab-btn");
        tabs.Count.ShouldBe(4);

        var tabNames = new List<string>();
        foreach (var tab in tabs)
        {
            var text = (await tab.TextContentAsync())!.Trim();
            tabNames.Add(text);
        }

        tabNames[0].ShouldContain("Config");
        tabNames[1].ShouldContain("Entities");
        tabNames[2].ShouldContain("Enums");
        tabNames[3].ShouldContain("Seed Data");
    }

    [Fact]
    public async Task ConfigTabIsActiveByDefault()
    {
        await NavigateToUiAsync();

        var configTab = await Page.QuerySelectorAsync(".tab-btn[data-tab='config']");
        var isActive = await configTab!.EvaluateAsync<bool>("el => el.classList.contains('active')");
        isActive.ShouldBeTrue();

        var configContent = await Page.QuerySelectorAsync("#tab-config.tab-content-active");
        configContent.ShouldNotBeNull();
    }

    [Fact]
    public async Task TabNavigation_SwitchesBetweenTabs()
    {
        await NavigateToUiAsync();

        // Switch to Entities
        await SwitchTabAsync("entities");
        var entitiesActive = await Page.QuerySelectorAsync("#tab-entities.tab-content-active");
        entitiesActive.ShouldNotBeNull();

        // Config should no longer be active
        var configActive = await Page.QuerySelectorAsync("#tab-config.tab-content-active");
        configActive.ShouldBeNull();

        // Switch to Enums
        await SwitchTabAsync("enums");
        var enumsActive = await Page.QuerySelectorAsync("#tab-enums.tab-content-active");
        enumsActive.ShouldNotBeNull();

        // Switch to Seed Data
        await SwitchTabAsync("seed");
        var seedActive = await Page.QuerySelectorAsync("#tab-seed.tab-content-active");
        seedActive.ShouldNotBeNull();
    }

    [Fact]
    public async Task HeaderButtons_ArePresent()
    {
        await NavigateToUiAsync();

        (await Page.QuerySelectorAsync("#btn-save")).ShouldNotBeNull();
        (await Page.QuerySelectorAsync("#btn-build")).ShouldNotBeNull();
        (await Page.QuerySelectorAsync("#btn-undo")).ShouldNotBeNull();
        (await Page.QuerySelectorAsync("#btn-redo")).ShouldNotBeNull();
        (await Page.QuerySelectorAsync("#btn-export")).ShouldNotBeNull();
        (await Page.QuerySelectorAsync("#btn-shortcuts")).ShouldNotBeNull();
    }

    [Fact]
    public async Task UndoRedoButtons_DisabledInitially()
    {
        await NavigateToUiAsync();

        var undoDisabled = await Page.EvalOnSelectorAsync<bool>("#btn-undo", "el => el.disabled");
        var redoDisabled = await Page.EvalOnSelectorAsync<bool>("#btn-redo", "el => el.disabled");

        undoDisabled.ShouldBeTrue();
        redoDisabled.ShouldBeTrue();
    }

    [Fact]
    public async Task DirtyIndicator_HiddenInitially()
    {
        await NavigateToUiAsync();

        var isDirty = await IsDirtyAsync();
        isDirty.ShouldBeFalse();
    }

    [Fact]
    public async Task ViewToggle_ThreeModesPresent()
    {
        await NavigateToUiAsync();

        var splitBtn = await Page.QuerySelectorAsync("[data-view='split']");
        var formBtn = await Page.QuerySelectorAsync("[data-view='form']");
        var jsonBtn = await Page.QuerySelectorAsync("[data-view='json']");

        splitBtn.ShouldNotBeNull();
        formBtn.ShouldNotBeNull();
        jsonBtn.ShouldNotBeNull();

        // Split should be active by default
        var splitActive = await splitBtn.EvaluateAsync<bool>("el => el.classList.contains('active')");
        splitActive.ShouldBeTrue();
    }

    [Fact]
    public async Task EmptyState_ShowsTemplateAndScratchButtons()
    {
        await NavigateToUiAsync();

        var templateBtn = await Page.QuerySelectorAsync("#btn-start-template");
        var scratchBtn = await Page.QuerySelectorAsync("#btn-start-scratch");

        templateBtn.ShouldNotBeNull();
        scratchBtn.ShouldNotBeNull();
    }

    [Fact]
    public async Task ValidationPanel_IsVisible()
    {
        await NavigateToUiAsync();

        var panel = await Page.QuerySelectorAsync("#validation-panel");
        panel.ShouldNotBeNull();
    }
}
