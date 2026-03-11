using Ninjadog.Templates.CrudWebAPI.Template.Database;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class DatabaseSeederTemplateTests
{
    [Fact]
    public Task GenerateOne_WithSeedData_ProducesSeederClass()
    {
        var template = new DatabaseSeederTemplate();
        var settings = new SeededSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateOne_WithNoSeedData_ProducesEmpty()
    {
        var template = new DatabaseSeederTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }
}
