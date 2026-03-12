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
                case "Guid":
                    sb.Append("Guid.NewGuid()");
                    break;
                case "DateOnly":
                    sb.Append($"DateOnly.FromDateTime(request.{p.Key})");
                    break;
                default:
                    sb.Append($"request.{p.Key}");
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

        return $$"""

                     public static {{st.Model}} {{st.MethodToModel}}(this {{st.ClassCreateModelRequest}} request)
                     {
                         return new {{st.Model}}
                         {
                             {{sb}}
                         };
                     }
                 """;
    }

    private static string GenerateToModelFromUpdateMethods(NinjadogEntityWithKey entity)
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
                    sb.Append($"DateOnly.FromDateTime(request.{p.Key})");
                    break;
                default:
                    sb.Append($"request.{p.Key}");
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

        return
            $$"""

                  public static {{st.Model}} {{st.MethodToModel}}(this {{st.ClassUpdateModelRequest}} request)
                  {
                      return new {{st.Model}}
                      {
                          {{sb}}
                      };
                  }
              """;
    }
}
