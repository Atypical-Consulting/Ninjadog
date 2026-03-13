namespace Ninjadog.Templates.CrudWebAPI.Template.Mapping;

/// <summary>
/// Shared utility for generating property mapping expressions used across mapper templates.
/// Handles type conversions for ValueOf, DateOnly, and Guid types.
/// </summary>
internal static class PropertyMappingGenerator
{
    /// <summary>
    /// Generates property assignment expressions for mapping from a source to domain model.
    /// Used when the source is a request/DTO and the target is a domain model.
    /// </summary>
    /// <param name="entity">The entity to generate mappings for.</param>
    /// <param name="sourcePrefix">The prefix for accessing source properties (e.g., "request", "todoItemDto").</param>
    /// <param name="indent">The base indentation level.</param>
    /// <param name="generateAutoKey">Whether to generate auto-key values (e.g., Guid.NewGuid()).</param>
    internal static string GenerateToDomainMappings(
        NinjadogEntityWithKey entity,
        string sourcePrefix,
        byte indent,
        bool generateAutoKey)
    {
        var modelProperties = entity.Properties.FromKeys();
        IndentedStringBuilder sb = new(indent);

        for (var i = 0; i < modelProperties.Count; i++)
        {
            var isLastItem = i == modelProperties.Count - 1;
            var p = modelProperties[i];
            var isValueOf = p.Type is "ValueOf";
            var valueOfArgument = p.Type ?? string.Empty;

            sb.Append($"{p.Key} = ");

            if (isValueOf)
            {
                sb.Append($"{p.Type}.From(");
            }

            var realType = isValueOf ? valueOfArgument : p.Type;

            switch (realType)
            {
                case "Guid" when generateAutoKey && p.IsKey:
                    sb.Append("Guid.NewGuid()");
                    break;
                case "DateOnly":
                    sb.Append($"DateOnly.FromDateTime({sourcePrefix}.{p.Key})");
                    break;
                default:
                    sb.Append($"{sourcePrefix}.{p.Key}");
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

        return sb.ToString().TrimStart();
    }

    /// <summary>
    /// Generates property assignment expressions for mapping from a domain model to a response/DTO.
    /// Handles ValueOf unwrapping and DateOnly conversion.
    /// </summary>
    /// <param name="entity">The entity to generate mappings for.</param>
    /// <param name="sourcePrefix">The prefix for accessing source properties (e.g., "todoItem", "x").</param>
    /// <param name="indent">The base indentation level.</param>
    internal static string GenerateFromDomainMappings(
        NinjadogEntityWithKey entity,
        string sourcePrefix,
        byte indent)
    {
        var modelProperties = entity.Properties.FromKeys();
        IndentedStringBuilder sb = new(indent);

        for (var i = 0; i < modelProperties.Count; i++)
        {
            var isLastItem = i == modelProperties.Count - 1;
            var p = modelProperties[i];
            var isValueOf = p.Type is "ValueOf";
            var valueOfArgument = p.Type ?? string.Empty;

            sb.Append($"{p.Key} = {sourcePrefix}.{p.Key}");

            var realType = p.Type;

            if (isValueOf)
            {
                sb.Append(".Value");
                realType = valueOfArgument;
            }

            switch (realType)
            {
                case "DateOnly":
                    sb.Append(".ToDateTime(TimeOnly.MinValue)");
                    break;
            }

            if (!isLastItem)
            {
                sb.AppendLine(",");
            }
        }

        return sb.ToString().TrimStart();
    }
}
