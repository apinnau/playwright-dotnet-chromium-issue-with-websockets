using Microsoft.Playwright;

namespace dotnetDemo.E2ETests.Infrastructure;

public class LocalizedBrowser : IDisposable
{
    private readonly string _baseURL;
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private readonly IBrowserContext _context;
    private readonly IPage _page;
    private readonly float _default_timeout = 5000.0f;

    private LocalizedBrowser(BrowserLanguage language, BrowserType browserType, string baseURL)
    {
        var context = GetLocalizedContext(language);

        _baseURL = baseURL;
        _playwright = Playwright.CreateAsync().Result;
        _browser = CreateBrowser(browserType).Result;
        _context = _browser.NewContextAsync(context).Result;
        _page = _context.NewPageAsync().Result;

        _page.SetDefaultTimeout(_default_timeout);
    }

    public static LocalizedBrowser Create(BrowserLanguage language, BrowserType browserType, string baseUrl)
    {
        return new LocalizedBrowser(language, browserType, baseUrl);
    }

    public static LocalizedBrowser CreateGermanChrome(string baseUrl)
    {
        return new LocalizedBrowser(BrowserLanguage.German, BrowserType.Chromium, baseUrl);
    }

    public static LocalizedBrowser CreateEnglishChrome(string baseUrl)
    {
        return new LocalizedBrowser(BrowserLanguage.English, BrowserType.Chromium, baseUrl);
    }

    public static LocalizedBrowser CreateGermanFirefox(string baseUrl)
    {
        return new LocalizedBrowser(BrowserLanguage.German, BrowserType.Firefox, baseUrl);
    }

    public static LocalizedBrowser CreateEnglishFirefox(string baseUrl)
    {
        return new LocalizedBrowser(BrowserLanguage.English, BrowserType.Firefox, baseUrl);
    }

    public IPage Page
    {
        get { return _page; }
    }

    private async Task<IBrowser> CreateBrowser(BrowserType type)
    {
        switch (type)
        {
            case BrowserType.Chromium:
                return await _playwright.Chromium.LaunchAsync();
            case BrowserType.Webkit:
                return await _playwright.Webkit.LaunchAsync();
            case BrowserType.Firefox:
                return await _playwright.Firefox.LaunchAsync();
        }

        throw new ArgumentOutOfRangeException(nameof(type), "Unknown Browser Type.");
    }

    private BrowserNewContextOptions GetLocalizedContext(BrowserLanguage language)
    {
        switch (language)
        {
            case BrowserLanguage.German:
                return GetContextOptionsForGermanBrowser();
            case BrowserLanguage.English:
                return GetContextOptionsForEnglishBrowser();
        }

        throw new ArgumentOutOfRangeException(nameof(language), "Unknown Language Type.");
    }

    private BrowserNewContextOptions GetContextOptionsForGermanBrowser()
    {
        return new BrowserNewContextOptions()
        {
            BaseURL = _baseURL,
            Locale = "de,en;q=0.9,en-GB;q=0.8,de-DE;q=0.7,en-US;q=0.6",
        };
    }

    private BrowserNewContextOptions GetContextOptionsForEnglishBrowser()
    {
        return new BrowserNewContextOptions()
        {
            BaseURL = _baseURL,
            Locale = "en,de;q=0.9,en-GB;q=0.8,de-DE;q=0.7,en-US;q=0.6",
        };
    }

    public void Dispose()
    {
        _page.CloseAsync();
        _context.CloseAsync();
        _browser.CloseAsync();
        _playwright.Dispose();
    }
}
