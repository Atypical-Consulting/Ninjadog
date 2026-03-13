using Ninjadog.Settings;
using Ninjadog.Settings.Config;
using Ninjadog.Settings.Entities;

namespace Ninjadog.Tests.Helpers;

/// <summary>
/// Factory methods for creating test settings with specific feature flag combinations.
/// Use these instead of one-off record types for simple configuration variations.
/// </summary>
public static class TestSettingsFactory
{
    public static ConfiguredSettings WithSoftDelete()
    {
        return new ConfiguredSettings(new TestConfiguration() with { SoftDelete = true }, new TestEntitiesCollection());
    }

    public static ConfiguredSettings WithAuditing()
    {
        return new ConfiguredSettings(new TestConfiguration() with { Auditing = true }, new TestEntitiesCollection());
    }

    public static ConfiguredSettings WithPostgres()
    {
        return new ConfiguredSettings(new TestConfiguration() with { DatabaseProvider = "postgresql" }, new TestEntitiesCollection());
    }

    public static ConfiguredSettings WithSqlServer()
    {
        return new ConfiguredSettings(new TestConfiguration() with { DatabaseProvider = "sqlserver" }, new TestEntitiesCollection());
    }

    public static ConfiguredSettings WithRelationships()
    {
        return new ConfiguredSettings(new TestConfiguration(), new RelationshipEntitiesCollection());
    }

    public static ConfiguredSettings WithAot()
    {
        return new ConfiguredSettings(new TestConfiguration() with { Aot = true }, new TestEntitiesCollection());
    }

    public static ConfiguredSettings WithAotSeeded()
    {
        return new ConfiguredSettings(new TestConfiguration() with { Aot = true }, new AotSeededEntitiesCollection());
    }

    public static ConfiguredSettings WithRateLimit()
    {
        return new ConfiguredSettings(
            new TestConfiguration() with { RateLimit = new NinjadogRateLimitConfiguration() },
            new TestEntitiesCollection());
    }

    public static ConfiguredSettings WithCustomRateLimit(int permitLimit = 50, int windowSeconds = 30, int segmentsPerWindow = 3)
    {
        return new ConfiguredSettings(
            new TestConfiguration() with { RateLimit = new NinjadogRateLimitConfiguration(PermitLimit: permitLimit, WindowSeconds: windowSeconds, SegmentsPerWindow: segmentsPerWindow) },
            new TestEntitiesCollection());
    }

    public static ConfiguredSettings WithVersioning()
    {
        return new ConfiguredSettings(
            new TestConfiguration() with { Versioning = new NinjadogVersioningConfiguration() },
            new TestEntitiesCollection());
    }

    public static ConfiguredSettings WithHeaderVersioning()
    {
        return new ConfiguredSettings(
            new TestConfiguration() with { Versioning = new NinjadogVersioningConfiguration(Strategy: "HeaderBased") },
            new TestEntitiesCollection());
    }

    public static ConfiguredSettings WithAuth(params string[] roles)
    {
        return new ConfiguredSettings(
            new TestConfiguration() with { Auth = new NinjadogAuthConfiguration(Roles: roles) },
            new TestEntitiesCollection());
    }
}

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

/// <summary>
/// Concrete NinjadogSettings for tests with a specific configuration and entity set.
/// </summary>
public sealed record ConfiguredSettings(NinjadogConfiguration Config, NinjadogEntities Entities, Dictionary<string, List<string>>? Enums = null)
    : NinjadogSettings(Config, Entities, Enums);

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
