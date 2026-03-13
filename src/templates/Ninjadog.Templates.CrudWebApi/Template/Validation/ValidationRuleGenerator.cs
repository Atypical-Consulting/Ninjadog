namespace Ninjadog.Templates.CrudWebAPI.Template.Validation;

/// <summary>
/// Shared utility for generating FluentValidation rules from entity properties.
/// Used by both CreateRequestValidatorTemplate and UpdateRequestValidatorTemplate.
/// </summary>
internal static class ValidationRuleGenerator
{
    private static readonly HashSet<string> _valueTypes =
    [
        "Int32",
        "Decimal",
        "Boolean"
    ];

    internal static string GenerateValidationRules(NinjadogEntityWithKey entity)
    {
        var properties = entity.Properties;

        IndentedStringBuilder sb = new(2);
        var hasWrittenRule = false;

        foreach (var (propertyName, propertyValue) in properties)
        {
            if (propertyValue.IsKey)
            {
                continue;
            }

            var isValueType = _valueTypes.Contains(propertyValue.Type);
            var hasValidationAttrs = propertyValue.Required
                || propertyValue.MaxLength.HasValue
                || propertyValue.MinLength.HasValue
                || propertyValue.Min.HasValue
                || propertyValue.Max.HasValue
                || propertyValue.Pattern != null;

            // Skip value types that have no validation attributes
            if (isValueType && !hasValidationAttrs)
            {
                continue;
            }

            if (hasWrittenRule)
            {
                sb.AppendLine();
            }

            if (!isValueType && !hasValidationAttrs)
            {
                // Non-value-type properties without Required flag: skip validation
                continue;
            }

            // Generate rules based on validation attributes
            var rules = new List<string>();

            if (propertyValue.Required)
            {
                rules.Add($".NotEmpty().WithMessage(\"{propertyName} is required!\")");
            }

            if (propertyValue.MaxLength.HasValue)
            {
                rules.Add($".MaximumLength({propertyValue.MaxLength.Value})");
            }

            if (propertyValue.MinLength.HasValue)
            {
                rules.Add($".MinimumLength({propertyValue.MinLength.Value})");
            }

            if (propertyValue.Min.HasValue)
            {
                rules.Add($".GreaterThanOrEqualTo({propertyValue.Min.Value})");
            }

            if (propertyValue.Max.HasValue)
            {
                rules.Add($".LessThanOrEqualTo({propertyValue.Max.Value})");
            }

            if (propertyValue.Pattern != null)
            {
                rules.Add($".Matches(\"{propertyValue.Pattern}\")");
            }

            sb
                .AppendLine($"RuleFor(x => x.{propertyName})")
                .IncrementIndent();

            for (var i = 0; i < rules.Count; i++)
            {
                if (i == rules.Count - 1)
                {
                    sb.AppendLine($"{rules[i]};");
                }
                else
                {
                    sb.AppendLine(rules[i]);
                }
            }

            sb.DecrementIndent();

            hasWrittenRule = true;
        }

        return sb.ToString();
    }
}
