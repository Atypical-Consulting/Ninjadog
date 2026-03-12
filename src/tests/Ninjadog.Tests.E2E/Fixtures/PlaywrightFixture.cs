using Microsoft.Playwright;

namespace Ninjadog.Tests.E2E.Fixtures;

/// <summary>
/// Shared Playwright browser instance (one per test collection).
/// </summary>
public sealed class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }

    /// <summary>
    /// Creates a fresh browser context and page for test isolation.
    /// </summary>
    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync();
        return await context.NewPageAsync();
    }
}
