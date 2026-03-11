using Ninjadog.Engine.Core.Models;
using Ninjadog.Engine.Core.ValueObjects;
using Ninjadog.Settings;
using Ninjadog.Settings.Extensions.Entities;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Models;

public class NinjadogTemplateTests
{
    [Fact]
    public void GenerateOne_DefaultImplementation_ReturnsEmpty()
    {
        var template = new TestPerEntityTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void GenerateOneByEntity_DefaultImplementation_ReturnsEmpty()
    {
        var template = new TestSingleTemplate();
        var entity = TestEntities.CreateGuidKeyEntity();
        var result = template.GenerateOneByEntity(entity, "Test");
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void GenerateMany_IteratesOverEntities()
    {
        var template = new TestPerEntityTemplate();
        var settings = new TestSettings();
        var results = template.GenerateMany(settings).ToList();

        Assert.Single(results);
        Assert.Equal("TodoItem.cs", results[0].FileName);
        Assert.Contains("TodoItem", results[0].Content);
    }

    [Fact]
    public void GenerateOne_CustomTemplate_ProducesContent()
    {
        var template = new TestSingleTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);

        Assert.False(result.IsEmpty);
        Assert.Equal("Test.cs", result.FileName);
        Assert.Equal("// generated", result.Content);
    }

    [Fact]
    public void Category_DefaultsToNull()
    {
        var template = new TestSingleTemplate();
        Assert.Null(template.Category);
    }

    [Fact]
    public void Category_CanBeSet()
    {
        var template = new TestSingleTemplate { Category = "Endpoints" };
        Assert.Equal("Endpoints", template.Category);
    }

    [Fact]
    public void CreateNinjadogContentFile_IncludesCategoryInKey()
    {
        var template = new TestSingleTemplate { Category = "Database" };
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        Assert.Equal("Database/Test.cs", result.Key);
    }

    [Fact]
    public void WriteFileScopedNamespace_ReturnsNamespaceDeclaration()
    {
        var template = new TestPerEntityTemplate();
        var entity = TestEntities.CreateGuidKeyEntity();
        var result = template.GenerateOneByEntity(entity, "MyApp.Api");
        Assert.Contains("namespace MyApp.Api;", result.Content);
    }

    private sealed class TestSingleTemplate : NinjadogTemplate
    {
        public override string Name => "TestSingle";

        public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
        {
            return CreateNinjadogContentFile("Test.cs", "// generated", useDefaultLayout: false);
        }
    }

    private sealed class TestPerEntityTemplate : NinjadogTemplate
    {
        public override string Name => "TestPerEntity";

        public override NinjadogContentFile GenerateOneByEntity(NinjadogEntityWithKey entity, string rootNamespace)
        {
            return CreateNinjadogContentFile(
                $"{entity.StringTokens.Model}.cs",
                $"namespace {rootNamespace}; public class {entity.StringTokens.Model} {{ }}",
                useDefaultLayout: false);
        }
    }
}

public class GetRouteConstraintTests
{
    [Theory]
    [InlineData("Guid", "guid")]
    [InlineData("Int32", "int")]
    [InlineData("Int64", "long")]
    [InlineData("String", "alpha")]
    public void GetRouteConstraint_ReturnsCorrectConstraint(string typeName, string expected)
    {
        Assert.Equal(expected, RouteConstraintTestTemplate.TestGetRouteConstraint(typeName));
    }

    [Theory]
    [InlineData("Decimal")]
    [InlineData("Boolean")]
    [InlineData("UnknownType")]
    public void GetRouteConstraint_UnknownType_ReturnsEmpty(string typeName)
    {
        Assert.Equal(string.Empty, RouteConstraintTestTemplate.TestGetRouteConstraint(typeName));
    }

    private sealed class RouteConstraintTestTemplate : NinjadogTemplate
    {
        public override string Name => "RouteTest";

        public static string TestGetRouteConstraint(string typeName)
        {
            return GetRouteConstraint(typeName);
        }
    }
}
