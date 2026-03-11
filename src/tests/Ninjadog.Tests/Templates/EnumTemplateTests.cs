using Ninjadog.Templates.CrudWebAPI.Template.Domain;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class EnumTemplateTests
{
    private readonly EnumTemplate _template = new();

    [Fact]
    public Task GenerateMany_WithEnums_ProducesEnumFiles()
    {
        var settings = new EnumTestSettings();
        var results = _template.GenerateMany(settings).ToList();
        return Verify(results.Select(r => r.Content));
    }

    [Fact]
    public Task GenerateMany_WithNoEnums_ProducesEmpty()
    {
        var settings = new TestSettings();
        var results = _template.GenerateMany(settings).ToList();
        return Verify(results.Select(r => r.Content));
    }
}
