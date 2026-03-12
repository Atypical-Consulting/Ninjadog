namespace Ninjadog.Templates.CrudWebAPI.Template.Contracts.Data;

/// <summary>
/// This template generates the DTO class for a given entity.
/// </summary>
public sealed class DtoTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DataTransferObject";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Contracts.Data";
        var fileName = $"{st.ClassModelDto}.cs";

        var content =
            $$"""

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassModelDto}}
              {
              {{GenerateDtoProperties(entity)}}
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    /// <summary>
    /// Generates DTO properties with database-friendly types.
    /// Guid is mapped to string (stored as TEXT in SQLite) and DateOnly is mapped to DateTime.
    /// </summary>
    private static string GenerateDtoProperties(NinjadogEntityWithKey entity)
    {
        return entity.Properties
            .FromKeys()
            .Select(GenerateDtoProperty)
            .Aggregate((x, y) => $"{x}\n{y}");
    }

    private static string GenerateDtoProperty(NinjadogEntityPropertyWithKey p)
    {
        var dtoType = p.Type switch
        {
            "Guid" => "string",
            "DateOnly" => "DateTime",
            _ => p.Type
        };

        IndentedStringBuilder sb = new(1);
        sb.Append($"public {dtoType} {p.Key} {{ get; init; }}");

        if (dtoType == "string")
        {
            sb.Append(" = default!;");
        }

        sb.AppendLine();
        return sb.ToString();
    }
}
