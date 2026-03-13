// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

namespace Ninjadog.Templates.CrudWebAPI.Template.Contracts.Requests;

/// <summary>
/// This template generates the Get request for a given entity.
/// </summary>
public sealed class GetRequestTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "GetRequest";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOneByEntity(
        NinjadogEntityWithKey entity, string rootNamespace)
    {
        var st = entity.StringTokens;
        var ns = $"{rootNamespace}.Contracts.Requests";
        var fileName = $"{st.ClassGetModelRequest}.cs";
        var entityKey = entity.Properties.GetEntityKey();

        var content =
            $$"""

              {{WriteFileScopedNamespace(ns)}}

              /// <summary>
              ///     Request to get a {{st.Model}}.
              /// </summary>
              public partial class {{st.ClassGetModelRequest}}
              {
                  public {{entityKey.Type}} {{entityKey.PascalKey}} { get; init; }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
