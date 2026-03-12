using System.Globalization;

namespace Ninjadog.Templates.CrudWebAPI.Template;

/// <summary>
/// This template generates the .http file for testing API endpoints.
/// </summary>
public class HttpFileTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "HttpFile";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var appName = ninjadogSettings.Config.Name;
        var fileName = $"{appName}.CrudWebAPI.http";
        var entities = ninjadogSettings.Entities.FromKeys();

        var sections = new List<string>
        {
            $"@{appName}.CrudWebAPI_HostAddress = http://localhost:5111",
        };

        foreach (var entity in entities)
        {
            sections.Add(GenerateEntitySection(appName, entity));
        }

        var content = string.Join(Environment.NewLine + Environment.NewLine, sections);

        return CreateNinjadogContentFile(fileName, content, false);
    }

    private static string GenerateEntitySection(string appName, NinjadogEntityWithKey entity)
    {
        var st = entity.StringTokens;
        var entityKey = entity.Properties.GetEntityKey();
        var hostVar = $"{{{{{appName}.CrudWebAPI_HostAddress}}}}";
        var sampleId = GetSampleId(entityKey.Type);
        var nonKeyProperties = entity.Properties.FromKeys().Where(p => !p.IsKey).ToList();
        var jsonBody = GenerateJsonBody(nonKeyProperties);

        const string template =
            """
            # Get all {0}
            GET {1}{2}
            Accept: application/json

            ###

            # Get {3} by id
            GET {1}{2}/{4}
            Accept: application/json

            ###

            # Create a new {3}
            POST {1}{2}
            Content-Type: application/json

            {5}

            ###

            # Update a {3}
            PUT {1}{2}/{4}
            Content-Type: application/json

            {5}

            ###

            # Delete a {3}
            DELETE {1}{2}/{4}

            ###
            """;

        return string.Format(
            CultureInfo.InvariantCulture,
            template,
            st.ModelsHumanized,
            hostVar,
            st.ModelEndpoint,
            st.ModelHumanized,
            sampleId,
            jsonBody);
    }

    private static string GenerateJsonBody(List<NinjadogEntityPropertyWithKey> properties)
    {
        var lines = new List<string>();

        for (var i = 0; i < properties.Count; i++)
        {
            var prop = properties[i];
            var jsonKey = char.ToLowerInvariant(prop.Key[0]) + prop.Key[1..];
            var value = GetSampleJsonValue(prop.Type);
            var comma = i < properties.Count - 1 ? "," : string.Empty;
            lines.Add(string.Format(CultureInfo.InvariantCulture, "  \"{0}\": {1}{2}", jsonKey, value, comma));
        }

        return "{" + Environment.NewLine + string.Join(Environment.NewLine, lines) + Environment.NewLine + "}";
    }

    private static string GetSampleId(string typeName)
    {
        return typeName switch
        {
            "Guid" => "00000000-0000-0000-0000-000000000000",
            "Int32" => "1",
            "Int64" => "1",
            _ => "id"
        };
    }

    private static string GetSampleJsonValue(string typeName)
    {
        return typeName switch
        {
            "String" => "\"string\"",
            "Int32" => "0",
            "Int64" => "0",
            "Decimal" => "0.0",
            "Double" => "0.0",
            "Single" => "0.0",
            "Boolean" => "false",
            "Guid" => "\"00000000-0000-0000-0000-000000000000\"",
            "DateTime" => "\"2024-01-01T00:00:00\"",
            "DateOnly" => "\"2024-01-01\"",
            "TimeOnly" => "\"00:00:00\"",
            _ => "null"
        };
    }
}
