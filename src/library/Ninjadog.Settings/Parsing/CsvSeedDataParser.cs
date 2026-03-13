namespace Ninjadog.Settings.Parsing;

/// <summary>
/// Parses CSV files into seed data dictionaries for entity seeding.
/// </summary>
internal static class CsvSeedDataParser
{
    internal static List<Dictionary<string, object>> ParseCsvFile(string csvPath, string? basePath)
    {
        var resolvedPath = basePath is not null
            ? Path.Combine(basePath, csvPath)
            : csvPath;

        string[] lines;
        try
        {
            lines = File.ReadAllLines(resolvedPath);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new InvalidOperationException($"Seed data CSV file not found: '{resolvedPath}'.", ex);
        }

        if (lines.Length == 0)
        {
            throw new InvalidOperationException($"Seed data CSV file '{csvPath}' is empty.");
        }

        var headers = ParseCsvLine(lines[0]);
        var seedData = new List<Dictionary<string, object>>();

        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var values = ParseCsvLine(line);
            if (values.Length != headers.Length)
            {
                throw new InvalidOperationException($"Seed data CSV file '{csvPath}' row {i + 1} has {values.Length} fields but header has {headers.Length}.");
            }

            var row = new Dictionary<string, object>();
            for (var j = 0; j < headers.Length; j++)
            {
                row[headers[j]] = ParseCsvValue(values[j]);
            }

            seedData.Add(row);
        }

        return seedData;
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString().Trim());

        return [.. fields];
    }

    private static object ParseCsvValue(string value)
    {
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ? true :
            string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) ? false :
            int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var intVal) ? intVal :
            decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var decVal) ? decVal :
            value;
    }
}
