using Ninjadog.Templates.CrudWebAPI.Template.IntegrationTests;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class IntegrationTestTemplateTests
{
    [Fact]
    public Task GenerateCsproj_ProducesCorrectProjectFile()
    {
        var template = new IntegrationTestCsprojTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateWebApplicationFactory_ProducesCorrectFactory()
    {
        var template = new CustomWebApplicationFactoryTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateTestBase_ProducesCorrectBaseClass()
    {
        var template = new IntegrationTestBaseTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateEntityTest_WithGuidKey_ProducesCorrectTests()
    {
        var template = new EntityIntegrationTestTemplate();
        var settings = new TestSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results.Select(r => r.Content));
    }

    [Fact]
    public Task GenerateEntityTest_WithIntKey_ProducesCorrectTests()
    {
        var template = new EntityIntegrationTestTemplate();
        var entity = TestEntities.CreateIntKeyEntity();
        var result = template.GenerateOneByEntity(entity, "TestApp.Api");
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateEntityTest_WithStringOnlyEntity_ProducesCorrectTests()
    {
        var template = new EntityIntegrationTestTemplate();
        var entity = TestEntities.CreateStringOnlyEntity();
        var result = template.GenerateOneByEntity(entity, "TestApp.Api");
        return Verify(result.Content);
    }
}
