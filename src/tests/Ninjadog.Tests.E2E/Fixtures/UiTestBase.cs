using Microsoft.Playwright;

namespace Ninjadog.Tests.E2E.Fixtures;

/// <summary>
/// Base class for UI e2e tests. Provides a fresh page and server fixture per test.
/// </summary>
[Collection("NinjadogUi")]
public abstract class UiTestBase(NinjadogUiFixture server, PlaywrightFixture pw) : IAsyncLifetime
{
    protected NinjadogUiFixture Server { get; } = server;
    protected PlaywrightFixture Pw { get; } = pw;
    protected IPage Page { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Reset server state before each test — delete config file so each test starts clean
        Server.ResetConfig();
        Page = await Pw.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Page.Context.DisposeAsync();
    }

    /// <summary>
    /// Navigates to the UI and waits for it to be ready (tabs rendered).
    /// </summary>
    protected async Task NavigateToUiAsync()
    {
        await Page.GotoAsync(Server.BaseUrl);
        // Wait for the React app to initialize (tabs are rendered)
        await Page.WaitForSelectorAsync(".tab-btn[data-tab='config']");
        // Give Monaco a moment to load (CDN-dependent, may timeout in offline)
        await Page.WaitForTimeoutAsync(500);
    }

    /// <summary>
    /// Clicks a tab button and waits for the content to appear.
    /// </summary>
    protected async Task SwitchTabAsync(string tabName)
    {
        await Page.ClickAsync($".tab-btn[data-tab='{tabName}']");
        await Page.WaitForSelectorAsync($"#tab-{tabName}.tab-content-active");
    }

    /// <summary>
    /// Types into an input field identified by data-field attribute.
    /// Clears existing value first.
    /// </summary>
    protected async Task FillFieldAsync(string dataField, string value)
    {
        var selector = $"[data-field='{dataField}']";
        await Page.ClickAsync(selector, new PageClickOptions { ClickCount = 3 });
        await Page.FillAsync(selector, value);
    }

    /// <summary>
    /// Waits for a toast notification with the given text.
    /// </summary>
    protected async Task<IElementHandle?> WaitForToastAsync(string containsText, int timeoutMs = 5000)
    {
        return await Page.WaitForSelectorAsync(
            $".toast:has-text('{containsText}')",
            new PageWaitForSelectorOptions { Timeout = timeoutMs });
    }

    /// <summary>
    /// Checks whether the dirty indicator dot is visible.
    /// </summary>
    protected async Task<bool> IsDirtyAsync()
    {
        var dot = await Page.QuerySelectorAsync("#dirty-indicator");
        return dot is not null && !await dot.EvaluateAsync<bool>("el => el.classList.contains('hidden')");
    }

    /// <summary>
    /// Gets the current JSON content from the Monaco editor via the API.
    /// </summary>
    protected async Task<string> GetEditorJsonAsync()
    {
        var response = await Page.APIRequest.GetAsync($"{Server.BaseUrl}/api/config");
        return await response.TextAsync();
    }

    /// <summary>
    /// Clicks the Save button and waits for the success toast.
    /// </summary>
    protected async Task SaveAndWaitAsync()
    {
        await Page.ClickAsync("#btn-save");
        await WaitForToastAsync("Configuration saved");
    }
}
