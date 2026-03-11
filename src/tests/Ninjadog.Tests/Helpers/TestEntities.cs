using Ninjadog.Settings.Entities.Properties;
using Ninjadog.Settings.Extensions.Entities;
using Ninjadog.Settings.Extensions.Entities.Properties;

namespace Ninjadog.Tests.Helpers;

public static class TestEntities
{
    public static NinjadogEntityWithKey CreateGuidKeyEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityId() },
            { "Title", new NinjadogEntityProperty<string>() },
            { "Description", new NinjadogEntityProperty<string>() },
            { "IsCompleted", new NinjadogEntityProperty<bool>() },
            { "DueDate", new NinjadogEntityProperty<DateTime>() },
            { "Priority", new NinjadogEntityProperty<int>() },
            { "Cost", new NinjadogEntityProperty<decimal>() },
        };

        return new NinjadogEntityWithKey("TodoItem", properties, null);
    }

    public static NinjadogEntityWithKey CreateIntKeyEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "OrderId", new NinjadogEntityProperty("Int32", true) },
            { "CustomerName", new NinjadogEntityProperty<string>() },
            { "Total", new NinjadogEntityProperty<decimal>() },
        };

        return new NinjadogEntityWithKey("Order", properties, null);
    }

    public static NinjadogEntityWithKey CreateStringOnlyEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityId() },
            { "Name", new NinjadogEntityProperty<string>() },
            { "Color", new NinjadogEntityProperty<string>() },
        };

        return new NinjadogEntityWithKey("Category", properties, null);
    }

    public static NinjadogEntityWithKey CreateValidatedEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityId() },
            { "Name", new NinjadogEntityProperty("String", Required: true, MaxLength: 100, MinLength: 2) },
            { "Email", new NinjadogEntityProperty("String", Required: true, Pattern: @"^[^@]+@[^@]+\.[^@]+$") },
            { "Age", new NinjadogEntityProperty("Int32", Min: 0, Max: 150) },
            { "Bio", new NinjadogEntityProperty("String", MaxLength: 500) },
        };

        return new NinjadogEntityWithKey("Contact", properties, null);
    }

    public static NinjadogEntityWithKey CreateSeededEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityId() },
            { "Name", new NinjadogEntityProperty<string>() },
            { "IsActive", new NinjadogEntityProperty<bool>() },
        };

        var seedData = new List<Dictionary<string, object>>
        {
            new() { ["Id"] = "550e8400-e29b-41d4-a716-446655440001", ["Name"] = "Default Category", ["IsActive"] = true },
            new() { ["Id"] = "550e8400-e29b-41d4-a716-446655440002", ["Name"] = "Archive", ["IsActive"] = false },
        };

        return new NinjadogEntityWithKey("Category", properties, null, seedData);
    }
}
