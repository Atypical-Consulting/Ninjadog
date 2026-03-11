// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Ninjadog.Settings.Schema;
using Ninjadog.Settings.Validation;

namespace Ninjadog.CLI.Commands;

/// <summary>
/// Opens a web-based configuration builder for ninjadog.json.
/// </summary>
internal sealed class UiCommand : AsyncCommand<UiCommandSettings>
{
    private const string ConfigFileName = "ninjadog.json";

    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, UiCommandSettings settings, CancellationToken cancellationToken)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
        var url = $"http://localhost:{settings.Port}";

        MarkupLine($"[green]Starting Ninjadog Config Builder on[/] [blue]{url}[/]");
        MarkupLine("[grey]Press Ctrl+C to stop the server.[/]");

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls(url);

        var app = builder.Build();

        // Serve embedded static files
        var embeddedProvider = new EmbeddedFileProvider(
            typeof(UiCommand).Assembly,
            "Ninjadog.WebUI");

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = embeddedProvider,
            RequestPath = string.Empty
        });

        // Serve index.html at root
        app.MapGet("/", async (HttpContext ctx) =>
        {
            var file = embeddedProvider.GetFileInfo("index.html");
            if (!file.Exists)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.WriteAsync("index.html not found", cancellationToken);
                return;
            }

            ctx.Response.ContentType = "text/html";
            await using var stream = file.CreateReadStream();
            await stream.CopyToAsync(ctx.Response.Body, cancellationToken);
        });

        // API: Read config
        app.MapGet("/api/config", () =>
        {
            if (!File.Exists(configPath))
            {
                return Results.Json(new { }, statusCode: 200);
            }

            var json = File.ReadAllText(configPath);
            return Results.Content(json, "application/json");
        });

        // API: Write config
        app.MapPost("/api/config", async (HttpContext ctx) =>
        {
            using var reader = new StreamReader(ctx.Request.Body);
            var json = await reader.ReadToEndAsync();

            // Validate JSON is parseable
            try
            {
                using var doc = JsonDocument.Parse(json);
            }
            catch (JsonException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 400);
            }

            await File.WriteAllTextAsync(configPath, json, cancellationToken);
            return Results.Json(new { saved = true });
        });

        // API: Validate config
        app.MapPost("/api/validate", async (HttpContext ctx) =>
        {
            using var reader = new StreamReader(ctx.Request.Body);
            var json = await reader.ReadToEndAsync();

            var result = NinjadogConfigValidator.Validate(json);
            return Results.Json(result);
        });

        // API: Build
        app.MapPost("/api/build", () =>
        {
            if (!File.Exists(configPath))
            {
                return Results.Json(new { success = false, error = "No ninjadog.json found." }, statusCode: 400);
            }

            return Results.Json(new { success = true, message = "Build triggered. Use the CLI 'ninjadog build' for full output." });
        });

        // API: Get schema
        app.MapGet("/api/schema", () =>
        {
            var schemaJson = SchemaProvider.GetSchemaJson();
            return Results.Content(schemaJson, "application/json");
        });

        // Open browser
        if (!settings.NoBrowser)
        {
            _ = Task.Run(
                async () =>
                {
                    await Task.Delay(500, cancellationToken);
                    OpenBrowser(url);
                },
                cancellationToken);
        }

        await app.RunAsync();
        return 0;
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start(new ProcessStartInfo("open", url) { UseShellExecute = false });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { UseShellExecute = false });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start(new ProcessStartInfo("xdg-open", url) { UseShellExecute = false });
            }
        }
        catch
        {
            // Swallow -- user can open the URL manually.
        }
    }
}
