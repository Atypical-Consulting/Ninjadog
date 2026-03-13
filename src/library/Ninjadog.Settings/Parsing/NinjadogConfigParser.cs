using System.Text.Json;
using Ninjadog.Settings.Config;

namespace Ninjadog.Settings.Parsing;

/// <summary>
/// Parses the "config" section of a ninjadog.json file into a <see cref="NinjadogLoadedConfiguration"/>.
/// </summary>
internal static class NinjadogConfigParser
{
    internal static NinjadogLoadedConfiguration Parse(JsonElement configElement)
    {
        var cors = ParseCors(configElement);
        var (softDelete, auditing, aot) = ParseFeatures(configElement);
        var databaseProvider = ParseDatabaseProvider(configElement);
        var versioning = ParseVersioning(configElement);
        var auth = ParseAuth(configElement);
        var rateLimit = ParseRateLimit(configElement);

        return new NinjadogLoadedConfiguration(
            Name: configElement.GetRequiredString("name"),
            Version: configElement.GetRequiredString("version"),
            Description: configElement.GetRequiredString("description"),
            RootNamespace: configElement.GetRequiredString("rootNamespace"),
            OutputPath: configElement.GetOptionalString("outputPath") ?? ".",
            SaveGeneratedFiles: configElement.GetOptionalBoolean("saveGeneratedFiles"),
            Cors: cors,
            SoftDelete: softDelete,
            Auditing: auditing,
            DatabaseProvider: databaseProvider,
            Aot: aot,
            Auth: auth,
            RateLimit: rateLimit,
            Versioning: versioning);
    }

    private static NinjadogCorsConfiguration? ParseCors(JsonElement configElement)
    {
        return !configElement.TryGetOptionalObject("cors", out var corsElement)
            ? null
            : new NinjadogCorsConfiguration(
                corsElement.GetOptionalStringArray("origins") ?? [],
                corsElement.GetOptionalStringArray("methods"),
                corsElement.GetOptionalStringArray("headers"));
    }

    private static (bool SoftDelete, bool Auditing, bool Aot) ParseFeatures(JsonElement configElement)
    {
        return !configElement.TryGetOptionalObject("features", out var featuresElement)
            ? (false, false, false)
            : (featuresElement.GetOptionalBoolean("softDelete"),
                featuresElement.GetOptionalBoolean("auditing"),
                featuresElement.GetOptionalBoolean("aot"));
    }

    private static string ParseDatabaseProvider(JsonElement configElement)
    {
        return !configElement.TryGetOptionalObject("database", out var dbElement)
            ? "sqlite"
            : dbElement.GetOptionalString("provider") ?? "sqlite";
    }

    private static NinjadogVersioningConfiguration? ParseVersioning(JsonElement configElement)
    {
        return !configElement.TryGetOptionalObject("versioning", out var versioningElement)
            ? null
            : new NinjadogVersioningConfiguration(
                Strategy: versioningElement.GetOptionalString("strategy") ?? NinjadogVersioningConfiguration.UrlPathStrategy,
                DefaultVersion: versioningElement.GetOptionalInt32("defaultVersion") ?? 1,
                Prefix: versioningElement.GetOptionalString("prefix") ?? "v",
                HeaderName: versioningElement.GetOptionalString("headerName") ?? "X-Api-Version");
    }

    private static NinjadogAuthConfiguration? ParseAuth(JsonElement configElement)
    {
        return !configElement.TryGetOptionalObject("auth", out var authElement)
            ? null
            : new NinjadogAuthConfiguration(
                Provider: authElement.GetOptionalString("provider") ?? "jwt",
                Issuer: authElement.GetOptionalString("issuer") ?? "https://localhost",
                Audience: authElement.GetOptionalString("audience") ?? "api",
                TokenExpirationMinutes: authElement.GetOptionalInt32("tokenExpirationMinutes") ?? 60,
                Roles: authElement.GetOptionalStringArray("roles"),
                GenerateLoginEndpoint: !authElement.TryGetOptionalProperty("generateLoginEndpoint", out _) || authElement.GetOptionalBoolean("generateLoginEndpoint"),
                GenerateRegisterEndpoint: !authElement.TryGetOptionalProperty("generateRegisterEndpoint", out _) || authElement.GetOptionalBoolean("generateRegisterEndpoint"));
    }

    private static NinjadogRateLimitConfiguration? ParseRateLimit(JsonElement configElement)
    {
        return !configElement.TryGetOptionalObject("rateLimit", out var rateLimitElement)
            ? null
            : new NinjadogRateLimitConfiguration(
                PermitLimit: rateLimitElement.GetOptionalInt32("permitLimit") ?? 100,
                WindowSeconds: rateLimitElement.GetOptionalInt32("windowSeconds") ?? 60,
                SegmentsPerWindow: rateLimitElement.GetOptionalInt32("segmentsPerWindow") ?? 6);
    }
}
