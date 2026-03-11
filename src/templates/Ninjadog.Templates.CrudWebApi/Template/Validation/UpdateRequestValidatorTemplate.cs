// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Templates.CrudWebAPI.Template.Validation;

/// <summary>
/// This template generates the validation rules for the update request of a given entity.
/// </summary>
public sealed class UpdateRequestValidatorTemplate
    : NinjadogTemplate
{
    private static readonly HashSet<string> _valueTypes =
    [
        "Int32",
        "Decimal",
        "Boolean"
    ];

    /// <inheritdoc />
    public override string Name => "UpdateRequestValidator";

    /// <inheritdoc/>
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Validation";
        var fileName = $"{st.ClassUpdateModelRequestValidator}.cs";

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Requests;
              using FastEndpoints;
              using FluentValidation;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassUpdateModelRequestValidator}} : Validator<{{st.ClassUpdateModelRequest}}>
              {
                  public {{st.ClassUpdateModelRequestValidator}}()
                  {
              {{GenerateValidationRules(entity)}}
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }

    private static string GenerateValidationRules(NinjadogEntityWithKey entity)
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

            if (_valueTypes.Contains(propertyValue.Type))
            {
                continue;
            }

            if (hasWrittenRule)
            {
                sb.AppendLine();
            }

            sb.AppendLine($"RuleFor(x => x.{propertyName}).NotEmpty();");

            hasWrittenRule = true;
        }

        return sb.ToString();
    }
}
