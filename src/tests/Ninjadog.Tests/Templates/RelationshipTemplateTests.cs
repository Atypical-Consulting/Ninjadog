using Ninjadog.Templates.CrudWebAPI.Template.Endpoints;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class RelationshipTemplateTests
{
    [Fact]
    public Task GetByParentEndpoint_WithOneToManyRelationship_GeneratesNestedEndpoint()
    {
        var template = new GetByParentEndpointTemplate();
        var settings = TestSettingsFactory.WithRelationships();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results.Select(r => r.Content));
    }

    [Fact]
    public Task GetByParentEndpoint_WithNoRelationships_GeneratesEmpty()
    {
        var template = new GetByParentEndpointTemplate();
        var settings = new TestSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results.Select(r => r.Content));
    }
}
