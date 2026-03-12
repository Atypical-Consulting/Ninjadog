namespace Ninjadog.Settings.Config;

/// <summary>
/// Represents the rate limiting configuration for the generated API.
/// Uses ASP.NET Core's built-in sliding window rate limiter.
/// </summary>
/// <param name="PermitLimit">The maximum number of requests permitted in the window.</param>
/// <param name="WindowSeconds">The time window duration in seconds.</param>
/// <param name="SegmentsPerWindow">The number of segments the window is divided into.</param>
public record NinjadogRateLimitConfiguration(
    int PermitLimit = 100,
    int WindowSeconds = 60,
    int SegmentsPerWindow = 6);
