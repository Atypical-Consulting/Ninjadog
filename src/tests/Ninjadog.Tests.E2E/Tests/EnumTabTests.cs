using System.Text.Json;
using Ninjadog.Tests.E2E.Fixtures;
using Shouldly;

namespace Ninjadog.Tests.E2E.Tests;

/// <summary>
/// Tests for the Enums tab — add/remove enums and values.
/// </summary>
public sealed class EnumTabTests : UiTestBase
{
    public EnumTabTests(NinjadogUiFixture server, PlaywrightFixture pw) : base(server, pw) { }

    [Fact]
    public async Task AddEnum_InlineForm()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "Status");
        await Page.ClickAsync("#enum-add-confirm");

        var card = await Page.WaitForSelectorAsync(".entity-card[data-enum='Status']");
        card.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddEnum_PressEnter()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "Priority");
        await Page.PressAsync("#enum-add-input", "Enter");

        var card = await Page.WaitForSelectorAsync(".entity-card[data-enum='Priority']");
        card.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddEnum_EmptyName_DoesNotCreate()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.ClickAsync("#enum-add-confirm");

        var cards = await Page.QuerySelectorAllAsync(".entity-card[data-enum]");
        cards.Count.ShouldBe(0);
    }

    [Fact]
    public async Task AddEnumValue_ClickAdd()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "Color");
        await Page.ClickAsync("#enum-add-confirm");

        // Add a value
        await Page.FillAsync(".enum-val-input[data-enum='Color']", "Red");
        await Page.ClickAsync(".enum-val-add[data-enum='Color']");

        // Value tag should appear
        var tag = await Page.WaitForSelectorAsync(".entity-card[data-enum='Color'] .enum-value-tag");
        tag.ShouldNotBeNull();
        var text = await tag.TextContentAsync();
        text.ShouldContain("Red");
    }

    [Fact]
    public async Task AddEnumValue_PressEnter()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "Size");
        await Page.ClickAsync("#enum-add-confirm");

        await Page.FillAsync(".enum-val-input[data-enum='Size']", "Small");
        await Page.PressAsync(".enum-val-input[data-enum='Size']", "Enter");

        var tag = await Page.WaitForSelectorAsync(".entity-card[data-enum='Size'] .enum-value-tag");
        tag.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddMultipleEnumValues()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "Direction");
        await Page.ClickAsync("#enum-add-confirm");

        var values = new[] { "North", "South", "East", "West" };
        foreach (var v in values)
        {
            await Page.FillAsync(".enum-val-input[data-enum='Direction']", v);
            await Page.ClickAsync(".enum-val-add[data-enum='Direction']");
        }

        var tags = await Page.QuerySelectorAllAsync(".entity-card[data-enum='Direction'] .enum-value-tag");
        tags.Count.ShouldBe(4);
    }

    [Fact]
    public async Task RemoveEnumValue_ClickX()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "Fruit");
        await Page.ClickAsync("#enum-add-confirm");

        await Page.FillAsync(".enum-val-input[data-enum='Fruit']", "Apple");
        await Page.ClickAsync(".enum-val-add[data-enum='Fruit']");

        await Page.FillAsync(".enum-val-input[data-enum='Fruit']", "Banana");
        await Page.ClickAsync(".enum-val-add[data-enum='Fruit']");

        // Remove "Apple" (index 0)
        await Page.ClickAsync(".enum-val-remove[data-enum='Fruit'][data-index='0']");

        var tags = await Page.QuerySelectorAllAsync(".entity-card[data-enum='Fruit'] .enum-value-tag");
        tags.Count.ShouldBe(1);

        var text = await tags[0].TextContentAsync();
        text.ShouldContain("Banana");
    }

    [Fact]
    public async Task RemoveEnum_TwoClickConfirmation()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "TempEnum");
        await Page.ClickAsync("#enum-add-confirm");

        // First click — shows "Sure?"
        await Page.ClickAsync(".enum-remove[data-enum='TempEnum']");
        var text = await Page.TextContentAsync(".enum-remove[data-enum='TempEnum']");
        text!.Trim().ShouldBe("Sure?");

        // Second click — removes
        await Page.ClickAsync(".enum-remove[data-enum='TempEnum']");
        var card = await Page.QuerySelectorAsync(".entity-card[data-enum='TempEnum']");
        card.ShouldBeNull();
    }

    [Fact]
    public async Task EnumValues_SaveAndPersist()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "OrderStatus");
        await Page.ClickAsync("#enum-add-confirm");

        var values = new[] { "Pending", "Processing", "Shipped" };
        foreach (var v in values)
        {
            await Page.FillAsync(".enum-val-input[data-enum='OrderStatus']", v);
            await Page.ClickAsync(".enum-val-add[data-enum='OrderStatus']");
        }

        await SaveAndWaitAsync();

        var saved = Server.ReadSavedConfig();
        saved.ShouldNotBeNull();
        using var doc = JsonDocument.Parse(saved);
        var enums = doc.RootElement.GetProperty("enums");
        var orderStatus = enums.GetProperty("OrderStatus");
        orderStatus.GetArrayLength().ShouldBe(3);
        orderStatus[0].GetString().ShouldBe("Pending");
        orderStatus[1].GetString().ShouldBe("Processing");
        orderStatus[2].GetString().ShouldBe("Shipped");
    }

    [Fact]
    public async Task EnumBadge_UpdatesOnAdd()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        // Badge hidden initially
        var badge = await Page.QuerySelectorAsync("#badge-enums.hidden");
        badge.ShouldNotBeNull();

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "Role");
        await Page.ClickAsync("#enum-add-confirm");

        var badgeVisible = await Page.QuerySelectorAsync("#badge-enums:not(.hidden)");
        badgeVisible.ShouldNotBeNull();
        (await badgeVisible.TextContentAsync())!.Trim().ShouldBe("1");
    }

    [Fact]
    public async Task EnumCard_ShowsValueCount()
    {
        await NavigateToUiAsync();
        await SwitchTabAsync("enums");

        await Page.ClickAsync("#btn-add-enum");
        await Page.FillAsync("#enum-add-input", "Season");
        await Page.ClickAsync("#enum-add-confirm");

        var countText = await Page.TextContentAsync(".entity-card[data-enum='Season'] .text-xs.text-gray-500");
        countText.ShouldContain("0 values");

        await Page.FillAsync(".enum-val-input[data-enum='Season']", "Spring");
        await Page.ClickAsync(".enum-val-add[data-enum='Season']");

        countText = await Page.TextContentAsync(".entity-card[data-enum='Season'] .text-xs.text-gray-500");
        countText.ShouldContain("1 values");
    }
}
