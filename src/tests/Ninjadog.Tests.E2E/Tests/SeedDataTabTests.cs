using System.Text.Json;
using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for the Seed Data tab — rows, auto-keys, import, validation.
/// </summary>
public sealed class SeedDataTabTests : UiTestBase
{
    public SeedDataTabTests(NinjadogUiFixture server, PlaywrightFixture pw) : base(server, pw) { }

    private async Task SetupEntityWithPropertiesAsync()
    {
        await SwitchTabAsync("entities");

        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Product");
        await Page.ClickAsync("#entity-add-confirm");

        await Page.ClickAsync(".preset-btn[data-preset='id'][data-entity='Product']");
        await Page.ClickAsync(".preset-btn[data-preset='name'][data-entity='Product']");
    }

    [Fact]
    public async Task SeedTab_NoEntities_ShowsMessage()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("seed");

        var text = await Page.TextContentAsync("#tab-seed");
        text.ShouldContain("No entities defined");
    }

    [Fact]
    public async Task SeedTab_EntityPresent_ShowsCard()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        var card = await Page.WaitForSelectorAsync("[data-seed-entity='Product']");
        card.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddRow_CreatesRowWithAutoKey()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        await Page.ClickAsync(".seed-add-row[data-entity='Product']");

        // Row should exist
        var row = await Page.WaitForSelectorAsync("tr[data-entity='Product'][data-row='0']");
        row.ShouldNotBeNull();

        // The "id" field should have an auto-generated GUID value
        var idInput = await Page.QuerySelectorAsync(
            "tr[data-entity='Product'][data-row='0'] .seed-field[data-prop='id']");
        idInput.ShouldNotBeNull();
        var idValue = await idInput.EvaluateAsync<string>("el => el.value");
        idValue.ShouldNotBeNullOrEmpty();
        // Should look like a GUID
        idValue.Length.ShouldBe(36);
        idValue.ShouldContain("-");
    }

    [Fact]
    public async Task AddMultipleRows_CountUpdates()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        await Page.ClickAsync(".seed-add-row[data-entity='Product']");
        await Page.ClickAsync(".seed-add-row[data-entity='Product']");
        await Page.ClickAsync(".seed-add-row[data-entity='Product']");

        var rows = await Page.QuerySelectorAllAsync("tr[data-entity='Product'][data-row]");
        rows.Count.ShouldBe(3);

        // Row count shown in header
        var countText = await Page.TextContentAsync("[data-seed-entity='Product'] .text-xs.text-gray-500");
        countText.ShouldContain("3 rows");
    }

    [Fact]
    public async Task RemoveRow_ClickX()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        await Page.ClickAsync(".seed-add-row[data-entity='Product']");
        await Page.ClickAsync(".seed-add-row[data-entity='Product']");

        // Remove first row
        await Page.ClickAsync(".seed-remove-row[data-entity='Product'][data-row='0']");

        var rows = await Page.QuerySelectorAllAsync("tr[data-entity='Product'][data-row]");
        rows.Count.ShouldBe(1);
    }

    [Fact]
    public async Task EditSeedField_PersistsOnSave()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        await Page.ClickAsync(".seed-add-row[data-entity='Product']");

        // Edit the name field
        var nameInput = await Page.QuerySelectorAsync(
            "tr[data-entity='Product'][data-row='0'] .seed-field[data-prop='name']");
        await nameInput!.FocusAsync();
        await Page.Keyboard.PressAsync("Control+a");
        await nameInput.TypeAsync("Test Widget");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(saved);
        var seedData = doc.RootElement
            .GetProperty("entities")
            .GetProperty("Product")
            .GetProperty("seedData");
        seedData[0].GetProperty("name").GetString().ShouldBe("Test Widget");
    }

    [Fact]
    public async Task SeedBadge_UpdatesOnAddRow()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        await Page.ClickAsync(".seed-add-row[data-entity='Product']");

        // Switch back to check badge
        var badge = await Page.QuerySelectorAsync("#badge-seed:not(.hidden)");
        badge.ShouldNotBeNull();
        (await badge.TextContentAsync())!.Trim().ShouldBe("1");
    }

    [Fact]
    public async Task ImportButton_OpensModal()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        await Page.ClickAsync(".seed-import[data-entity='Product']");

        var modal = await Page.WaitForSelectorAsync("#import-modal:not(.hidden)");
        modal.ShouldNotBeNull();

        var title = await Page.TextContentAsync(".import-modal-title");
        title.ShouldContain("Product");
    }

    [Fact]
    public async Task ImportModal_CancelCloses()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        await Page.ClickAsync(".seed-import[data-entity='Product']");
        await Page.WaitForSelectorAsync("#import-modal:not(.hidden)");

        await Page.ClickAsync("#import-cancel");

        var modal = await Page.QuerySelectorAsync("#import-modal.hidden");
        modal.ShouldNotBeNull();
    }

    [Fact]
    public async Task ImportModal_JsonImport()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        await Page.ClickAsync(".seed-import[data-entity='Product']");
        await Page.WaitForSelectorAsync("#import-modal:not(.hidden)");

        var jsonData = """[{"id":"00000000-0000-0000-0000-000000000001","name":"Widget A"}]""";
        await Page.FillAsync("#import-textarea", jsonData);
        await Page.ClickAsync("#import-confirm");

        // Should have a row
        var row = await Page.WaitForSelectorAsync("tr[data-entity='Product'][data-row='0']");
        row.ShouldNotBeNull();
    }

    [Fact]
    public async Task ImportModal_CsvImport()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        await Page.ClickAsync(".seed-import[data-entity='Product']");
        await Page.WaitForSelectorAsync("#import-modal:not(.hidden)");

        var csvData = "id,name\n00000000-0000-0000-0000-000000000001,Widget A\n00000000-0000-0000-0000-000000000002,Widget B";
        await Page.FillAsync("#import-textarea", csvData);
        await Page.ClickAsync("#import-confirm");

        var rows = await Page.QuerySelectorAllAsync("tr[data-entity='Product'][data-row]");
        rows.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ImportModal_InvalidData_ShowsError()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        await Page.ClickAsync(".seed-import[data-entity='Product']");
        await Page.WaitForSelectorAsync("#import-modal:not(.hidden)");

        // Single line is not valid CSV (needs header + data)
        await Page.FillAsync("#import-textarea", "just a single line");
        await Page.ClickAsync("#import-confirm");

        await WaitForToastAsync("Could not parse");
    }

    [Fact]
    public async Task GenerateKeyButton_FillsEmptyKey()
    {
        await NavigateToUiAsync();
        await SetupEntityWithPropertiesAsync();
        await SwitchTabAsync("seed");

        // Import a row with empty id
        await Page.ClickAsync(".seed-import[data-entity='Product']");
        await Page.WaitForSelectorAsync("#import-modal:not(.hidden)");
        await Page.FillAsync("#import-textarea", """[{"id":"","name":"No Key"}]""");
        await Page.ClickAsync("#import-confirm");

        // Click the generate key button
        var genBtn = await Page.WaitForSelectorAsync(".seed-gen-key[data-entity='Product'][data-row='0']");
        genBtn.ShouldNotBeNull();
        await genBtn.ClickAsync();

        // Key should now be filled
        var idInput = await Page.QuerySelectorAsync(
            "tr[data-entity='Product'][data-row='0'] .seed-field[data-prop='id']");
        var idValue = await idInput!.EvaluateAsync<string>("el => el.value");
        idValue.ShouldNotBeNullOrEmpty();
        idValue.Length.ShouldBe(36); // GUID format
    }
}
