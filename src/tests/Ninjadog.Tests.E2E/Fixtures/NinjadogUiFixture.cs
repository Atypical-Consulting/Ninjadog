using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Ninjadog.Settings.Schema;
using Ninjadog.Settings.Validation;

namespace Ninjadog.Tests.E2E.Fixtures;

/// <summary>
/// Starts the ninjadog UI web server in-process for e2e testing.
/// Each test class gets its own isolated temp directory and port.
/// </summary>
public sealed class NinjadogUiFixture : IAsyncLifetime
{
    private const string ConfigFileName = "ninjadog.json";

    private WebApplication? _app;
    private string _tempDir = null!;
    private int _port;

    public string BaseUrl => $"http://localhost:{_port}";
    public string TempDir => _tempDir;
    public string ConfigPath => Path.Combine(_tempDir, ConfigFileName);

    public async Task InitializeAsync()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ninjadog-e2e-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);

        _port = GetAvailablePort();

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls($"http://localhost:{_port}");
        builder.Logging.ClearProviders();

        _app = builder.Build();

        var configPath = ConfigPath;

        var embeddedProvider = new EmbeddedFileProvider(
            typeof(Ninjadog.CLI.Commands.UiCommandSettings).Assembly,
            "Ninjadog.CLI.WebUI");

        _app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = embeddedProvider,
            RequestPath = string.Empty
        });

        _app.MapGet("/", async ctx =>
        {
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

        _app.MapGet("/api/config", () =>
        {
            if (!File.Exists(configPath))
            {
                return Results.Json(new { }, statusCode: 200);
            }

            var json = File.ReadAllText(configPath);
            return Results.Content(json, "application/json");
        });

        _app.MapPost("/api/config", async (HttpContext ctx) =>
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

            await File.WriteAllTextAsync(configPath, json, ctx.RequestAborted);
            return Results.Json(new { saved = true });
        });

        _app.MapPost("/api/validate", async (HttpContext ctx) =>
        {
            using var reader = new StreamReader(ctx.Request.Body);
            var json = await reader.ReadToEndAsync(ctx.RequestAborted);

            var result = NinjadogConfigValidator.Validate(json);
            return Results.Json(result);
        });

        _app.MapPost("/api/build", () =>
        {
            if (!File.Exists(configPath))
            {
                return Results.Json(new { success = false, error = "No ninjadog.json found." }, statusCode: 400);
            }

            return Results.Json(new { success = true, message = "Build triggered. Use the CLI 'ninjadog build' for full output." });
        });

        _app.MapGet("/api/schema", () =>
        {
            var schemaJson = SchemaProvider.GetSchemaText();
            return Results.Content(schemaJson, "application/json");
        });

        _app.MapGet("/api/directories", (string? path) =>
        {
            var basePath = _tempDir;
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
                    parent = parent != null ? Path.GetRelativePath(basePath, parent) : (string?)null,
                    directories = dirs
                });
            }
            catch
            {
                return Results.Json(new { error = "Cannot access directory" }, statusCode: 400);
            }
        });

        await _app.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }

        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    /// <summary>
    /// Deletes the config file to reset state between tests.
    /// </summary>
    public void ResetConfig()
    {
        if (File.Exists(ConfigPath))
        {
            File.Delete(ConfigPath);
        }
    }

    /// <summary>
    /// Seeds a ninjadog.json config file in the temp directory before a test runs.
    /// </summary>
    public void SeedConfig(object config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }

    /// <summary>
    /// Seeds a ninjadog.json from a raw JSON string.
    /// </summary>
    public void SeedConfigJson(string json)
    {
        File.WriteAllText(ConfigPath, json);
    }

    /// <summary>
    /// Reads the current ninjadog.json content from disk.
    /// </summary>
    public string? ReadSavedConfig()
    {
        return File.Exists(ConfigPath) ? File.ReadAllText(ConfigPath) : null;
    }

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
