using Ninjadog.Templates.CrudWebAPI.Template;
using Ninjadog.Templates.CrudWebAPI.Template.Repositories;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class AotTemplateTests
{
    [Fact]
    public Task GenerateProgram_WithAot_UsesSlimBuilder()
    {
        var template = new ProgramTemplate();
        var settings = TestSettingsFactory.WithAot();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateProgram_WithoutAot_UsesStandardBuilder()
    {
        var template = new ProgramTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateProgram_WithAotAndSeedData_IncludesSeederCall()
    {
        var template = new ProgramTemplate();
        var settings = TestSettingsFactory.WithAotSeeded();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateExtensions_WithAot_RemovesSwaggerAndConfiguresFastEndpoints()
    {
        var template = new CrudWebApiExtensionsTemplate();
        var settings = TestSettingsFactory.WithAot();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateExtensions_WithoutAot_IncludesSwaggerAndClientGen()
    {
        var template = new CrudWebApiExtensionsTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateExtensions_WithAotAndSeedData_IncludesSeederRegistration()
    {
        var template = new CrudWebApiExtensionsTemplate();
        var settings = TestSettingsFactory.WithAotSeeded();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateJsonSerializerContext_WithAot_GeneratesContext()
    {
        var template = new JsonSerializerContextTemplate();
        var settings = TestSettingsFactory.WithAot();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public void GenerateJsonSerializerContext_WithoutAot_ReturnsEmpty()
    {
        var template = new JsonSerializerContextTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        Assert.Equal(string.Empty, result.Content);
    }

    [Fact]
    public Task GenerateJsonSerializerContext_WithAotMultipleEntities_IncludesAllTypes()
    {
        var template = new JsonSerializerContextTemplate();
        var settings = TestSettingsFactory.WithAotSeeded();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateRepository_WithAot_AddsDapperAotAttribute()
    {
        var template = new RepositoryTemplate();
        var settings = TestSettingsFactory.WithAot();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results.Select(r => r.Content));
    }

    [Fact]
    public Task GenerateRepository_WithoutAot_NoDapperAotAttribute()
    {
        var template = new RepositoryTemplate();
        var settings = new TestSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results.Select(r => r.Content));
    }
}
