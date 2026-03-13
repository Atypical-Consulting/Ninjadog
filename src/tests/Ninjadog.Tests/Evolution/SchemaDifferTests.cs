using Ninjadog.Evolution;
using Ninjadog.Settings.Entities;
using Ninjadog.Settings.Entities.Properties;
using Ninjadog.Tests.Helpers;
using Shouldly;

namespace Ninjadog.Tests.Evolution;

public class SchemaDifferTests
{
    [Fact]
    public void Compare_IdenticalSettings_ReturnsNoChanges()
    {
        var settings = new TestSettings();

        var diff = SchemaDiffer.Compare(settings, settings);

        diff.HasChanges.ShouldBeFalse();
        diff.EntityChanges.ShouldBeEmpty();
        diff.ConfigChanges.HasChanges.ShouldBeFalse();
        diff.EnumChanges.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_NewEntityAdded_DetectsAddition()
    {
        var previous = new TestSettings();
        var currentEntities = new TestEntitiesWithExtra();
        var current = new ConfiguredSettings(new TestConfiguration(), currentEntities);

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.AddedEntities.Count().ShouldBe(1);

        var added = diff.AddedEntities.First();
        added.EntityKey.ShouldBe("Category");
        added.Kind.ShouldBe(ChangeKind.Added);
        added.PropertyChanges.ShouldAllBe(p => p.Kind == ChangeKind.Added);
    }

    [Fact]
    public void Compare_EntityRemoved_DetectsRemoval()
    {
        var previous = new ConfiguredSettings(new TestConfiguration(), new TwoEntitiesCollection());
        var current = new ConfiguredSettings(new TestConfiguration(), new TestEntitiesCollection());

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.RemovedEntities.Count().ShouldBe(1);

        var removed = diff.RemovedEntities.First();
        removed.EntityKey.ShouldBe("Category");
        removed.Kind.ShouldBe(ChangeKind.Removed);
    }

    [Fact]
    public void Compare_PropertyAdded_DetectsAddition()
    {
        var prevEntities = new SinglePropertyEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
        });
        var currEntities = new SinglePropertyEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
            { "Priority", new NinjadogEntityProperty("Int32") },
        });

        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);
        var current = new ConfiguredSettings(new TestConfiguration(), currEntities);

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.ModifiedEntities.Count().ShouldBe(1);

        var entity = diff.ModifiedEntities.First();
        entity.AddedProperties.Count().ShouldBe(1);
        entity.AddedProperties.First().PropertyName.ShouldBe("Priority");
        entity.AddedProperties.First().After!.Type.ShouldBe("Int32");
    }

    [Fact]
    public void Compare_PropertyRemoved_DetectsRemoval()
    {
        var prevEntities = new SinglePropertyEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
            { "Priority", new NinjadogEntityProperty("Int32") },
        });
        var currEntities = new SinglePropertyEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
        });

        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);
        var current = new ConfiguredSettings(new TestConfiguration(), currEntities);

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        var entity = diff.ModifiedEntities.First();
        entity.RemovedProperties.Count().ShouldBe(1);
        entity.RemovedProperties.First().PropertyName.ShouldBe("Priority");
    }

    [Fact]
    public void Compare_PropertyTypeChanged_DetectsModification()
    {
        var prevEntities = new SinglePropertyEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Priority", new NinjadogEntityProperty("String") },
        });
        var currEntities = new SinglePropertyEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Priority", new NinjadogEntityProperty("Int32") },
        });

        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);
        var current = new ConfiguredSettings(new TestConfiguration(), currEntities);

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        var entity = diff.ModifiedEntities.First();
        entity.HasTypeChanges.ShouldBeTrue();

        var propDiff = entity.ModifiedProperties.First();
        propDiff.TypeChanged.ShouldBeTrue();
        propDiff.Before!.Type.ShouldBe("String");
        propDiff.After!.Type.ShouldBe("Int32");
    }

    [Fact]
    public void Compare_PropertyConstraintsChanged_DetectsModification()
    {
        var prevEntities = new SinglePropertyEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String", MaxLength: 100) },
        });
        var currEntities = new SinglePropertyEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String", MaxLength: 500, Required: true) },
        });

        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);
        var current = new ConfiguredSettings(new TestConfiguration(), currEntities);

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        var propDiff = diff.ModifiedEntities.First().ModifiedProperties.First();
        propDiff.OnlyConstraintsChanged.ShouldBeTrue();
        propDiff.TypeChanged.ShouldBeFalse();
    }

    [Fact]
    public void Compare_SoftDeleteEnabled_DetectsConfigChange()
    {
        var previous = new TestSettings();
        var current = TestSettingsFactory.WithSoftDelete();

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.ConfigChanges.SoftDeleteChanged.ShouldBeTrue();
        diff.ConfigChanges.SoftDeleteEnabled.ShouldBe(true);
    }

    [Fact]
    public void Compare_AuditingEnabled_DetectsConfigChange()
    {
        var previous = new TestSettings();
        var current = TestSettingsFactory.WithAuditing();

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.ConfigChanges.AuditingChanged.ShouldBeTrue();
        diff.ConfigChanges.AuditingEnabled.ShouldBe(true);
    }

    [Fact]
    public void Compare_DatabaseProviderChanged_DetectsConfigChange()
    {
        var previous = new TestSettings();
        var current = TestSettingsFactory.WithPostgres();

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.ConfigChanges.DatabaseProviderChanged.ShouldBeTrue();
        diff.ConfigChanges.OldDatabaseProvider.ShouldBe("sqlite");
        diff.ConfigChanges.NewDatabaseProvider.ShouldBe("postgresql");
    }

    [Fact]
    public void Compare_AotEnabled_DetectsConfigChange()
    {
        var previous = new TestSettings();
        var current = TestSettingsFactory.WithAot();

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.ConfigChanges.AotChanged.ShouldBeTrue();
    }

    [Fact]
    public void Compare_AuthAdded_DetectsConfigChange()
    {
        var previous = new TestSettings();
        var current = TestSettingsFactory.WithAuth("Admin", "User");

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.ConfigChanges.AuthChanged.ShouldBeTrue();
    }

    [Fact]
    public void Compare_RateLimitAdded_DetectsConfigChange()
    {
        var previous = new TestSettings();
        var current = TestSettingsFactory.WithRateLimit();

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.ConfigChanges.RateLimitChanged.ShouldBeTrue();
    }

    [Fact]
    public void Compare_VersioningAdded_DetectsConfigChange()
    {
        var previous = new TestSettings();
        var current = TestSettingsFactory.WithVersioning();

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.ConfigChanges.VersioningChanged.ShouldBeTrue();
    }

    [Fact]
    public void Compare_IdenticalConfig_ReturnsNoConfigChanges()
    {
        var settings = new TestSettings();

        var diff = SchemaDiffer.Compare(settings, settings);

        diff.ConfigChanges.HasChanges.ShouldBeFalse();
        diff.ConfigChanges.SoftDeleteChanged.ShouldBeFalse();
        diff.ConfigChanges.AuditingChanged.ShouldBeFalse();
        diff.ConfigChanges.DatabaseProviderChanged.ShouldBeFalse();
        diff.ConfigChanges.OldDatabaseProvider.ShouldBeNull();
        diff.ConfigChanges.NewDatabaseProvider.ShouldBeNull();
    }

    [Fact]
    public void Compare_EnumAdded_DetectsAddition()
    {
        var previous = new TestSettings();
        var current = new EnumTestSettings();

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.EnumChanges.Count.ShouldBe(2);
        diff.EnumChanges.ShouldAllBe(e => e.Kind == ChangeKind.Added);

        var priority = diff.EnumChanges.First(e => e.EnumName == "Priority");
        priority.After.ShouldNotBeNull();
        priority.After.ShouldContain("Low");
        priority.After.ShouldContain("Critical");
    }

    [Fact]
    public void Compare_EnumRemoved_DetectsRemoval()
    {
        var previous = new EnumTestSettings();
        var current = new TestSettings();

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.EnumChanges.Count.ShouldBe(2);
        diff.EnumChanges.ShouldAllBe(e => e.Kind == ChangeKind.Removed);
    }

    [Fact]
    public void Compare_EnumModified_DetectsValueChanges()
    {
        var previous = new ConfiguredSettings(
            new TestConfiguration(),
            new TestEntitiesCollection(),
            new Dictionary<string, List<string>>
            {
                ["Priority"] = ["Low", "Medium", "High"],
            });

        var current = new ConfiguredSettings(
            new TestConfiguration(),
            new TestEntitiesCollection(),
            new Dictionary<string, List<string>>
            {
                ["Priority"] = ["Low", "Medium", "High", "Critical"],
            });

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.EnumChanges.Count.ShouldBe(1);

        var enumDiff = diff.EnumChanges[0];
        enumDiff.Kind.ShouldBe(ChangeKind.Modified);
        enumDiff.AddedValues.ShouldContain("Critical");
        enumDiff.RemovedValues.ShouldBeEmpty();
    }

    [Fact]
    public void Compare_RelationshipAdded_DetectsAddition()
    {
        var prevEntities = new SinglePropertyEntities("Author", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Name", new NinjadogEntityProperty("String") },
        });

        var currProps = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Name", new NinjadogEntityProperty("String") },
        };
        var currRels = new NinjadogEntityRelationships
        {
            { "Posts", new NinjadogEntityRelationship("Post", NinjadogEntityRelationshipType.OneToMany) },
        };
        var currEntities = new NinjadogEntitiesWithRelationship("Author", currProps, currRels);

        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);
        var current = new ConfiguredSettings(new TestConfiguration(), currEntities);

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        var entity = diff.ModifiedEntities.First();
        entity.RelationshipChanges.Count.ShouldBe(1);
        entity.RelationshipChanges[0].Kind.ShouldBe(ChangeKind.Added);
        entity.RelationshipChanges[0].RelationshipName.ShouldBe("Posts");
    }

    [Fact]
    public void Compare_MultipleChanges_DetectsAll()
    {
        var prevEntities = new SinglePropertyEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
        });
        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);

        var currEntities = new TwoEntitiesCollection();
        var current = new ConfiguredSettings(
            new TestConfiguration() with { SoftDelete = true, DatabaseProvider = "postgresql" },
            currEntities,
            new Dictionary<string, List<string>> { ["Status"] = ["Active", "Inactive"] });

        var diff = SchemaDiffer.Compare(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.EntityChanges.Count.ShouldBeGreaterThan(0);
        diff.ConfigChanges.SoftDeleteChanged.ShouldBeTrue();
        diff.ConfigChanges.DatabaseProviderChanged.ShouldBeTrue();
        diff.EnumChanges.Count.ShouldBe(1);
    }

    [Fact]
    public void None_HasNoChanges()
    {
        SchemaDiff.None.HasChanges.ShouldBeFalse();
        SchemaDiff.None.EntityChanges.ShouldBeEmpty();
        SchemaDiff.None.EnumChanges.ShouldBeEmpty();
        SchemaDiff.None.ConfigChanges.HasChanges.ShouldBeFalse();
    }

    private sealed class TestEntitiesWithExtra : NinjadogEntities
    {
        public TestEntitiesWithExtra()
        {
            var guid = TestEntities.CreateGuidKeyEntity();
            Add(guid.Key, new(guid.Properties));

            var category = TestEntities.CreateStringOnlyEntity();
            Add(category.Key, new(category.Properties));
        }
    }

    private sealed class TwoEntitiesCollection : NinjadogEntities
    {
        public TwoEntitiesCollection()
        {
            var guid = TestEntities.CreateGuidKeyEntity();
            Add(guid.Key, new(guid.Properties));

            var category = TestEntities.CreateStringOnlyEntity();
            Add(category.Key, new(category.Properties));
        }
    }

    private sealed class SinglePropertyEntities : NinjadogEntities
    {
        public SinglePropertyEntities(string key, NinjadogEntityProperties properties)
        {
            Add(key, new NinjadogEntity(properties));
        }
    }

    private sealed class NinjadogEntitiesWithRelationship : NinjadogEntities
    {
        public NinjadogEntitiesWithRelationship(
            string key,
            NinjadogEntityProperties properties,
            NinjadogEntityRelationships relationships)
        {
            Add(key, new NinjadogEntity(properties, relationships));
        }
    }
}
