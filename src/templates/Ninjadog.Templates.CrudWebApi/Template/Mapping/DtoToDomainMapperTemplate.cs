namespace Ninjadog.Templates.CrudWebAPI.Template.Mapping;

/// <summary>
/// This template generates the DtoToDomainMapper class.
/// </summary>
public sealed class DtoToDomainMapperTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DtoToDomainMapper";

    /// <inheritdoc/>
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var entities = ninjadogSettings.Entities.FromKeys();
        var ns = $"{rootNamespace}.Mapping";
        const string className = "DtoToDomainMapper";
        const string fileName = $"{className}.cs";

        var methods = string.Join("\n", entities.Select(GenerateToModelMethods));

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

    private static string GenerateToModelMethods(NinjadogEntityWithKey entity)
    {
        var st = entity.StringTokens;
        var properties = PropertyMappingGenerator.GenerateToDomainMappings(entity, st.VarModelDto, 3, generateAutoKey: false);

        return $$"""

                     public static {{st.Model}} {{st.MethodToModel}}(this {{st.ClassModelDto}} {{st.VarModelDto}})
                     {
                         return new {{st.Model}}
                         {
                             {{properties}}
                         };
                     }
                 """;
    }
}
