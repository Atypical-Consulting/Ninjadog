// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Templates.CrudWebAPI.Template.Validation;

/// <summary>
/// This template generates the validation rules for the create request of a given entity.
/// </summary>
public sealed class CreateRequestValidatorTemplate
    : NinjadogTemplate
{
    private static readonly HashSet<string> _valueTypes =
    [
        "Int32",
        "Decimal",
        "Boolean"
    ];

    /// <inheritdoc />
    public override string Name => "CreateRequestValidator";

    /// <inheritdoc/>
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Validation";
        var fileName = $"{st.ClassCreateModelRequestValidator}.cs";

        var content =
            $$"""

              using {{rootNamespace}}.Contracts.Requests;
              using FastEndpoints;
              using FluentValidation;

              {{WriteFileScopedNamespace(ns)}}

              public partial class {{st.ClassCreateModelRequestValidator}} : Validator<{{st.ClassCreateModelRequest}}>
              {
                  public {{st.ClassCreateModelRequestValidator}}()
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

        IndentedStringBuilder stringBuilder = new(2);
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
                stringBuilder.AppendLine();
            }

            stringBuilder
                .AppendLine($"RuleFor(x => x.{propertyName})")
                .IncrementIndent()
                .AppendLine(".NotEmpty()")
                .AppendLine($".WithMessage(\"{propertyName} is required!\");")
                .DecrementIndent();

            hasWrittenRule = true;
        }

        return stringBuilder.ToString();
    }
}
