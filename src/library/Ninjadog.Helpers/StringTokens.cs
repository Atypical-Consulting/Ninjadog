namespace Ninjadog.Helpers;

/// <summary>
/// Provides a set of naming conventions derived from a base model name for use in various parts of the code generation process.
/// This record takes a PascalCase model name and generates various naming formats like camelCase, plural forms,
/// human-readable formats, and specific patterns for DTOs, services, repositories, and API endpoints.
/// </summary>
/// <param name="Pascal">The PascalCase name of the model from which to derive other naming formats.</param>
public sealed record StringTokens(string Pascal)
    : NamingConventionBase(Pascal)
{
    /// <summary>
    /// Gets the original PascalCase model name.
    /// </summary>
    public string Model
        => Pascal;

    /// <summary>
    /// Gets the humanized, lowercase format of the model name.
    /// </summary>
    public string ModelHumanized
        => Humanized;

    /// <summary>
    /// Gets the endpoint name in dashed format, typically used for URL paths.
    /// </summary>
    public string ModelEndpoint
        => $"/{DashedPlural}";

    /// <summary>
    /// Gets the camelCase variant of the model name, commonly used for variable names.
    /// </summary>
    public string VarModel
        => Camel;

    /// <summary>
    /// Gets the PascalCase name prefixed with 'existing', used for distinguishing between new and existing instances.
    /// </summary>
    public string VarExistingModel
        => $"existing{Pascal}";

    /// <summary>
    /// Gets the pluralized PascalCase name of the model, used for collections or groups of the entity.
    /// </summary>
    public string Models
        => PascalPlural;

    /// <summary>
    /// Gets the humanized and pluralized lowercased name of the model, for readable representations of collections.
    /// </summary>
    public string ModelsHumanized
        => HumanizedPlural;

    /// <summary>
    /// Gets the pluralized camelCase name of the model, used for variables representing collections of the model.
    /// </summary>
    public string VarModels
        => CamelPlural;

    /// <summary>
    /// Gets the name of the model's DTO class, following the pattern [ModelName]Dto.
    /// </summary>
    public string ClassModelDto
        => $"{Pascal}Dto";

    /// <summary>
    /// Gets the camelCase variable name for the model's DTO.
    /// </summary>
    public string VarModelDto
        => $"{Camel}Dto";

    /// <summary>
    /// Gets the pluralized camelCase variable name for a collection of the model's DTOs.
    /// </summary>
    public string VarModelDtos
        => $"{Camel}Dtos";

    /// <summary>
    /// Gets the name of the class used for the model's response, following the pattern [ModelName]Response.
    /// </summary>
    public string ClassModelResponse
        => $"{Pascal}Response";

    /// <summary>
    /// Gets the camelCase variable name for the model's response.
    /// </summary>
    public string VarModelResponse
        => $"{Camel}Response";

    /// <summary>
    /// Gets the name of the class used for the response of a collection of models, following the pattern GetAll[ModelName]Response.
    /// </summary>
    public string ClassGetAllModelsResponse
        => $"GetAll{PascalPlural}Response";

    /// <summary>
    /// Gets the camelCase variable name for the response of a collection of models.
    /// </summary>
    public string VarModelsResponse
        => $"{CamelPlural}Response";

    /// <summary>
    /// Gets the interface name for the model's repository, following the pattern I[ModelName]Repository.
    /// </summary>
    public string InterfaceModelRepository
        => $"I{Pascal}Repository";

    /// <summary>
    /// Gets the class name for the model's repository, following the pattern [ModelName]Repository.
    /// </summary>
    public string ClassModelRepository
        => $"{Pascal}Repository";

    /// <summary>
    /// Gets the name of the private repository field in camel case prefixed with an underscore.
    /// </summary>
    public string FieldModelRepository
        => $"_{Camel}Repository";

    /// <summary>
    /// Gets the name of the repository variable in camel case.
    /// </summary>
    public string VarModelRepository
        => $"{Camel}Repository";

    /// <summary>
    /// Gets the name of the service interface in Pascal case prefixed with 'I'.
    /// </summary>
    public string InterfaceModelService
        => $"I{Pascal}Service";

    /// <summary>
    /// Gets the name of the service class in Pascal case.
    /// </summary>
    public string ClassModelService
        => $"{Pascal}Service";

    /// <summary>
    /// Gets the name of the service property in Pascal case.
    /// </summary>
    public string PropertyModelService
        => $"{Pascal}Service";

    /// <summary>
    /// Gets the name of the private service field in camel case prefixed with an underscore.
    /// </summary>
    public string FieldModelService
        => $"_{Camel}Service";

    /// <summary>
    /// Gets the name of the service variable in camel case.
    /// </summary>
    public string VarModelService
        => $"{Camel}Service";

    /// <summary>
    /// Gets the name of the class representing a summary of all models in Pascal case.
    /// </summary>
    public string ClassGetAllModelsSummary
        => $"GetAll{PascalPlural}Summary";

    /// <summary>
    /// Gets the name of the endpoint class for retrieving all models in Pascal case.
    /// </summary>
    public string ClassGetAllModelsEndpoint
        => $"GetAll{PascalPlural}Endpoint";

    /// <summary>
    /// Gets the name of the class representing a summary of a single model in Pascal case.
    /// </summary>
    public string ClassGetModelSummary
        => $"Get{Pascal}Summary";

    /// <summary>
    /// Gets the name of the class representing a request to get a single model in Pascal case.
    /// </summary>
    public string ClassGetModelRequest
        => $"Get{Pascal}Request";

    /// <summary>
    /// Gets the name of the endpoint class for retrieving a single model in Pascal case.
    /// </summary>
    public string ClassGetModelEndpoint
        => $"Get{Pascal}Endpoint";

    /// <summary>
    /// Gets the name of the class representing a summary for deleting a model in Pascal case.
    /// </summary>
    public string ClassDeleteModelSummary
        => $"Delete{Pascal}Summary";

    /// <summary>
    /// Gets the name of the class representing a request to delete a model in Pascal case.
    /// </summary>
    public string ClassDeleteModelRequest
        => $"Delete{Pascal}Request";

    /// <summary>
    /// Gets the name of the endpoint class for deleting a model in Pascal case.
    /// </summary>
    public string ClassDeleteModelEndpoint
        => $"Delete{Pascal}Endpoint";

    /// <summary>
    /// Gets the name of the class representing a summary for creating a model in Pascal case.
    /// </summary>
    public string ClassCreateModelSummary
        => $"Create{Pascal}Summary";

    /// <summary>
    /// Gets the name of the class representing a request to create a model in Pascal case.
    /// </summary>
    public string ClassCreateModelRequest
        => $"Create{Pascal}Request";

    /// <summary>
    /// Gets the name of the class representing a validator for a request to create a model in Pascal case.
    /// </summary>
    public string ClassCreateModelRequestValidator
        => $"Create{Pascal}RequestValidator";

    /// <summary>
    /// Gets the name of the endpoint class for creating a model in Pascal case.
    /// </summary>
    public string ClassCreateModelEndpoint
        => $"Create{Pascal}Endpoint";

    /// <summary>
    /// Gets the name of the class representing a summary for updating a model in Pascal case.
    /// </summary>
    public string ClassUpdateModelSummary
        => $"Update{Pascal}Summary";

    /// <summary>
    /// Gets the name of the class representing a request to update a model in Pascal case.
    /// </summary>
    public string ClassUpdateModelRequest
        => $"Update{Pascal}Request";

    /// <summary>
    /// Gets the name of the class representing a validator for a request to update a model in Pascal case.
    /// </summary>
    public string ClassUpdateModelRequestValidator
        => $"Update{Pascal}RequestValidator";

    /// <summary>
    /// Gets the name of the endpoint class for updating a model in Pascal case.
    /// </summary>
    public string ClassUpdateModelEndpoint
        => $"Update{Pascal}Endpoint";

    /// <summary>
    /// Gets the method name for converting to the model type in Pascal case.
    /// </summary>
    public string MethodToModel
        => $"To{Pascal}";

    /// <summary>
    /// Gets the method name for converting to the model DTO (Data Transfer Object) type in Pascal case.
    /// </summary>
    public string MethodToModelDto
        => $"To{Pascal}Dto";

    /// <summary>
    /// Gets the method name for converting to the model response type in Pascal case.
    /// </summary>
    public string MethodToModelResponse
        => $"To{Pascal}Response";

    /// <summary>
    /// Gets the method name for converting to the models response type in plural Pascal case.
    /// </summary>
    public string MethodToModelsResponse
        => $"To{PascalPlural}Response";
}
