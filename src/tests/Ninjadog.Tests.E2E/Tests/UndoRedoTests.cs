using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for undo/redo functionality.
/// </summary>
public sealed class UndoRedoTests : UiTestBase
{
    public UndoRedoTests(NinjadogUiFixture server, PlaywrightFixture pw) : base(server, pw) { }

    [Fact]
    public async Task TypeInField_EnablesUndoButton()
    {
        await NavigateToUiAsync();

        var undoDisabled = await Page.EvalOnSelectorAsync<bool>("#btn-undo", "el => el.disabled");
        undoDisabled.ShouldBeTrue();

        await FillFieldAsync("name", "TestProject");

        undoDisabled = await Page.EvalOnSelectorAsync<bool>("#btn-undo", "el => el.disabled");
        undoDisabled.ShouldBeFalse();
    }

    [Fact]
    public async Task Undo_RevertsChange()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "Before");
        // Blur to finalize
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        await FillFieldAsync("name", "After");
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        // Click undo
        await Page.ClickAsync("#btn-undo");
        await Page.WaitForTimeoutAsync(200);

        var value = await Page.EvalOnSelectorAsync<string>("[data-field='name']", "el => el.value");
        value.ShouldBe("Before");
    }

    [Fact]
    public async Task Redo_RestoresUndoneChange()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "Original");
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        await FillFieldAsync("name", "Changed");
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        // Undo
        await Page.ClickAsync("#btn-undo");
        await Page.WaitForTimeoutAsync(200);

        // Redo
        var redoDisabled = await Page.EvalOnSelectorAsync<bool>("#btn-redo", "el => el.disabled");
        redoDisabled.ShouldBeFalse();

        await Page.ClickAsync("#btn-redo");
        await Page.WaitForTimeoutAsync(200);

        var value = await Page.EvalOnSelectorAsync<string>("[data-field='name']", "el => el.value");
        value.ShouldBe("Changed");
    }

    [Fact]
    public async Task AddEntity_CanBeUndone()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Temp");
        await Page.ClickAsync("#entity-add-confirm");

        var card = await Page.QuerySelectorAsync(".entity-card[data-entity='Temp']");
        card.ShouldNotBeNull();

        // Undo
        await Page.ClickAsync("#btn-undo");
        await Page.WaitForTimeoutAsync(200);

        card = await Page.QuerySelectorAsync(".entity-card[data-entity='Temp']");
        card.ShouldBeNull();
    }

    [Fact]
    public async Task AddEnum_CanBeUndone()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "TempEnum");
        await Page.ClickAsync("#enum-add-confirm");

        var card = await Page.QuerySelectorAsync(".entity-card[data-enum='TempEnum']");
        card.ShouldNotBeNull();

        await Page.ClickAsync("#btn-undo");
        await Page.WaitForTimeoutAsync(200);

        card = await Page.QuerySelectorAsync(".entity-card[data-enum='TempEnum']");
        card.ShouldBeNull();
    }

    [Fact]
    public async Task NewEdit_ClearsRedoStack()
    {
        await NavigateToUiAsync();

        await FillFieldAsync("name", "First");
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        await FillFieldAsync("name", "Second");
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        // Undo
        await Page.ClickAsync("#btn-undo");
        await Page.WaitForTimeoutAsync(200);

        // Redo should be available
        var redoDisabled = await Page.EvalOnSelectorAsync<bool>("#btn-redo", "el => el.disabled");
        redoDisabled.ShouldBeFalse();

        // Make a new edit — should clear redo
        await FillFieldAsync("name", "Third");
        await Page.ClickAsync(".section-title");
        await Page.WaitForTimeoutAsync(100);

        redoDisabled = await Page.EvalOnSelectorAsync<bool>("#btn-redo", "el => el.disabled");
        redoDisabled.ShouldBeTrue();
    }
}
