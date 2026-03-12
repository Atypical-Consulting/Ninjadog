using System.Text.Json;
using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for the Entities tab — add/remove/clone entities, properties, relationships, presets.
/// </summary>
public sealed class EntityTabTests : UiTestBase
{
    public EntityTabTests(NinjadogUiFixture server, PlaywrightFixture pw) : base(server, pw) { }

    [Fact]
    public async Task AddEntity_InlineForm_CreatesEntity()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        // Click "+ Add Entity"
        await Page.ClickAsync("#btn-add-entity");

        // Form should appear
        var addForm = await Page.QuerySelectorAsync("#entity-add-form:not(.hidden)");
        addForm.ShouldNotBeNull();

        // Type name and confirm
        await Page.FillAsync("#entity-add-input", "Product");
        await Page.ClickAsync("#entity-add-confirm");

        // Entity card should appear
        var card = await Page.WaitForSelectorAsync(".entity-card[data-entity='Product']");
        card.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddEntity_PressEnter_CreatesEntity()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Customer");
        await Page.PressAsync("#entity-add-input", "Enter");

        var card = await Page.WaitForSelectorAsync(".entity-card[data-entity='Customer']");
        card.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddEntity_CancelButton_HidesForm()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        var addBtn = await Page.QuerySelectorAsync("#btn-add-entity");
        var isHidden = await addBtn!.EvaluateAsync<bool>("el => el.classList.contains('hidden')");
        isHidden.ShouldBeTrue();

        await Page.ClickAsync("#entity-add-cancel");

        // Button should be visible again
        var isVisibleAgain = await addBtn.EvaluateAsync<bool>("el => !el.classList.contains('hidden')");
        isVisibleAgain.ShouldBeTrue();
    }

    [Fact]
    public async Task AddEntity_EmptyName_DoesNotCreate()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        await Page.ClickAsync("#entity-add-confirm");

        // No entity cards should exist
        var cards = await Page.QuerySelectorAllAsync(".entity-card[data-entity]");
        cards.Count.ShouldBe(0);
    }

    [Fact]
    public async Task AddEntity_UpdatesTabBadge()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        // Badge should be hidden initially
        var badge = await Page.QuerySelectorAsync("#badge-entities.hidden");
        badge.ShouldNotBeNull();

        // Add an entity
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Order");
        await Page.ClickAsync("#entity-add-confirm");

        // Badge should now show "1"
        var badgeVisible = await Page.QuerySelectorAsync("#badge-entities:not(.hidden)");
        badgeVisible.ShouldNotBeNull();
        var text = (await badgeVisible.TextContentAsync())!.Trim();
        text.ShouldBe("1");
    }

    [Fact]
    public async Task RemoveEntity_TwoClickConfirmation()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        // Add entity first
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "TempEntity");
        await Page.ClickAsync("#entity-add-confirm");

        // First click — shows "Sure?"
        await Page.ClickAsync(".entity-remove[data-entity='TempEntity']");
        var btnText = await Page.TextContentAsync(".entity-remove[data-entity='TempEntity']");
        btnText!.Trim().ShouldBe("Sure?");

        // Second click — actually removes
        await Page.ClickAsync(".entity-remove[data-entity='TempEntity']");
        var card = await Page.QuerySelectorAsync(".entity-card[data-entity='TempEntity']");
        card.ShouldBeNull();
    }

    [Fact]
    public async Task CloneEntity_CreatesACopy()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        // Add entity with a property
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Original");
        await Page.ClickAsync("#entity-add-confirm");

        // Clone it
        await Page.ClickAsync(".entity-clone[data-entity='Original']");

        // OriginalCopy should exist
        var copyCard = await Page.WaitForSelectorAsync(".entity-card[data-entity='OriginalCopy']");
        copyCard.ShouldNotBeNull();

        // Toast should appear
        await WaitForToastAsync("Cloned");
    }

    [Fact]
    public async Task AddProperty_InlineForm()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        // Add entity
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Item");
        await Page.ClickAsync("#entity-add-confirm");

        // Add property
        await Page.ClickAsync(".prop-add-btn[data-entity='Item']");
        await Page.FillAsync(".prop-add-input[data-entity='Item']", "title");
        await Page.ClickAsync(".prop-add-confirm[data-entity='Item']");

        // Property row should exist
        var propRow = await Page.WaitForSelectorAsync("tr[data-entity='Item'][data-prop='title']");
        propRow.ShouldNotBeNull();
    }

    [Fact]
    public async Task PresetButton_AddsProperties()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        // Add entity
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "User");
        await Page.ClickAsync("#entity-add-confirm");

        // Click "ID Field" preset
        await Page.ClickAsync(".preset-btn[data-preset='id'][data-entity='User']");

        // Should have an 'id' property with type Guid
        var idRow = await Page.WaitForSelectorAsync("tr[data-entity='User'][data-prop='id']");
        idRow.ShouldNotBeNull();

        await WaitForToastAsync("Added");
    }

    [Fact]
    public async Task PresetTimestamps_AddsTwoProperties()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Event");
        await Page.ClickAsync("#entity-add-confirm");

        await Page.ClickAsync(".preset-btn[data-preset='timestamps'][data-entity='Event']");

        var createdAt = await Page.QuerySelectorAsync("tr[data-entity='Event'][data-prop='createdAt']");
        var updatedAt = await Page.QuerySelectorAsync("tr[data-entity='Event'][data-prop='updatedAt']");

        createdAt.ShouldNotBeNull();
        updatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task RemoveProperty_ClickX()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        // Setup: entity with property
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Widget");
        await Page.ClickAsync("#entity-add-confirm");

        await Page.ClickAsync(".preset-btn[data-preset='name'][data-entity='Widget']");
        var nameRow = await Page.WaitForSelectorAsync("tr[data-entity='Widget'][data-prop='name']");
        nameRow.ShouldNotBeNull();

        // Remove it
        await Page.ClickAsync("tr[data-entity='Widget'][data-prop='name'] .prop-remove");

        var removedRow = await Page.QuerySelectorAsync("tr[data-entity='Widget'][data-prop='name']");
        removedRow.ShouldBeNull();
    }

    [Fact]
    public async Task AddRelationship_InlineForm()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Post");
        await Page.ClickAsync("#entity-add-confirm");

        // Add relationship
        await Page.ClickAsync(".rel-add-btn[data-entity='Post']");
        await Page.FillAsync(".rel-add-input[data-entity='Post']", "comments");
        await Page.ClickAsync(".rel-add-confirm[data-entity='Post']");

        var relRow = await Page.WaitForSelectorAsync("tr[data-entity='Post'][data-rel='comments']");
        relRow.ShouldNotBeNull();
    }

    [Fact]
    public async Task RemoveRelationship_ClickX()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Author");
        await Page.ClickAsync("#entity-add-confirm");

        await Page.ClickAsync(".rel-add-btn[data-entity='Author']");
        await Page.FillAsync(".rel-add-input[data-entity='Author']", "books");
        await Page.ClickAsync(".rel-add-confirm[data-entity='Author']");

        await Page.ClickAsync("tr[data-entity='Author'][data-rel='books'] .rel-remove");

        var removedRel = await Page.QuerySelectorAsync("tr[data-entity='Author'][data-rel='books']");
        removedRel.ShouldBeNull();
    }

    [Fact]
    public async Task EntityCard_ShowsPropertyCount()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Thing");
        await Page.ClickAsync("#entity-add-confirm");

        // Shows "0 props"
        var badge = await Page.TextContentAsync(".entity-card[data-entity='Thing'] .badge");
        badge!.Trim().ShouldBe("0 props");

        // Add a property
        await Page.ClickAsync(".preset-btn[data-preset='id'][data-entity='Thing']");

        badge = await Page.TextContentAsync(".entity-card[data-entity='Thing'] .badge");
        badge!.Trim().ShouldBe("1 prop");
    }

    [Fact]
    public async Task Entity_SaveAndReload_PersistsProperties()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Invoice");
        await Page.ClickAsync("#entity-add-confirm");

        await Page.ClickAsync(".preset-btn[data-preset='id'][data-entity='Invoice']");
        await Page.ClickAsync(".preset-btn[data-preset='name'][data-entity='Invoice']");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(saved);
        var entities = doc.RootElement.GetProperty("entities");
        entities.TryGetProperty("Invoice", out var invoice).ShouldBeTrue();
        var props = invoice.GetProperty("properties");
        props.TryGetProperty("id", out _).ShouldBeTrue();
        props.TryGetProperty("name", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task ChangePropertyType_PersistsCorrectly()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Metric");
        await Page.ClickAsync("#entity-add-confirm");

        await Page.ClickAsync(".prop-add-btn[data-entity='Metric']");
        await Page.FillAsync(".prop-add-input[data-entity='Metric']", "value");
        await Page.ClickAsync(".prop-add-confirm[data-entity='Metric']");

        // Change type from string to decimal
        await Page.SelectOptionAsync(
            "tr[data-entity='Metric'][data-prop='value'] .prop-field[data-key='type']",
            "decimal");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        using var doc = JsonDocument.Parse(saved!);
        doc.RootElement
            .GetProperty("entities")
            .GetProperty("Metric")
            .GetProperty("properties")
            .GetProperty("value")
            .GetProperty("type")
            .GetString()
            .ShouldBe("decimal");
    }
}
