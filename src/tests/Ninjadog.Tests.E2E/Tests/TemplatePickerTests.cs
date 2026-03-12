using System.Text.Json;
using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for the template picker modal — selecting starter templates.
/// </summary>
public sealed class TemplatePickerTests : UiTestBase
{
    public TemplatePickerTests(NinjadogUiFixture server, PlaywrightFixture pw) : base(server, pw) { }

    [Fact]
    public async Task StartFromTemplate_OpensPickerModal()
    {
        await NavigateToUiAsync();

        await Page.ClickAsync("#btn-start-template");

        var picker = await Page.WaitForSelectorAsync("#template-picker:not(.hidden)");
        picker.ShouldNotBeNull();
    }

    [Fact]
    public async Task TemplatePicker_ShowsFourOptions()
    {
        await NavigateToUiAsync();
        await Page.ClickAsync("#btn-start-template");

        var cards = await Page.QuerySelectorAllAsync("#template-grid .template-card");
        cards.Count.ShouldBe(4);
    }

    [Fact]
    public async Task TemplatePicker_Cancel_ClosesPicker()
    {
        await NavigateToUiAsync();
        await Page.ClickAsync("#btn-start-template");
        await Page.WaitForSelectorAsync("#template-picker:not(.hidden)");

        await Page.ClickAsync("#template-picker-close");

        var picker = await Page.QuerySelectorAsync("#template-picker.hidden");
        picker.ShouldNotBeNull();
    }

    [Fact]
    public async Task TemplatePicker_SelectBlank_AppliesTemplate()
    {
        await NavigateToUiAsync();
        await Page.ClickAsync("#btn-start-template");

        await Page.ClickAsync("[data-template='blank']");

        await WaitForToastAsync("Template applied");

        // Picker should close
        var picker = await Page.QuerySelectorAsync("#template-picker.hidden");
        picker.ShouldNotBeNull();
    }

    [Fact]
    public async Task TemplatePicker_SelectTodo_CreatesEntities()
    {
        await NavigateToUiAsync();
        await Page.ClickAsync("#btn-start-template");

        await Page.ClickAsync("[data-template='todo']");

        await WaitForToastAsync("Template applied");

        // Should have TodoItem entity
        await SwitchTabAsync("entities");
        var card = await Page.WaitForSelectorAsync(".entity-card[data-entity='TodoItem']");
        card.ShouldNotBeNull();

        // Entity badge should show count
        var badge = await Page.QuerySelectorAsync("#badge-entities:not(.hidden)");
        badge.ShouldNotBeNull();
    }

    [Fact]
    public async Task TemplatePicker_SelectBlog_CreatesThreeEntities()
    {
        await NavigateToUiAsync();
        await Page.ClickAsync("#btn-start-template");

        await Page.ClickAsync("[data-template='blog']");
        await WaitForToastAsync("Template applied");

        await SwitchTabAsync("entities");

        // Blog template has Post, Comment, Tag
        var cards = await Page.QuerySelectorAllAsync(".entity-card[data-entity]");
        cards.Count.ShouldBe(3);
    }

    [Fact]
    public async Task TemplatePicker_SelectEcommerce_SetsConfigAndEntities()
    {
        await NavigateToUiAsync();
        await Page.ClickAsync("#btn-start-template");

        await Page.ClickAsync("[data-template='ecommerce']");
        await WaitForToastAsync("Template applied");

        // Config tab should have postgres selected
        await SwitchTabAsync("config");
        var provider = await Page.EvalOnSelectorAsync<string>(
            "[data-field='databaseProvider']", "el => el.value");
        provider.ShouldBe("postgres");

        // Soft delete and auditing should be checked
        var softDelete = await Page.EvalOnSelectorAsync<bool>(
            "[data-field='softDelete']", "el => el.checked");
        softDelete.ShouldBeTrue();

        // Should have 4 entities
        await SwitchTabAsync("entities");
        var badge = await Page.TextContentAsync("#badge-entities:not(.hidden)");
        badge!.Trim().ShouldBe("4");

        // Should have 1 enum
        var enumBadge = await Page.QuerySelectorAsync("#badge-enums:not(.hidden)");
        enumBadge.ShouldNotBeNull();
    }

    [Fact]
    public async Task TemplatePicker_ApplyTemplateAndSave_PersistsCorrectly()
    {
        await NavigateToUiAsync();
        await Page.ClickAsync("#btn-start-template");

        await Page.ClickAsync("[data-template='todo']");
        await WaitForToastAsync("Template applied");

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(saved);
        var root = doc.RootElement;

        root.GetProperty("config").GetProperty("name").GetString().ShouldBe("TodoApi");
        root.GetProperty("entities").TryGetProperty("TodoItem", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task StartFromScratch_NavigatesToEntitiesTab()
    {
        await NavigateToUiAsync();

        await Page.ClickAsync("#btn-start-scratch");

        // Entities tab should be active
        var entitiesActive = await Page.WaitForSelectorAsync("#tab-entities.tab-content-active");
        entitiesActive.ShouldNotBeNull();
    }
}
