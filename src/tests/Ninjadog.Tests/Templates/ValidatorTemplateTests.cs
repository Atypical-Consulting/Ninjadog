using Ninjadog.Templates.CrudWebAPI.Template.Validation;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class ValidatorTemplateTests
{
    [Fact]
    public Task CreateValidator_WithMixedTypes_SkipsValueTypes()
    {
        var template = new CreateRequestValidatorTemplate();
        var entity = TestEntities.CreateGuidKeyEntity();
        var result = template.GenerateOneByEntity(entity, "TestApp.Api");
        return Verify(result.Content);
    }

    [Fact]
    public Task UpdateValidator_WithMixedTypes_SkipsValueTypes()
    {
        var template = new UpdateRequestValidatorTemplate();
        var entity = TestEntities.CreateGuidKeyEntity();
        var result = template.GenerateOneByEntity(entity, "TestApp.Api");
        return Verify(result.Content);
    }

    [Fact]
    public Task CreateValidator_WithStringOnlyEntity_ValidatesAllNonKeyProps()
    {
        var template = new CreateRequestValidatorTemplate();
        var entity = TestEntities.CreateStringOnlyEntity();
        var result = template.GenerateOneByEntity(entity, "TestApp.Api");
        return Verify(result.Content);
    }

    [Fact]
    public Task CreateValidator_WithValidationAttributes_GeneratesFluentRules()
    {
        var template = new CreateRequestValidatorTemplate();
        var entity = TestEntities.CreateValidatedEntity();
        var result = template.GenerateOneByEntity(entity, "TestApp.Api");
        return Verify(result.Content);
    }

    [Fact]
    public Task UpdateValidator_WithValidationAttributes_GeneratesFluentRules()
    {
        var template = new UpdateRequestValidatorTemplate();
        var entity = TestEntities.CreateValidatedEntity();
        var result = template.GenerateOneByEntity(entity, "TestApp.Api");
        return Verify(result.Content);
    }
}
