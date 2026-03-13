using Ninjadog.Templates.CrudWebAPI.Template;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class ProgramTemplateTests
{
    private readonly ProgramTemplate _template = new();

    [Fact]
    public Task GenerateOne_WithDefaultSettings_ProducesCorrectOutput()
    {
        var settings = new TestSettings();
        var result = _template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateOne_WithRateLimit_AddsRateLimiter()
    {
        var settings = TestSettingsFactory.WithRateLimit();
        var result = _template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task GenerateOne_WithCustomRateLimit_UsesCustomValues()
    {
        var settings = TestSettingsFactory.WithCustomRateLimit();
        var result = _template.GenerateOne(settings);
        return Verify(result.Content);
    }
}
