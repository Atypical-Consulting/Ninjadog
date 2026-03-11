// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Templates.CrudWebAPI.Template.Domain;

/// <summary>
/// This template generates C# enum files based on the enums defined in the settings.
/// </summary>
public sealed class EnumTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "Enum";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        // This template generates multiple files, handled via GenerateMany
        return NinjadogContentFile.Empty;
    }

    /// <inheritdoc />
    public override IEnumerable<NinjadogContentFile> GenerateMany(NinjadogSettings ninjadogSettings)
    {
        var enums = ninjadogSettings.Enums;
        if (enums == null || enums.Count == 0)
        {
            yield break;
        }

        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var ns = $"{rootNamespace}.Domain";

        foreach (var (enumName, values) in enums)
        {
            var fileName = $"{enumName}.cs";
            var valuesStr = string.Join(",\n", values.Select(v => $"    {v}"));

            var content =
                $$"""

                  {{WriteFileScopedNamespace(ns)}}

                  public enum {{enumName}}
                  {
                  {{valuesStr}}
                  }
                  """;

            yield return CreateNinjadogContentFile(fileName, content);
        }
    }
}
