using Ninjadog.Evolution;
using Ninjadog.Evolution.Migrations;
using Ninjadog.Settings.Entities;
using Ninjadog.Settings.Entities.Properties;
using Ninjadog.Tests.Helpers;
using Shouldly;

namespace Ninjadog.Tests.Evolution;

public class MigrationSqlGeneratorTests
{
    [Fact]
    public void Generate_NoChanges_ReturnsEmptyList()
    {
        var settings = new TestSettings();
        var diff = SchemaDiff.None;

        var operations = MigrationSqlGenerator.Generate(diff, settings);

        operations.ShouldBeEmpty();
    }

    [Fact]
    public Task Generate_NewEntity_ProducesCreateTable()
    {
        var previous = new TestSettings();
        var currentEntities = new EntitiesWithCategory();
        var current = new ConfiguredSettings(new TestConfiguration(), currentEntities);

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        var createOp = operations.First(o => o.Description.Contains("Create table"));
        return Verify(createOp.Sql);
    }

    [Fact]
    public Task Generate_NewEntity_PostgreSql_ProducesCorrectTypes()
    {
        var previous = new ConfiguredSettings(
            new TestConfiguration() with { DatabaseProvider = "postgresql" },
            new TestEntitiesCollection());
        var current = new ConfiguredSettings(
            new TestConfiguration() with { DatabaseProvider = "postgresql" },
            new EntitiesWithCategory());

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        var createOp = operations.First(o => o.Description.Contains("Create table"));
        return Verify(createOp.Sql);
    }

    [Fact]
    public Task Generate_NewEntity_SqlServer_ProducesCorrectTypes()
    {
        var previous = new ConfiguredSettings(
            new TestConfiguration() with { DatabaseProvider = "sqlserver" },
            new TestEntitiesCollection());
        var current = new ConfiguredSettings(
            new TestConfiguration() with { DatabaseProvider = "sqlserver" },
            new EntitiesWithCategory());

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        var createOp = operations.First(o => o.Description.Contains("Create table"));
        return Verify(createOp.Sql);
    }

    [Fact]
    public void Generate_RemovedEntity_ProducesDropTable()
    {
        var previous = new ConfiguredSettings(new TestConfiguration(), new EntitiesWithCategory());
        var current = new ConfiguredSettings(new TestConfiguration(), new TestEntitiesCollection());

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        var dropOp = operations.First(o => o.Description.Contains("Drop table"));
        dropOp.Sql.ShouldContain("DROP TABLE IF EXISTS Categories");
    }

    [Fact]
    public void Generate_AddedProperty_ProducesAlterTableAddColumn()
    {
        var prevEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
        });
        var currEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
            { "Priority", new NinjadogEntityProperty("Int32") },
        });

        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);
        var current = new ConfiguredSettings(new TestConfiguration(), currEntities);

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        operations.Count.ShouldBe(1);
        operations[0].Sql.ShouldContain("ALTER TABLE TodoItems ADD COLUMN Priority INTEGER");
    }

    [Fact]
    public void Generate_AddedRequiredProperty_IncludesDefaultValue()
    {
        var prevEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
        });
        var currEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Name", new NinjadogEntityProperty("String", Required: true) },
        });

        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);
        var current = new ConfiguredSettings(new TestConfiguration(), currEntities);

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        operations[0].Sql.ShouldContain("NOT NULL");
        operations[0].Sql.ShouldContain("DEFAULT ''");
    }

    [Fact]
    public void Generate_RemovedProperty_Sqlite_ProducesWarning()
    {
        var prevEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
            { "Priority", new NinjadogEntityProperty("Int32") },
        });
        var currEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
        });

        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);
        var current = new ConfiguredSettings(new TestConfiguration(), currEntities);

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        operations[0].IsWarning.ShouldBeTrue();
        operations[0].Sql.ShouldContain("SQLite does not support DROP COLUMN");
    }

    [Fact]
    public void Generate_RemovedProperty_PostgreSql_ProducesDropColumn()
    {
        var prevEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
            { "Priority", new NinjadogEntityProperty("Int32") },
        });
        var currEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String") },
        });

        var previous = new ConfiguredSettings(
            new TestConfiguration() with { DatabaseProvider = "postgresql" },
            prevEntities);
        var current = new ConfiguredSettings(
            new TestConfiguration() with { DatabaseProvider = "postgresql" },
            currEntities);

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        operations[0].IsWarning.ShouldBeFalse();
        operations[0].Sql.ShouldBe("ALTER TABLE TodoItems DROP COLUMN Priority;");
    }

    [Fact]
    public void Generate_TypeChanged_PostgreSql_ProducesAlterColumnType()
    {
        var prevEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Priority", new NinjadogEntityProperty("String") },
        });
        var currEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Priority", new NinjadogEntityProperty("Int32") },
        });

        var previous = new ConfiguredSettings(
            new TestConfiguration() with { DatabaseProvider = "postgresql" },
            prevEntities);
        var current = new ConfiguredSettings(
            new TestConfiguration() with { DatabaseProvider = "postgresql" },
            currEntities);

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        operations[0].Sql.ShouldBe("ALTER TABLE TodoItems ALTER COLUMN Priority TYPE INTEGER;");
    }

    [Fact]
    public void Generate_TypeChanged_SqlServer_ProducesAlterColumn()
    {
        var prevEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Priority", new NinjadogEntityProperty("String") },
        });
        var currEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Priority", new NinjadogEntityProperty("Int32") },
        });

        var previous = new ConfiguredSettings(
            new TestConfiguration() with { DatabaseProvider = "sqlserver" },
            prevEntities);
        var current = new ConfiguredSettings(
            new TestConfiguration() with { DatabaseProvider = "sqlserver" },
            currEntities);

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        operations[0].Sql.ShouldBe("ALTER TABLE TodoItems ALTER COLUMN Priority INT;");
    }

    [Fact]
    public void Generate_TypeChanged_Sqlite_ProducesWarning()
    {
        var prevEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Priority", new NinjadogEntityProperty("String") },
        });
        var currEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Priority", new NinjadogEntityProperty("Int32") },
        });

        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);
        var current = new ConfiguredSettings(new TestConfiguration(), currEntities);

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        operations[0].IsWarning.ShouldBeTrue();
        operations[0].Sql.ShouldContain("SQLite does not support ALTER COLUMN");
    }

    [Fact]
    public void Generate_SoftDeleteEnabled_ProducesWarning()
    {
        var previous = new TestSettings();
        var current = TestSettingsFactory.WithSoftDelete();

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        var softDeleteOp = operations.First(o => o.Description.Contains("soft delete"));
        softDeleteOp.IsWarning.ShouldBeTrue();
        softDeleteOp.Sql.ShouldContain("IsDeleted");
        softDeleteOp.Sql.ShouldContain("DeletedAt");
    }

    [Fact]
    public void Generate_AuditingEnabled_ProducesWarning()
    {
        var previous = new TestSettings();
        var current = TestSettingsFactory.WithAuditing();

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        var auditOp = operations.First(o => o.Description.Contains("auditing"));
        auditOp.IsWarning.ShouldBeTrue();
        auditOp.Sql.ShouldContain("CreatedAt");
        auditOp.Sql.ShouldContain("UpdatedAt");
    }

    [Fact]
    public void Generate_DatabaseProviderChanged_ProducesWarning()
    {
        var previous = new TestSettings();
        var current = TestSettingsFactory.WithPostgres();

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        var providerOp = operations.First(o => o.Description.Contains("provider"));
        providerOp.IsWarning.ShouldBeTrue();
        providerOp.Sql.ShouldContain("sqlite");
        providerOp.Sql.ShouldContain("postgresql");
    }

    [Fact]
    public void Generate_ConstraintOnlyChange_ProducesNoOperations()
    {
        var prevEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String", MaxLength: 100) },
        });
        var currEntities = new SimpleEntities("TodoItem", new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", true) },
            { "Title", new NinjadogEntityProperty("String", MaxLength: 500) },
        });

        var previous = new ConfiguredSettings(new TestConfiguration(), prevEntities);
        var current = new ConfiguredSettings(new TestConfiguration(), currEntities);

        var diff = SchemaDiffer.Compare(previous, current);
        var operations = MigrationSqlGenerator.Generate(diff, current);

        operations.ShouldBeEmpty();
    }

    private sealed class EntitiesWithCategory : NinjadogEntities
    {
        public EntitiesWithCategory()
        {
            var guid = TestEntities.CreateGuidKeyEntity();
            Add(guid.Key, new(guid.Properties));

            var category = TestEntities.CreateStringOnlyEntity();
            Add(category.Key, new(category.Properties));
        }
    }

    private sealed class SimpleEntities : NinjadogEntities
    {
        public SimpleEntities(string key, NinjadogEntityProperties properties)
        {
            Add(key, new NinjadogEntity(properties));
        }
    }
}
