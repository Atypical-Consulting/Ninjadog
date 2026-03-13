using Ninjadog.Templates.CrudWebAPI.Template.Repositories;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class RepositoryTemplateTests
{
    private readonly RepositoryTemplate _template = new();

    [Fact]
    public Task GenerateOneByEntity_GuidKey_UsesEntityKeyInSql()
    {
        var entity = TestEntities.CreateGuidKeyEntity();
        var result = _template.GenerateOneByEntity(entity, "TestApp.Api");
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateOneByEntity_IntKey_UsesEntityKeyInSql()
    {
        var entity = TestEntities.CreateIntKeyEntity();
        var result = _template.GenerateOneByEntity(entity, "TestApp.Api");
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateMany_WithSoftDelete_AddsSoftDeleteLogic()
    {
        var settings = TestSettingsFactory.WithSoftDelete();
        var results = _template.GenerateMany(settings).ToList();
        return Verify(results.Select(r => r.Content));
    }

    [Fact]
    public Task GenerateMany_WithAuditing_AddsAuditLogic()
    {
        var settings = TestSettingsFactory.WithAuditing();
        var results = _template.GenerateMany(settings).ToList();
        return Verify(results.Select(r => r.Content));
    }
}
