using Ninjadog.Templates.CrudWebAPI.Template.Endpoints;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class HealthCheckTemplateTests
{
    [Fact]
    public Task HealthEndpoint_ProducesCorrectOutput()
    {
        var template = new HealthEndpointTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task ReadyEndpoint_ProducesCorrectOutput()
    {
        var template = new ReadyEndpointTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }
}
