using Ninjadog.Settings;
using Ninjadog.Settings.Config;
using Ninjadog.Settings.Entities;

namespace Ninjadog.Tests.Helpers;

public sealed record TestConfiguration()
    : NinjadogConfiguration(
        Name: "TestApp",
        Version: "1.0.0",
        Description: "Test application",
        RootNamespace: "TestApp.Api",
        OutputPath: "output",
        SaveGeneratedFiles: false);

public sealed class TestEntitiesCollection : NinjadogEntities
{
    public TestEntitiesCollection()
    {
        var guidEntity = TestEntities.CreateGuidKeyEntity();
        Add(guidEntity.Key, new(guidEntity.Properties));
    }
}

public sealed record TestSettings()
    : NinjadogSettings(new TestConfiguration(), new TestEntitiesCollection());

public sealed record EnumTestSettings()
    : NinjadogSettings(
        new TestConfiguration(),
        new TestEntitiesCollection(),
        new Dictionary<string, List<string>>
        {
            ["Priority"] = ["Low", "Medium", "High", "Critical"],
            ["Status"] = ["Draft", "Active", "Archived"],
        });

public sealed class SeededEntitiesCollection : NinjadogEntities
{
    public SeededEntitiesCollection()
    {
        var entity = TestEntities.CreateSeededEntity();
        Add(entity.Key, new(entity.Properties, null, entity.SeedData));
    }
}

public sealed record SeededSettings()
    : NinjadogSettings(new TestConfiguration(), new SeededEntitiesCollection());
