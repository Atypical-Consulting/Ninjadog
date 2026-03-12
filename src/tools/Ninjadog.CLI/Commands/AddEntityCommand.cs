using System.Text.Json;

namespace Ninjadog.CLI.Commands;

internal sealed class AddEntityCommand
    : Command<AddEntityCommandSettings>
{
    public override int Execute(CommandContext context, AddEntityCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            const string settingsFileName = "ninjadog.json";
            var settingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), settingsFileName);

            if (!File.Exists(settingsFilePath))
            {
                MarkupLine("[red]Error:[/] No ninjadog.json found. Run [green]ninjadog init[/] first.");
                return 1;
            }

            var json = File.ReadAllText(settingsFilePath);
            using var doc = JsonDocument.Parse(json);

            // Rebuild JSON with the new entity added
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

            writer.WriteStartObject();

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name == "entities")
                {
                    writer.WritePropertyName("entities");
                    writer.WriteStartObject();

                    // Copy existing entities
                    foreach (var entity in prop.Value.EnumerateObject())
                    {
                        writer.WritePropertyName(entity.Name);
                        entity.Value.WriteTo(writer);
                    }

                    // Add new entity with default Guid Id
                    writer.WritePropertyName(settings.EntityName);
                    writer.WriteStartObject();
                    writer.WritePropertyName("properties");
                    writer.WriteStartObject();

                    writer.WritePropertyName("Id");
                    writer.WriteStartObject();
                    writer.WriteString("type", "Guid");
                    writer.WriteBoolean("isKey", true);
                    writer.WriteEndObject();

                    writer.WriteEndObject(); // properties
                    writer.WriteEndObject(); // entity

                    writer.WriteEndObject(); // entities
                }
                else
                {
                    prop.WriteTo(writer);
                }
            }

            // If no entities section existed, create one
            if (!doc.RootElement.TryGetProperty("entities", out _))
            {
                writer.WritePropertyName("entities");
                writer.WriteStartObject();

                writer.WritePropertyName(settings.EntityName);
                writer.WriteStartObject();
                writer.WritePropertyName("properties");
                writer.WriteStartObject();

                writer.WritePropertyName("Id");
                writer.WriteStartObject();
                writer.WriteString("type", "Guid");
                writer.WriteBoolean("isKey", true);
                writer.WriteEndObject();

                writer.WriteEndObject(); // properties
                writer.WriteEndObject(); // entity
                writer.WriteEndObject(); // entities
            }

            writer.WriteEndObject();
            writer.Flush();

            var updatedJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            File.WriteAllText(settingsFilePath, updatedJson);

            MarkupLine($"[green]Entity '{settings.EntityName.EscapeMarkup()}' added successfully.[/]");
            return 0;
        }
        catch (Exception e)
        {
            WriteLine();
            WriteException(e);
            return 1;
        }
    }
}
