using Ninjadog.Templates.CrudWebAPI.Template.Middleware;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class MiddlewareTemplateTests
{
    [Fact]
    public Task RequestCorrelationMiddleware_ProducesCorrectOutput()
    {
        var template = new RequestCorrelationMiddlewareTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }
}
