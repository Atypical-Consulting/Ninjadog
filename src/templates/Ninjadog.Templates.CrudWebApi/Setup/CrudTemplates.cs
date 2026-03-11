// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using Ninjadog.Templates.CrudWebAPI.Template;
using Ninjadog.Templates.CrudWebAPI.Template.Contracts.Data;
using Ninjadog.Templates.CrudWebAPI.Template.Contracts.Requests;
using Ninjadog.Templates.CrudWebAPI.Template.Contracts.Responses;
using Ninjadog.Templates.CrudWebAPI.Template.Database;
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
        Add(new ProgramTemplate());
        Add(new CrudWebApiExtensionsTemplate());

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
            new UpdateEndpointTemplate());

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
    }
}
