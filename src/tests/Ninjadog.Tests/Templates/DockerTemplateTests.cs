using Ninjadog.Templates.CrudWebAPI.Template.Docker;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class DockerTemplateTests
{
    [Fact]
    public Task Dockerfile_ProducesCorrectOutput()
    {
        var template = new DockerfileTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task DockerCompose_Sqlite_ProducesCorrectOutput()
    {
        var template = new DockerComposeTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task DockerCompose_Postgres_ProducesCorrectOutput()
    {
        var template = new DockerComposeTemplate();
        var settings = TestSettingsFactory.WithPostgres();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task DockerCompose_SqlServer_ProducesCorrectOutput()
    {
        var template = new DockerComposeTemplate();
        var settings = TestSettingsFactory.WithSqlServer();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task DockerIgnore_ProducesCorrectOutput()
    {
        var template = new DockerIgnoreTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }
}
