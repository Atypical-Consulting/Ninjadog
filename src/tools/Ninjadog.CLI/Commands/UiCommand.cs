using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ninjadog.CLI.AI;
using Ninjadog.Evolution;
using Ninjadog.Evolution.Migrations;
using Ninjadog.Settings.Schema;
using Ninjadog.Settings.Validation;

namespace Ninjadog.CLI.Commands;

/// <summary>
/// Opens a web-based configuration builder for ninjadog.json.
/// </summary>
internal sealed class UiCommand : AsyncCommand<UiCommandSettings>
{
    private const string ConfigFileName = "ninjadog.json";
    private const int MaxPortRetries = 10;

    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, UiCommandSettings settings, CancellationToken cancellationToken)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
        var port = FindAvailablePort(settings.Port);

        if (port < 0)
        {
            MarkupLine($"[red]Could not find an available port (tried {settings.Port}–{settings.Port + MaxPortRetries - 1}).[/]");
            MarkupLine("[grey]Hint: stop the other process or specify a different port with --port.[/]");
            return 1;
        }

        var url = $"http://localhost:{port}";

        if (port != settings.Port)
        {
            MarkupLine($"[yellow]Port {settings.Port} is already in use, using port {port} instead.[/]");
        }

        MarkupLine($"[green]Starting Ninjadog Config Builder on[/] [blue]{url}[/]");
        MarkupLine("[grey]Press Ctrl+C to stop the server.[/]");

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls(url);

        // Suppress all ASP.NET infrastructure logs — we use Spectre.Console for output
        builder.Logging.ClearProviders();

        var app = builder.Build();

        // Serve embedded static files (Vite build output in WebUI/dist)
        var embeddedProvider = new EmbeddedFileProvider(
            typeof(UiCommand).Assembly,
            "Ninjadog.CLI.WebUI.dist");

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = embeddedProvider,
            RequestPath = string.Empty
        });

        // SPA catch-all: serve index.html for any non-API GET request
        // This enables client-side routing (React Router) to work with clean URLs
        app.MapFallback(async ctx =>
        {
            // Only serve index.html for GET requests that aren't API calls
            if (ctx.Request.Method != "GET" || ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = 404;
                return;
            }

            var file = embeddedProvider.GetFileInfo("index.html");
            if (!file.Exists)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.WriteAsync("index.html not found");
                return;
            }

            ctx.Response.ContentType = "text/html";
            await using var stream = file.CreateReadStream();
            await stream.CopyToAsync(ctx.Response.Body, ctx.RequestAborted);
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
            var json = await ReadBodyAsync(ctx);

            // Validate JSON is parseable
            try
            {
                using var doc = JsonDocument.Parse(json);
            }
            catch (JsonException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 400);
            }

            await File.WriteAllTextAsync(configPath, json, ctx.RequestAborted);
            return Results.Json(new { saved = true });
        });

        // API: Validate config
        app.MapPost("/api/validate", async (HttpContext ctx) =>
        {
            var json = await ReadBodyAsync(ctx);

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
            var schemaJson = SchemaProvider.GetSchemaText();
            return Results.Content(schemaJson, "application/json");
        });

        // API: Generate config from natural language using AI
        app.MapPost("/api/generate", async (HttpContext ctx) =>
        {
            var body = await ReadBodyAsync(ctx);

            List<ChatMessage> messages;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var messagesArray = doc.RootElement.GetProperty("messages");
                messages = [.. messagesArray.EnumerateArray()
                    .Select(m => new ChatMessage(
                        m.GetProperty("role").GetString()!,
                        m.GetProperty("content").GetString()!))];
            }
            catch (Exception ex)
            {
                return Results.Json(
                    new { success = false, error = $"Invalid request: {ex.Message}" },
                    statusCode: 400);
            }

            var result = await ConfigGenerator.GenerateAsync(messages, ctx.RequestAborted);

            return Results.Json(new
            {
                result.Success,
                result.Json,
                result.Error,
                Validation = result.Validation is not null
                    ? new
                    {
                        result.Validation.IsValid,
                        Diagnostics = result.Validation.Diagnostics.Select(d => new
                        {
                            d.Path,
                            d.Message,
                            Severity = d.Severity.ToString().ToLowerInvariant()
                        })
                    }
                    : null
            });
        });

        // API: List directories for folder picker
        app.MapGet("/api/directories", (string? path) =>
        {
            var basePath = Directory.GetCurrentDirectory();
            var targetPath = string.IsNullOrWhiteSpace(path) || path == "."
                ? basePath
                : Path.IsPathRooted(path) ? path : Path.Combine(basePath, path);

            try
            {
                var resolved = Path.GetFullPath(targetPath);
                if (!Directory.Exists(resolved))
                {
                    return Results.Json(new { error = "Directory not found" }, statusCode: 404);
                }

                var dirs = Directory.GetDirectories(resolved)
                    .Where(d => !Path.GetFileName(d).StartsWith('.'))
                    .Select(d => Path.GetFileName(d))
                    .OrderBy(d => d)
                    .ToArray();

                var parent = Path.GetDirectoryName(resolved);
                var relativeToCwd = Path.GetRelativePath(basePath, resolved);
                if (relativeToCwd == ".")
                {
                    relativeToCwd = ".";
                }

                return Results.Json(new
                {
                    current = relativeToCwd,
                    absolute = resolved,
                    parent = parent != null ? Path.GetRelativePath(basePath, parent) : null,
                    directories = dirs
                });
            }
            catch
            {
                return Results.Json(new { error = "Cannot access directory" }, statusCode: 400);
            }
        });

        // API: Preview schema evolution (diff only, no file writes)
        app.MapPost("/api/evolve/preview", async (HttpContext ctx) =>
        {
            var currentJson = await ReadBodyAsync(ctx);

            var projectRoot = Directory.GetCurrentDirectory();

            if (!SchemaState.Exists(projectRoot))
            {
                return Results.Json(new { hasBaseline = false });
            }

            var previousJson = SchemaState.Load(projectRoot)!;
            NinjadogSettings previousSettings;
            NinjadogSettings currentSettings;

            try
            {
                previousSettings = NinjadogSettings.FromJsonString(previousJson, projectRoot);
                currentSettings = NinjadogSettings.FromJsonString(currentJson, projectRoot);
            }
            catch (JsonException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 400);
            }

            var diff = SchemaDiffer.Compare(previousSettings, currentSettings);
            var operations = MigrationSqlGenerator.Generate(diff, currentSettings);

            return Results.Json(new
            {
                hasBaseline = true,
                hasChanges = diff.HasChanges,
                entities = diff.EntityChanges.Select(e => new
                {
                    key = e.EntityKey,
                    kind = e.Kind.ToString(),
                    properties = e.PropertyChanges.Select(p => new
                    {
                        name = p.PropertyName,
                        kind = p.Kind.ToString(),
                        typeChanged = p.TypeChanged,
                        beforeType = p.Before?.Type,
                        afterType = p.After?.Type,
                    }),
                    relationships = e.RelationshipChanges.Select(r => new
                    {
                        name = r.RelationshipName,
                        kind = r.Kind.ToString(),
                    }),
                }),
                config = new
                {
                    diff.ConfigChanges.HasChanges,
                    diff.ConfigChanges.SoftDeleteChanged,
                    diff.ConfigChanges.SoftDeleteEnabled,
                    diff.ConfigChanges.AuditingChanged,
                    diff.ConfigChanges.AuditingEnabled,
                    diff.ConfigChanges.DatabaseProviderChanged,
                    diff.ConfigChanges.OldDatabaseProvider,
                    diff.ConfigChanges.NewDatabaseProvider,
                    diff.ConfigChanges.AotChanged,
                    diff.ConfigChanges.CorsChanged,
                    diff.ConfigChanges.AuthChanged,
                    diff.ConfigChanges.RateLimitChanged,
                    diff.ConfigChanges.VersioningChanged,
                },
                enums = diff.EnumChanges.Select(e => new
                {
                    name = e.EnumName,
                    kind = e.Kind.ToString(),
                    addedValues = e.AddedValues,
                    removedValues = e.RemovedValues,
                }),
                operations = operations.Select(o => new
                {
                    o.Description,
                    o.Sql,
                    o.IsWarning,
                }),
            });
        });

        // API: Apply schema evolution (save baseline + write migration file)
        app.MapPost("/api/evolve/apply", async (HttpContext ctx) =>
        {
            using var reader = new StreamReader(ctx.Request.Body);
            var body = await reader.ReadToEndAsync(ctx.RequestAborted);
            string currentJson;
            string? migrationName = null;

            try
            {
                using var doc = JsonDocument.Parse(body);
                currentJson = doc.RootElement.GetProperty("config").GetRawText();
                if (doc.RootElement.TryGetProperty("name", out var nameElement))
                {
                    migrationName = nameElement.GetString();
                }
            }
            catch (JsonException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 400);
            }

            var projectRoot = Directory.GetCurrentDirectory();

            if (!SchemaState.Exists(projectRoot))
            {
                SchemaState.Save(projectRoot, currentJson);
                return Results.Json(new { firstRun = true, message = "Baseline saved." });
            }

            var previousJson = SchemaState.Load(projectRoot)!;
            var previousSettings = NinjadogSettings.FromJsonString(previousJson, projectRoot);
            var currentSettings = NinjadogSettings.FromJsonString(currentJson, projectRoot);

            var diff = SchemaDiffer.Compare(previousSettings, currentSettings);

            if (!diff.HasChanges)
            {
                return Results.Json(new { hasChanges = false });
            }

            var operations = MigrationSqlGenerator.Generate(diff, currentSettings);
            var migrationPath = MigrationFileWriter.Write(projectRoot, operations, migrationName);
            SchemaState.Save(projectRoot, currentJson);

            return Results.Json(new
            {
                hasChanges = true,
                migrationFile = migrationPath is not null ? Path.GetRelativePath(projectRoot, migrationPath) : null,
                operationCount = operations.Count,
            });
        });

        // API: Save current config as evolution baseline
        app.MapPost("/api/evolve/baseline", async (HttpContext ctx) =>
        {
            using var reader = new StreamReader(ctx.Request.Body);
            var json = await reader.ReadToEndAsync(ctx.RequestAborted);

            try
            {
                using var doc = JsonDocument.Parse(json);
            }
            catch (JsonException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: 400);
            }

            var projectRoot = Directory.GetCurrentDirectory();
            SchemaState.Save(projectRoot, json);
            return Results.Json(new { saved = true });
        });

        // API: Check if evolution baseline exists
        app.MapGet("/api/evolve/status", () =>
        {
            var projectRoot = Directory.GetCurrentDirectory();
            return Results.Json(new { hasBaseline = SchemaState.Exists(projectRoot) });
        });

        // Open browser only after server is ready
        if (!settings.NoBrowser)
        {
            app.Lifetime.ApplicationStarted.Register(() => OpenBrowser(url));
        }

        await app.StartAsync(cancellationToken);
        await app.WaitForShutdownAsync(cancellationToken);
        return 0;
    }

    /// <summary>
    /// Finds an available port starting from <paramref name="preferredPort"/>,
    /// trying up to <see cref="MaxPortRetries"/> consecutive ports.
    /// Returns -1 if none are available.
    /// </summary>
    private static int FindAvailablePort(int preferredPort)
    {
        for (var i = 0; i < MaxPortRetries; i++)
        {
            var candidate = preferredPort + i;
            if (IsPortAvailable(candidate))
            {
                return candidate;
            }
        }

        return -1;
    }

    private static bool IsPortAvailable(int port)
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
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

    private static async Task<string> ReadBodyAsync(HttpContext ctx)
    {
        using var reader = new StreamReader(ctx.Request.Body);
        return await reader.ReadToEndAsync(ctx.RequestAborted);
    }
}
