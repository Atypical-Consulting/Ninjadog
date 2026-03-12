using Ninjadog.Templates.CrudWebAPI.Template;
using Ninjadog.Templates.CrudWebAPI.Template.Auth;
using Ninjadog.Templates.CrudWebAPI.Template.Contracts.Data;
using Ninjadog.Templates.CrudWebAPI.Template.Contracts.Requests;
using Ninjadog.Templates.CrudWebAPI.Template.Contracts.Responses;
using Ninjadog.Templates.CrudWebAPI.Template.Database;
using Ninjadog.Templates.CrudWebAPI.Template.Docker;
using Ninjadog.Templates.CrudWebAPI.Template.Domain;
using Ninjadog.Templates.CrudWebAPI.Template.Endpoints;
using Ninjadog.Templates.CrudWebAPI.Template.Mapping;
using Ninjadog.Templates.CrudWebAPI.Template.Repositories;
using Ninjadog.Templates.CrudWebAPI.Template.Services;
using Ninjadog.Templates.CrudWebAPI.Template.Summaries;
using Ninjadog.Templates.CrudWebAPI.Template.Validation;

namespace Ninjadog.Templates.CrudWebAPI.Setup;

/// <summary>
/// Represents a specific collection of templates for CRUD operations in a Web API.
/// This class initializes various types of request, response, and data transfer object (DTO) templates.
/// </summary>
public class CrudTemplates : NinjadogTemplates
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CrudTemplates"/> class,
    /// adding templates for different CRUD operations.
    /// </summary>
    public CrudTemplates()
    {
        Add(new AppSettingsTemplate());
        Add(new HttpFileTemplate());
        Add(new ProgramTemplate());
        Add(new CrudWebApiExtensionsTemplate());
        Add(new JsonSerializerContextTemplate());

        AddTemplates(
            "Properties",
            new LaunchSettingsTemplate());

        AddTemplates(
            "Contracts/Data",
            new DtoTemplate());

        AddTemplates(
            "Contracts/Requests",
            new CreateRequestTemplate(),
            new DeleteRequestTemplate(),
            new GetRequestTemplate(),
            new UpdateRequestTemplate());

        AddTemplates(
            "Contracts/Responses",
            new GetAllResponseTemplate(),
            new ResponseTemplate());

        AddTemplates(
            "Database",
            new DatabaseInitializerTemplate(),
            new DbConnectionFactoryTemplate(),
            new DatabaseSeederTemplate());

        AddTemplates(
            "Domain",
            new DomainEntityTemplate(),
            new EnumTemplate());

        AddTemplates(
            "Endpoints",
            new CreateEndpointTemplate(),
            new DeleteEndpointTemplate(),
            new GetAllEndpointTemplate(),
            new GetEndpointTemplate(),
            new UpdateEndpointTemplate(),
            new GetByParentEndpointTemplate(),
            new HealthEndpointTemplate(),
            new ReadyEndpointTemplate());

        AddTemplates(
            "Mapping",
            new ApiContractToDomainMapperTemplate(),
            new DomainToApiContractMapperTemplate(),
            new DomainToDtoMapperTemplate(),
            new DtoToDomainMapperTemplate());

        AddTemplates(
            "Repositories",
            new RepositoryTemplate(),
            new RepositoryInterfaceTemplate());

        AddTemplates(
            "Services",
            new ServiceTemplate(),
            new ServiceInterfaceTemplate());

        AddTemplates(
            "Summaries",
            new CreateSummaryTemplate(),
            new DeleteSummaryTemplate(),
            new GetAllSummaryTemplate(),
            new GetSummaryTemplate(),
            new UpdateSummaryTemplate());

        AddTemplates(
            "Validation",
            new CreateRequestValidatorTemplate(),
            new UpdateRequestValidatorTemplate());

        AddTemplates(
            "Auth",
            new AuthExtensionsTemplate(),
            new TokenServiceInterfaceTemplate(),
            new TokenServiceTemplate(),
            new UserEntityTemplate(),
            new UserRepositoryInterfaceTemplate(),
            new UserRepositoryTemplate(),
            new UserInitializerTemplate(),
            new LoginEndpointTemplate(),
            new RegisterEndpointTemplate(),
            new LoginRequestTemplate(),
            new LoginResponseTemplate(),
            new RegisterRequestTemplate(),
            new RegisterResponseTemplate(),
            new LoginRequestValidatorTemplate(),
            new RegisterRequestValidatorTemplate());

        AddTemplates(
            "wwwroot",
            new IndexPageTemplate());

        Add(new DockerfileTemplate());
        Add(new DockerComposeTemplate());
        Add(new DockerIgnoreTemplate());
    }
}
