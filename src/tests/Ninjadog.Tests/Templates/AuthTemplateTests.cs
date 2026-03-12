using Ninjadog.Templates.CrudWebAPI.Template;
using Ninjadog.Templates.CrudWebAPI.Template.Auth;
using Ninjadog.Templates.CrudWebAPI.Template.Endpoints;
using Ninjadog.Tests.Helpers;

namespace Ninjadog.Tests.Templates;

public partial class AuthTemplateTests
{
    [Fact]
    public Task AuthExtensions_WithAuth_GeneratesJwtSetup()
    {
        var template = new AuthExtensionsTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task AuthExtensions_WithoutAuth_ReturnsEmpty()
    {
        var template = new AuthExtensionsTemplate();
        var settings = new TestSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task TokenServiceInterface_WithAuth_GeneratesInterface()
    {
        var template = new TokenServiceInterfaceTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task TokenService_WithAuth_GeneratesImplementation()
    {
        var template = new TokenServiceTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task UserEntity_WithAuth_GeneratesUserClass()
    {
        var template = new UserEntityTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task UserRepositoryInterface_WithAuth_GeneratesInterface()
    {
        var template = new UserRepositoryInterfaceTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task UserRepository_WithAuth_GeneratesImplementation()
    {
        var template = new UserRepositoryTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task UserInitializer_WithAuth_GeneratesTableCreation()
    {
        var template = new UserInitializerTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task LoginEndpoint_WithAuth_GeneratesEndpoint()
    {
        var template = new LoginEndpointTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task RegisterEndpoint_WithAuth_GeneratesEndpoint()
    {
        var template = new RegisterEndpointTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task LoginRequest_WithAuth_GeneratesRecord()
    {
        var template = new LoginRequestTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task LoginResponse_WithAuth_GeneratesRecord()
    {
        var template = new LoginResponseTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task RegisterRequest_WithAuth_GeneratesRecord()
    {
        var template = new RegisterRequestTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task RegisterResponse_WithAuth_GeneratesRecord()
    {
        var template = new RegisterResponseTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task LoginRequestValidator_WithAuth_GeneratesValidator()
    {
        var template = new LoginRequestValidatorTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task RegisterRequestValidator_WithAuth_GeneratesValidator()
    {
        var template = new RegisterRequestValidatorTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task Program_WithAuth_IncludesAuthMiddleware()
    {
        var template = new ProgramTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task AppSettings_WithAuth_IncludesJwtSection()
    {
        var template = new AppSettingsTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task CrudWebApiExtensions_WithAuth_IncludesAuthServices()
    {
        var template = new CrudWebApiExtensionsTemplate();
        var settings = new AuthSettings();
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task CreateEndpoint_WithAuth_RemovesAllowAnonymous()
    {
        var template = new CreateEndpointTemplate();
        var settings = new AuthSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results.First().Content);
    }

    [Fact]
    public Task DeleteEndpoint_WithAuth_RemovesAllowAnonymous()
    {
        var template = new DeleteEndpointTemplate();
        var settings = new AuthSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results.First().Content);
    }

    [Fact]
    public Task UpdateEndpoint_WithAuth_RemovesAllowAnonymous()
    {
        var template = new UpdateEndpointTemplate();
        var settings = new AuthSettings();
        var results = template.GenerateMany(settings).ToList();
        return Verify(results.First().Content);
    }
}
