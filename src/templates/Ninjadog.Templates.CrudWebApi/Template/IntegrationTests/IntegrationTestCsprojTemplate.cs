namespace Ninjadog.Templates.CrudWebAPI.Template.IntegrationTests;

/// <summary>
/// This template generates the .csproj file for the integration test project.
/// </summary>
public sealed class IntegrationTestCsprojTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "IntegrationTestCsproj";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var projectName = ninjadogSettings.Config.Name;
        var fileName = $"{projectName}.IntegrationTests.csproj";

        var content =
            $$"""
              <Project Sdk="Microsoft.NET.Sdk">

                <PropertyGroup>
                  <TargetFramework>net10.0</TargetFramework>
                  <ImplicitUsings>enable</ImplicitUsings>
                  <Nullable>enable</Nullable>
                  <IsPackable>false</IsPackable>
                  <IsTestProject>true</IsTestProject>
                </PropertyGroup>

                <ItemGroup>
                  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.5" />
                  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
                  <PackageReference Include="xunit" Version="2.9.3" />
                  <PackageReference Include="xunit.runner.visualstudio" Version="3.1.1" />
                  <PackageReference Include="FluentAssertions" Version="8.3.0" />
                </ItemGroup>

                <ItemGroup>
                  <ProjectReference Include="..\{{projectName}}.CrudWebAPI\{{projectName}}.CrudWebAPI.csproj" />
                </ItemGroup>

              </Project>
              """;

        return CreateNinjadogContentFile(fileName, content, useDefaultLayout: false);
    }
}
