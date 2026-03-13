namespace Ninjadog.Templates.CrudWebAPI.Template.Mapping;

/// <summary>
/// This template generates the DomainToApiContractMapper class.
/// </summary>
public sealed class DomainToApiContractMapperTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DomainToApiContractMapper";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var entities = ninjadogSettings.Entities.FromKeys();
        var ns = $"{rootNamespace}.Mapping";
        const string className = "DomainToApiContractMapper";
        const string fileName = $"{className}.cs";

        var toModelResponseMethods = string.Join("\n", entities.Select(GenerateToModelResponseMethods));
        var toModelsResponseMethods = string.Join("\n", entities.Select(GenerateToModelsResponseMethods));

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Responses;
              using {{rootNamespace}}.Domain;

              {{WriteFileScopedNamespace(ns)}}

              public static class {{className}}
              {
                  {{toModelsResponseMethods}}
                  {{toModelResponseMethods}}
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateToModelResponseMethods(NinjadogEntityWithKey entity)
    {
        var st = entity.StringTokens;
        var properties = PropertyMappingGenerator.GenerateFromDomainMappings(entity, st.VarModel, 3);

        return $$"""

                     public static {{st.ClassModelResponse}} {{st.MethodToModelResponse}}(this {{st.Model}} {{st.VarModel}})
                     {
                         return new {{st.ClassModelResponse}}
                         {
                             {{properties}}
                         };
                     }
                 """;
    }

    private static string GenerateToModelsResponseMethods(NinjadogEntityWithKey entity)
    {
        var st = entity.StringTokens;
        var properties = PropertyMappingGenerator.GenerateFromDomainMappings(entity, "x", 4);

        return $$"""

                     public static {{st.ClassGetAllModelsResponse}} {{st.MethodToModelsResponse}}(this IEnumerable<{{st.Model}}> {{st.VarModels}}, int page, int pageSize, int totalCount)
                     {
                         return new {{st.ClassGetAllModelsResponse}}
                         {
                             {{st.Models}} = {{st.VarModels}}.Select(x => new {{st.ClassModelResponse}}
                             {
                                 {{properties}}
                             }),
                             Page = page,
                             PageSize = pageSize,
                             TotalCount = totalCount
                         };
                     }
                 """;
    }
}
