namespace Ninjadog.Settings.Extensions.Entities.Properties;

/// <summary>
/// Represents a Ninjadog entity property along with its key.
/// This record extends <see cref="NinjadogEntityProperty"/> by associating it with a specific key,
/// providing additional context and utility for template generation.
/// </summary>
/// <param name="Key">The key associated with the property.</param>
/// <param name="Property">The entity property.</param>
public sealed record NinjadogEntityPropertyWithKey(
    string Key, NinjadogEntityProperty Property)
    : NinjadogEntityProperty(Property)
{
    /// <summary>
    /// Gets the PascalCase version of the key, suitable for C# property names and SQL column names.
    /// </summary>
    public string PascalKey => Key.UppercaseFirst();

    /// <summary>
    /// Generates the member property definition for the associated entity property.
    /// This method produces the C# code representation of the property, including its type and accessors.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the C# code for the member property.</returns>
    public string GenerateMemberProperty()
    {
        // var baseTypeName = p.Type.BaseType?.Name;
        // var isValueOf = baseTypeName is "ValueOf";
        // var valueOfArgument = p.Type.BaseType?.TypeArguments.FirstOrDefault()?.ToString() ?? "";
        //
        // var realType = isValueOf
        //     ? valueOfArgument
        //     : p.Type.ToString();
        //
        // var propertyType = realType switch
        // {
        //     "System.Guid" => "string",
        //     "System.DateOnly" => "DateTime",
        //     _ => realType
        // };
        IndentedStringBuilder sb = new(1);

        sb.Append($"public {Type} {PascalKey} {{ get; init; }}");

        if (Type == "string")
        {
            sb.Append(" = default!;");
        }

        sb.AppendLine();

        return sb.ToString();
    }
}
