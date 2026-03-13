namespace Ninjadog.Templates.CrudWebAPI.Template.Mapping;

/// <summary>
/// This template generates the ApiContractToDomainMapper class.
/// </summary>
public sealed class ApiContractToDomainMapperTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "ApiContractToDomainMapper";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var entities = ninjadogSettings.Entities.FromKeys();
        var ns = $"{rootNamespace}.Mapping";
        const string className = "ApiContractToDomainMapper";
        const string fileName = $"{className}.cs";

        var toModelFromCreateMethods = string.Join("\n", entities.Select(GenerateToModelFromCreateMethods));
        var toModelFromUpdateMethods = string.Join("\n", entities.Select(GenerateToModelFromUpdateMethods));

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Requests;
              using {{rootNamespace}}.Domain;

              {{WriteFileScopedNamespace(ns)}}

              public static class {{className}}
              {
                  {{toModelFromCreateMethods}}
                  {{toModelFromUpdateMethods}}
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateToModelFromCreateMethods(NinjadogEntityWithKey entity)
    {
        var st = entity.StringTokens;
        var properties = PropertyMappingGenerator.GenerateToDomainMappings(entity, "request", 3, generateAutoKey: true);

        return $$"""

                     public static {{st.Model}} {{st.MethodToModel}}(this {{st.ClassCreateModelRequest}} request)
                     {
                         return new {{st.Model}}
                         {
                             {{properties}}
                         };
                     }
                 """;
    }

    private static string GenerateToModelFromUpdateMethods(NinjadogEntityWithKey entity)
    {
        var st = entity.StringTokens;
        var properties = PropertyMappingGenerator.GenerateToDomainMappings(entity, "request", 3, generateAutoKey: false);

        return
            $$"""

                  public static {{st.Model}} {{st.MethodToModel}}(this {{st.ClassUpdateModelRequest}} request)
                  {
                      return new {{st.Model}}
                      {
                          {{properties}}
                      };
                  }
              """;
    }
}
