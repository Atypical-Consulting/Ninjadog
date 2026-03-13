using Ninjadog.Templates.CrudWebAPI.Template.Database;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class DatabaseInitializerTemplateTests
{
    private readonly DatabaseInitializerTemplate _template = new();

    [Fact]
    public Task GenerateOne_WithGuidKeyEntity_ProducesCorrectSql()
    {
        var settings = new TestSettings();
        var result = _template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateOne_WithSoftDelete_AddsSoftDeleteColumns()
    {
        var settings = TestSettingsFactory.WithSoftDelete();
        var result = _template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateOne_WithAuditing_AddsAuditColumns()
    {
        var settings = TestSettingsFactory.WithAuditing();
        var result = _template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateOne_WithRelationships_AddsForeignKeyConstraints()
    {
        var settings = TestSettingsFactory.WithRelationships();
        var result = _template.GenerateOne(settings);
        return Verify(result.Content);
    }
}
