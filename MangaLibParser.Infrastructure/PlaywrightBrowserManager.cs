using Microsoft.Playwright;

namespace MangaLibParser.Infrastructure;

public class PlaywrightBrowserManager : IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private IBrowserContext? _context;
    private IPlaywright? _playwright;

    public async ValueTask DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
            _context = null;
        }

        if (_playwright != null)
        {
            await CastAndDispose(_playwright);
            _playwright = null;
        }

        await CastAndDispose(_semaphore);

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
            {
                await resourceAsyncDisposable.DisposeAsync();
            }
            else
            {
                resource.Dispose();
            }
        }
    }

    public async Task<IPage> GetNewPageAsync()
    {
        await InitializeAsync();

        if (_context == null)
        {
            throw new InvalidOperationException("Browser context is not initialized.");
        }

        return await _context.NewPageAsync();
    }

    private async Task InitializeAsync()
    {
        if (_context != null)
        {
            return;
        }

        await _semaphore.WaitAsync();

        try
        {
            if (_context != null)
            {
                return;
            }

            // TODO: Вынести этот путь в appsettings.json
            const string userDataDir = @"C:\Users\googl\AppData\Local\Microsoft\Edge\User Data";

            _playwright = await Playwright.CreateAsync();
            var context = await _playwright.Chromium.LaunchPersistentContextAsync(userDataDir,
                new BrowserTypeLaunchPersistentContextOptions
                {
                    Headless = true,
                    Channel = "msedge",
                    Args = ["--profile-directory=Default"],
                    IgnoreDefaultArgs = ["--enable-automation"],
                });
        }
        finally
        {
            _semaphore.Release();
        }
    }
}