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

public sealed record SoftDeleteSettings()
    : NinjadogSettings(new TestConfiguration() with { SoftDelete = true }, new TestEntitiesCollection());

public sealed record AuditSettings()
    : NinjadogSettings(new TestConfiguration() with { Auditing = true }, new TestEntitiesCollection());

public sealed record PostgresSettings()
    : NinjadogSettings(new TestConfiguration() with { DatabaseProvider = "postgresql" }, new TestEntitiesCollection());

public sealed record SqlServerSettings()
    : NinjadogSettings(new TestConfiguration() with { DatabaseProvider = "sqlserver" }, new TestEntitiesCollection());

public sealed class RelationshipEntitiesCollection : NinjadogEntities
{
    public RelationshipEntitiesCollection()
    {
        var parent = TestEntities.CreateParentEntity();
        var child = TestEntities.CreateChildEntity();
        Add(parent.Key, new(parent.Properties, parent.Relationships));
        Add(child.Key, new(child.Properties));
    }
}

public sealed record RelationshipSettings()
    : NinjadogSettings(new TestConfiguration(), new RelationshipEntitiesCollection());

public sealed record AotSettings()
    : NinjadogSettings(new TestConfiguration() with { Aot = true }, new TestEntitiesCollection());

public sealed class AotSeededEntitiesCollection : NinjadogEntities
{
    public AotSeededEntitiesCollection()
    {
        var guidEntity = TestEntities.CreateGuidKeyEntity();
        var seededEntity = TestEntities.CreateSeededEntity();
        Add(guidEntity.Key, new(guidEntity.Properties));
        Add(seededEntity.Key, new(seededEntity.Properties, null, seededEntity.SeedData));
    }
}

public sealed record AotSeededSettings()
    : NinjadogSettings(new TestConfiguration() with { Aot = true }, new AotSeededEntitiesCollection());

public sealed record RateLimitSettings()
    : NinjadogSettings(new TestConfiguration() with { RateLimit = new NinjadogRateLimitConfiguration() }, new TestEntitiesCollection());

public sealed record CustomRateLimitSettings()
    : NinjadogSettings(
        new TestConfiguration() with { RateLimit = new NinjadogRateLimitConfiguration(PermitLimit: 50, WindowSeconds: 30, SegmentsPerWindow: 3) },
        new TestEntitiesCollection());

public sealed record VersioningSettings()
    : NinjadogSettings(new TestConfiguration() with { Versioning = new NinjadogVersioningConfiguration() }, new TestEntitiesCollection());

public sealed record HeaderVersioningSettings()
    : NinjadogSettings(new TestConfiguration() with { Versioning = new NinjadogVersioningConfiguration(Strategy: "HeaderBased") }, new TestEntitiesCollection());

public sealed record AuthSettings()
    : NinjadogSettings(new TestConfiguration() with { Auth = new NinjadogAuthConfiguration(Roles: ["Admin", "User"]) }, new TestEntitiesCollection());
