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
}
