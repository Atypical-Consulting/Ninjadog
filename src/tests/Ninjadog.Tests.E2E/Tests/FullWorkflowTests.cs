using System.Text.Json;
using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// End-to-end workflow tests — full user journeys across multiple tabs.
/// </summary>
public sealed class FullWorkflowTests : UiTestBase
{
    public FullWorkflowTests(NinjadogUiFixture server, PlaywrightFixture pw) : base(server, pw) { }

    [Fact]
    public async Task CreateProjectFromScratch_ConfigureAndSave()
    {
        await NavigateToUiAsync();

        // 1. Fill config
        await FillFieldAsync("name", "MyApi");
        await FillFieldAsync("version", "1.0.0");
        await FillFieldAsync("rootNamespace", "MyCompany.Api");
        await Page.CheckAsync("[data-field='softDelete']");

        // 2. Add entity
        await SwitchTabAsync("entities");
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Product");
        await Page.ClickAsync("#entity-add-confirm");

        // Add properties via presets
        await Page.ClickAsync(".preset-btn[data-preset='id'][data-entity='Product']");
        await Page.ClickAsync(".preset-btn[data-preset='name'][data-entity='Product']");

        // Add custom property
        await Page.ClickAsync(".prop-add-btn[data-entity='Product']");
        await Page.FillAsync(".prop-add-input[data-entity='Product']", "price");
        await Page.ClickAsync(".prop-add-confirm[data-entity='Product']");

        // Change price type to decimal
        await Page.SelectOptionAsync(
            "tr[data-entity='Product'][data-prop='price'] .prop-field[data-key='type']",
            "decimal");

        // 3. Add enum
        await SwitchTabAsync("enums");
        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "ProductCategory");
        await Page.ClickAsync("#enum-add-confirm");

        await Page.FillAsync(".enum-val-input[data-enum='ProductCategory']", "Electronics");
        await Page.ClickAsync(".enum-val-add[data-enum='ProductCategory']");
        await Page.FillAsync(".enum-val-input[data-enum='ProductCategory']", "Clothing");
        await Page.ClickAsync(".enum-val-add[data-enum='ProductCategory']");

        // 4. Add seed data
        await SwitchTabAsync("seed");
        await Page.ClickAsync(".seed-add-row[data-entity='Product']");

        var nameInput = await Page.QuerySelectorAsync(
            "tr[data-entity='Product'][data-row='0'] .seed-field[data-prop='name']");
        await nameInput!.FocusAsync();
        await Page.Keyboard.PressAsync("Control+a");
        await nameInput.TypeAsync("Sample Product");

        // 5. Save
        await SaveAndWaitAsync();

        // 6. Verify saved file
        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(saved);
        var root = doc.RootElement;

        root.GetProperty("config").GetProperty("name").GetString().ShouldBe("MyApi");
        root.GetProperty("config").GetProperty("features").GetProperty("softDelete").GetBoolean().ShouldBeTrue();
        root.GetProperty("entities").GetProperty("Product").GetProperty("properties").GetProperty("price")
            .GetProperty("type").GetString().ShouldBe("decimal");
        root.GetProperty("enums").GetProperty("ProductCategory").GetArrayLength().ShouldBe(2);
        root.GetProperty("entities").GetProperty("Product").GetProperty("seedData")[0]
            .GetProperty("name").GetString().ShouldBe("Sample Product");
    }

    [Fact]
    public async Task LoadExistingConfig_EditAndSave()
    {
        Server.SeedConfigJson("""
        {
            "config": {
                "name": "ExistingApi",
                "version": "1.0.0",
                "rootNamespace": "Existing.Api"
            },
            "entities": {
                "User": {
                    "properties": {
                        "id": { "type": "Guid", "isKey": true },
                        "email": { "type": "string", "required": true }
                    }
                }
            }
        }
        """);

        await NavigateToUiAsync();

        // Verify config loaded
        var name = await Page.EvalOnSelectorAsync<string>("[data-field='name']", "el => el.value");
        name.ShouldBe("ExistingApi");

        // Verify entity loaded
        await SwitchTabAsync("entities");
        var userCard = await Page.WaitForSelectorAsync(".entity-card[data-entity='User']");
        userCard.ShouldNotBeNull();

        // Verify properties loaded
        var emailRow = await Page.QuerySelectorAsync("tr[data-entity='User'][data-prop='email']");
        emailRow.ShouldNotBeNull();

        // Modify the name
        await SwitchTabAsync("config");
        await FillFieldAsync("name", "ModifiedApi");

        // Add a new entity
        await SwitchTabAsync("entities");
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Order");
        await Page.ClickAsync("#entity-add-confirm");

        // Save
        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        using var doc = JsonDocument.Parse(saved!);
        doc.RootElement.GetProperty("config").GetProperty("name").GetString().ShouldBe("ModifiedApi");
        doc.RootElement.GetProperty("entities").TryGetProperty("User", out _).ShouldBeTrue();
        doc.RootElement.GetProperty("entities").TryGetProperty("Order", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task TemplateToCustomize_FullJourney()
    {
        await NavigateToUiAsync();

        // Start from blog template
        await Page.ClickAsync("#btn-start-template");
        await Page.ClickAsync("[data-template='blog']");
        await WaitForToastAsync("Template applied");

        // Customize: change name
        await SwitchTabAsync("config");
        await FillFieldAsync("name", "CustomBlog");

        // Add a property to Post entity
        await SwitchTabAsync("entities");
        await Page.ClickAsync(".prop-add-btn[data-entity='Post']");
        await Page.FillAsync(".prop-add-input[data-entity='Post']", "readTime");
        await Page.ClickAsync(".prop-add-confirm[data-entity='Post']");

        // Change type to int
        await Page.SelectOptionAsync(
            "tr[data-entity='Post'][data-prop='readTime'] .prop-field[data-key='type']",
            "int");

        // Save and verify
        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        using var doc = JsonDocument.Parse(saved!);
        doc.RootElement.GetProperty("config").GetProperty("name").GetString().ShouldBe("CustomBlog");

        var readTime = doc.RootElement
            .GetProperty("entities")
            .GetProperty("Post")
            .GetProperty("properties")
            .GetProperty("readTime");
        readTime.GetProperty("type").GetString().ShouldBe("int");
    }

    [Fact]
    public async Task EntityWithRelationship_PersistsCorrectly()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("entities");

        // Create Parent
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Author");
        await Page.ClickAsync("#entity-add-confirm");
        await Page.ClickAsync(".preset-btn[data-preset='id'][data-entity='Author']");
        await Page.ClickAsync(".preset-btn[data-preset='name'][data-entity='Author']");

        // Create Child
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "Book");
        await Page.ClickAsync("#entity-add-confirm");
        await Page.ClickAsync(".preset-btn[data-preset='id'][data-entity='Book']");

        // Add relationship on Author: hasMany books
        await Page.ClickAsync(".rel-add-btn[data-entity='Author']");
        await Page.FillAsync(".rel-add-input[data-entity='Author']", "books");
        await Page.ClickAsync(".rel-add-confirm[data-entity='Author']");

        // Set target entity and foreign key
        await Page.FillAsync("tr[data-entity='Author'][data-rel='books'] .rel-field[data-key='targetEntity']", "Book");
        await Page.FillAsync("tr[data-entity='Author'][data-rel='books'] .rel-field[data-key='foreignKey']", "authorId");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        using var doc = JsonDocument.Parse(saved!);
        var rels = doc.RootElement
            .GetProperty("entities")
            .GetProperty("Author")
            .GetProperty("relationships")
            .GetProperty("books");

        rels.GetProperty("type").GetString().ShouldBe("hasMany");
        rels.GetProperty("targetEntity").GetString().ShouldBe("Book");
        rels.GetProperty("foreignKey").GetString().ShouldBe("authorId");
    }

    [Fact]
    public async Task DirtyIndicator_TracksChangesAcrossTabs()
    {
        await NavigateToUiAsync();

        // Start clean
        (await IsDirtyAsync()).ShouldBeFalse();

        // Make change on config tab
        await FillFieldAsync("name", "DirtyTracker");
        (await IsDirtyAsync()).ShouldBeTrue();

        // Switch to entities tab — dirty should persist
        await SwitchTabAsync("entities");
        (await IsDirtyAsync()).ShouldBeTrue();

        // Save
        await SaveAndWaitAsync();
        (await IsDirtyAsync()).ShouldBeFalse();

        // Switch back and make another change
        await SwitchTabAsync("config");
        await FillFieldAsync("version", "2.0.0");
        (await IsDirtyAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task TabBadges_ReflectState()
    {
        await NavigateToUiAsync();

        // Add 2 entities
        await SwitchTabAsync("entities");
        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "A");
        await Page.ClickAsync("#entity-add-confirm");

        await Page.ClickAsync("#btn-add-entity");
        await Page.FillAsync("#entity-add-input", "B");
        await Page.ClickAsync("#entity-add-confirm");

        // Entity badge should show 2
        var entityBadge = await Page.TextContentAsync("#badge-entities:not(.hidden)");
        entityBadge!.Trim().ShouldBe("2");

        // Add an enum
        await SwitchTabAsync("enums");
        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "MyEnum");
        await Page.ClickAsync("#enum-add-confirm");

        var enumBadge = await Page.TextContentAsync("#badge-enums:not(.hidden)");
        enumBadge!.Trim().ShouldBe("1");
    }
}
