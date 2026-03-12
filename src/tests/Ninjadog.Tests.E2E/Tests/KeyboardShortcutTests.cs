using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for keyboard shortcuts and the shortcut overlay.
/// </summary>
public sealed class KeyboardShortcutTests : UiTestBase
{
    public KeyboardShortcutTests(NinjadogUiFixture server, PlaywrightFixture pw) : base(server, pw) { }

    [Fact]
    public async Task QuestionMark_TogglesShortcutOverlay()
    {
        await NavigateToUiAsync();

        // Press ?
        await Page.Keyboard.PressAsync("?");

        var overlay = await Page.WaitForSelectorAsync("#shortcut-overlay:not(.hidden)");
        overlay.ShouldNotBeNull();

        // Press ? again to close
        await Page.Keyboard.PressAsync("?");
        await Page.WaitForTimeoutAsync(200);

        var hidden = await Page.EvalOnSelectorAsync<bool>("#shortcut-overlay", "el => el.classList.contains('hidden')");
        hidden.ShouldBeTrue();
    }

    [Fact]
    public async Task ShortcutButton_OpensOverlay()
    {
        await NavigateToUiAsync();

        await Page.ClickAsync("#btn-shortcuts");

        var overlay = await Page.WaitForSelectorAsync("#shortcut-overlay:not(.hidden)");
        overlay.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShortcutOverlay_CloseButton()
    {
        await NavigateToUiAsync();

        await Page.ClickAsync("#btn-shortcuts");
        await Page.WaitForSelectorAsync("#shortcut-overlay:not(.hidden)");

        await Page.ClickAsync("#shortcut-overlay-close");
        await Page.WaitForTimeoutAsync(200);

        var hidden = await Page.EvalOnSelectorAsync<bool>("#shortcut-overlay", "el => el.classList.contains('hidden')");
        hidden.ShouldBeTrue();
    }

    [Fact]
    public async Task Escape_ClosesOverlay()
    {
        await NavigateToUiAsync();

        await Page.ClickAsync("#btn-shortcuts");
        await Page.WaitForSelectorAsync("#shortcut-overlay:not(.hidden)");

        await Page.Keyboard.PressAsync("Escape");
        await Page.WaitForTimeoutAsync(200);

        var hidden = await Page.EvalOnSelectorAsync<bool>("#shortcut-overlay", "el => el.classList.contains('hidden')");
        hidden.ShouldBeTrue();
    }

    [Fact]
    public async Task Escape_ClosesTemplatePicker()
    {
        await NavigateToUiAsync();

        await Page.ClickAsync("#btn-start-template");
        await Page.WaitForSelectorAsync("#template-picker:not(.hidden)");

        await Page.Keyboard.PressAsync("Escape");
        await Page.WaitForTimeoutAsync(200);

        var hidden = await Page.EvalOnSelectorAsync<bool>("#template-picker", "el => el.classList.contains('hidden')");
        hidden.ShouldBeTrue();
    }

    [Fact]
    public async Task CtrlS_SavesConfig()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "KeyboardSave");

        await Page.Keyboard.PressAsync("Control+s");

        await WaitForToastAsync("Configuration saved");
    }

    [Fact]
    public async Task CtrlZ_PerformsUndo()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "BeforeUndo");
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        await FillFieldAsync("name", "AfterEdit");
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        await Page.Keyboard.PressAsync("Control+z");
        await Page.WaitForTimeoutAsync(200);

        var value = await Page.EvalOnSelectorAsync<string>("[data-field='name']", "el => el.value");
        value.ShouldBe("BeforeUndo");
    }

    [Fact]
    public async Task CtrlY_PerformsRedo()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "Step1");
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        await FillFieldAsync("name", "Step2");
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        await Page.Keyboard.PressAsync("Control+z");
        await Page.WaitForTimeoutAsync(200);

        await Page.Keyboard.PressAsync("Control+y");
        await Page.WaitForTimeoutAsync(200);

        var value = await Page.EvalOnSelectorAsync<string>("[data-field='name']", "el => el.value");
        value.ShouldBe("Step2");
    }

    [Fact]
    public async Task ShortcutOverlay_ShowsAllShortcuts()
    {
        await NavigateToUiAsync();

        await Page.ClickAsync("#btn-shortcuts");
        await Page.WaitForSelectorAsync("#shortcut-overlay:not(.hidden)");

        var rows = await Page.QuerySelectorAllAsync(".shortcut-row");
        rows.Count.ShouldBeGreaterThanOrEqualTo(6);

        var overlayText = await Page.TextContentAsync("#shortcut-overlay");
        overlayText.ShouldContain("Save");
        overlayText.ShouldContain("Undo");
        overlayText.ShouldContain("Redo");
        overlayText.ShouldContain("Build");
    }
}
