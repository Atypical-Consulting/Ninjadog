using Ninjadog.Templates.CrudWebAPI.Template;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class IndexPageTemplateTests
{
    [Fact]
    public Task IndexPage_ProducesCorrectOutput()
    {
        var template = new IndexPageTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task IndexPage_MultipleEntities_ProducesCorrectOutput()
    {
        var template = new IndexPageTemplate();
        var settings = TestSettingsFactory.WithRelationships();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }
}
