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
            using var reader = new StreamReader(ctx.Request.Body);
            var json = await reader.ReadToEndAsync(ctx.RequestAborted);

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
            using var reader = new StreamReader(ctx.Request.Body);
            var json = await reader.ReadToEndAsync(ctx.RequestAborted);

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
}
