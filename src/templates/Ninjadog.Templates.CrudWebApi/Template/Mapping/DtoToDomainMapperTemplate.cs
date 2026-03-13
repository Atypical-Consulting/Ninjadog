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
        var modelProperties = entity.Properties.FromKeys();

        IndentedStringBuilder sb = new(3);

        for (var i = 0; i < modelProperties.Count; i++)
        {
            var isLastItem = i == modelProperties.Count - 1;
            var p = modelProperties[i];
            var baseTypeName = p.Type;
            var isValueOf = baseTypeName is "ValueOf";
            var valueOfArgument = p.Type ?? string.Empty;

            sb.Append($"{p.Key} = ");

            if (isValueOf)
            {
                sb.Append($"{p.Type}.From(");
            }

            var realType = isValueOf
                ? valueOfArgument
                : p.Type;

            switch (realType)
            {
                case "DateOnly":
                    sb.Append($"DateOnly.FromDateTime({st.VarModelDto}.{p.Key})");
                    break;
                default:
                    sb.Append($"{st.VarModelDto}.{p.Key}");
                    break;
            }

            if (isValueOf)
            {
                sb.Append(")");
            }

            if (!isLastItem)
            {
                sb.AppendLine(",");
            }
        }

        var properties = sb.ToString().TrimStart();

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
