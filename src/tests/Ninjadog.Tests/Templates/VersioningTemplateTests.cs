using Ninjadog.Templates.CrudWebAPI.Template;
using Ninjadog.Templates.CrudWebAPI.Template.Endpoints;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class VersioningTemplateTests
{
    [Fact]
    public Task GetAllEndpoint_WithVersioning_IncludesVersionCall()
    {
        var template = new GetAllEndpointTemplate();
        var settings = new VersioningSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results[0].Content);
    }

    [Fact]
    public Task GetEndpoint_WithVersioning_IncludesVersionCall()
    {
        var template = new GetEndpointTemplate();
        var settings = new VersioningSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results[0].Content);
    }

    [Fact]
    public Task CreateEndpoint_WithVersioning_IncludesVersionCall()
    {
        var template = new CreateEndpointTemplate();
        var settings = new VersioningSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results[0].Content);
    }

    [Fact]
    public Task DeleteEndpoint_WithVersioning_IncludesVersionCall()
    {
        var template = new DeleteEndpointTemplate();
        var settings = new VersioningSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results[0].Content);
    }

    [Fact]
    public Task UpdateEndpoint_WithVersioning_IncludesVersionCall()
    {
        var template = new UpdateEndpointTemplate();
        var settings = new VersioningSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results[0].Content);
    }

    [Fact]
    public Task Extensions_WithUrlPathVersioning_ConfiguresVersioningInUseFastEndpoints()
    {
        var template = new CrudWebApiExtensionsTemplate();
        var settings = new VersioningSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task Extensions_WithHeaderVersioning_ConfiguresVersioningWithPrependFalse()
    {
        var template = new CrudWebApiExtensionsTemplate();
        var settings = new HeaderVersioningSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }
}
