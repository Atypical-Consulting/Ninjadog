using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ninjadog.Settings.Schema;
using Ninjadog.Settings.Validation;

namespace Ninjadog.CLI.AI;

internal static partial class ConfigGenerator
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string Model = "claude-sonnet-4-20250514";
    private const string ApiVersion = "2023-06-01";
    private const int MaxTokens = 4096;

    private static readonly HttpClient _httpClient = new();
    private static readonly Lazy<string> _systemPrompt = new(BuildSystemPrompt);

    public static async Task<GenerationResult> GenerateAsync(
        string prompt,
        CancellationToken ct = default)
    {
        return await GenerateAsync([new ChatMessage("user", prompt)], ct);
    }

    public static async Task<GenerationResult> GenerateAsync(
        List<ChatMessage> messages,
        CancellationToken ct = default)
    {
        var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return GenerationResult.Failure(
                "ANTHROPIC_API_KEY environment variable is not set. Get your API key at https://console.anthropic.com/");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", ApiVersion);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestBody = new
            {
                model = Model,
                max_tokens = MaxTokens,
                system = _systemPrompt.Value,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = TryExtractErrorMessage(responseBody)
                    ?? $"API request failed with status {response.StatusCode}";
                return GenerationResult.Failure(errorMsg);
            }

            var assistantText = ExtractAssistantText(responseBody);
            if (assistantText is null)
            {
                return GenerationResult.Failure("Failed to parse API response.");
            }

            var configJson = StripMarkdownFences(assistantText);

            var validation = NinjadogConfigValidator.Validate(configJson);

            return new GenerationResult(
                validation.IsValid,
                configJson,
                assistantText,
                validation,
                validation.IsValid ? null : "Generated configuration has validation issues.");
        }
        catch (TaskCanceledException)
        {
            return GenerationResult.Failure("Request was cancelled.");
        }
        catch (HttpRequestException ex)
        {
            return GenerationResult.Failure($"Network error: {ex.Message}");
        }
    }

    private static string BuildSystemPrompt()
    {
        var schema = SchemaProvider.GetSchemaText();

        return $"""
            You are a Ninjadog configuration generator. Given a user's description of an API, produce a valid ninjadog.json configuration file.

            Rules:
            - Output ONLY valid JSON matching the schema below. No markdown fences, no explanations, no commentary.
            - Every entity MUST have exactly one property with "isKey": true (typically a Guid named "id").
            - Entity names must be PascalCase (e.g., "BlogPost", "UserProfile").
            - Property names must be camelCase (e.g., "firstName", "createdAt").
            - Use appropriate .NET types: string, int, long, bool, decimal, DateTime, DateOnly, Guid.
            - Include sensible validation constraints (required, maxLength, minLength, min, max, pattern) based on the domain.
            - Set config.name, config.version ("1.0.0"), config.description, and config.rootNamespace based on the user's description.
            - Default to sqlite for database provider unless the user specifies otherwise.
            - Include enums when the domain naturally has them (e.g., Status, Priority, Role).
            - Include relationships when entities are clearly related (e.g., Post belongs to User).
            - Include seed data with 2-3 realistic sample rows per entity when it helps illustrate the schema.
            - When updating an existing configuration, output the COMPLETE updated JSON, not just the changed parts.

            JSON Schema:
            {schema}
            """;
    }

    private static string? ExtractAssistantText(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var contentArray = doc.RootElement.GetProperty("content");
            foreach (var block in contentArray.EnumerateArray())
            {
                if (block.GetProperty("type").GetString() == "text")
                {
                    return block.GetProperty("text").GetString();
                }
            }
        }
        catch
        {
            // Failed to parse response
        }

        return null;
    }

    private static string? TryExtractErrorMessage(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var msg))
            {
                return msg.GetString();
            }
        }
        catch
        {
            // Not JSON
        }

        return null;
    }

    private static string StripMarkdownFences(string text)
    {
        var trimmed = text.Trim();
        var match = MarkdownJsonFenceRegex().Match(trimmed);
        return match.Success ? match.Groups[1].Value.Trim() : trimmed;
    }

    [GeneratedRegex(@"^```(?:json)?\s*\n?([\s\S]*?)\n?\s*```$", RegexOptions.Singleline)]
    private static partial Regex MarkdownJsonFenceRegex();
}
