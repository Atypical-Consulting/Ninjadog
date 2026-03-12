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

public sealed record SoftDeleteConfiguration()
    : NinjadogConfiguration(
        Name: "TestApp",
        Version: "1.0.0",
        Description: "Test application",
        RootNamespace: "TestApp.Api",
        OutputPath: "output",
        SaveGeneratedFiles: false,
        SoftDelete: true);

public sealed record SoftDeleteSettings()
    : NinjadogSettings(new SoftDeleteConfiguration(), new TestEntitiesCollection());

public sealed record AuditConfiguration()
    : NinjadogConfiguration(
        Name: "TestApp",
        Version: "1.0.0",
        Description: "Test application",
        RootNamespace: "TestApp.Api",
        OutputPath: "output",
        SaveGeneratedFiles: false,
        Auditing: true);

public sealed record AuditSettings()
    : NinjadogSettings(new AuditConfiguration(), new TestEntitiesCollection());

public sealed record PostgresConfiguration()
    : NinjadogConfiguration(
        Name: "TestApp",
        Version: "1.0.0",
        Description: "Test application",
        RootNamespace: "TestApp.Api",
        OutputPath: "output",
        SaveGeneratedFiles: false,
        DatabaseProvider: "postgresql");

public sealed record PostgresSettings()
    : NinjadogSettings(new PostgresConfiguration(), new TestEntitiesCollection());

public sealed record SqlServerConfiguration()
    : NinjadogConfiguration(
        Name: "TestApp",
        Version: "1.0.0",
        Description: "Test application",
        RootNamespace: "TestApp.Api",
        OutputPath: "output",
        SaveGeneratedFiles: false,
        DatabaseProvider: "sqlserver");

public sealed record SqlServerSettings()
    : NinjadogSettings(new SqlServerConfiguration(), new TestEntitiesCollection());

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

public sealed record AotConfiguration()
    : NinjadogConfiguration(
        Name: "TestApp",
        Version: "1.0.0",
        Description: "Test application",
        RootNamespace: "TestApp.Api",
        OutputPath: "output",
        SaveGeneratedFiles: false,
        Aot: true);

public sealed record AotSettings()
    : NinjadogSettings(new AotConfiguration(), new TestEntitiesCollection());

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
    : NinjadogSettings(new AotConfiguration(), new AotSeededEntitiesCollection());

public sealed record RateLimitConfiguration()
    : NinjadogConfiguration(
        Name: "TestApp",
        Version: "1.0.0",
        Description: "Test application",
        RootNamespace: "TestApp.Api",
        OutputPath: "output",
        SaveGeneratedFiles: false,
        RateLimit: new NinjadogRateLimitConfiguration());

public sealed record RateLimitSettings()
    : NinjadogSettings(new RateLimitConfiguration(), new TestEntitiesCollection());

public sealed record CustomRateLimitConfiguration()
    : NinjadogConfiguration(
        Name: "TestApp",
        Version: "1.0.0",
        Description: "Test application",
        RootNamespace: "TestApp.Api",
        OutputPath: "output",
        SaveGeneratedFiles: false,
        RateLimit: new NinjadogRateLimitConfiguration(
            PermitLimit: 50,
            WindowSeconds: 30,
            SegmentsPerWindow: 3));

public sealed record CustomRateLimitSettings()
    : NinjadogSettings(new CustomRateLimitConfiguration(), new TestEntitiesCollection());

public sealed record AuthConfiguration()
    : NinjadogConfiguration(
        Name: "TestApp",
        Version: "1.0.0",
        Description: "Test application",
        RootNamespace: "TestApp.Api",
        OutputPath: "output",
        SaveGeneratedFiles: false,
        Auth: new NinjadogAuthConfiguration(Roles: ["Admin", "User"]));

public sealed record AuthSettings()
    : NinjadogSettings(new AuthConfiguration(), new TestEntitiesCollection());
