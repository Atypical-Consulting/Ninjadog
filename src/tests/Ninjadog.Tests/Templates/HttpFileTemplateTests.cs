using Ninjadog.Templates.CrudWebAPI.Template;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class HttpFileTemplateTests
{
    [Fact]
    public Task HttpFile_ProducesCorrectOutput()
    {
        var template = new HttpFileTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task HttpFile_MultipleEntities_ProducesCorrectOutput()
    {
        var template = new HttpFileTemplate();
        var settings = new RelationshipSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }
}
