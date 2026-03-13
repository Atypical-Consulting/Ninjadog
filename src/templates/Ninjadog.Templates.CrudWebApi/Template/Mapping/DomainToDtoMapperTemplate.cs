namespace Ninjadog.Templates.CrudWebAPI.Template.Mapping;

/// <summary>
/// This template generates the DomainToDtoMapper class.
/// </summary>
public sealed class DomainToDtoMapperTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DomainToDtoMapper";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var entities = ninjadogSettings.Entities.FromKeys();
        var ns = $"{rootNamespace}.Mapping";
        const string className = "DomainToDtoMapper";
        const string fileName = $"{className}.cs";

        var methods = string.Join("\n", entities.Select(GenerateToModelDtoMethods));

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Data;
              using {{rootNamespace}}.Domain;

              {{WriteFileScopedNamespace(ns)}}

              public static class {{className}}
              {
                  {{methods}}
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateToModelDtoMethods(NinjadogEntityWithKey entity)
    {
        var st = entity.StringTokens;
        var properties = PropertyMappingGenerator.GenerateFromDomainMappings(entity, st.VarModel, 3);

        return $$"""

                     public static {{st.ClassModelDto}} {{st.MethodToModelDto}}(this {{st.Model}} {{st.VarModel}})
                     {
                         return new {{st.ClassModelDto}}
                         {
                             {{properties}}
                         };
                     }
                 """;
    }
}
