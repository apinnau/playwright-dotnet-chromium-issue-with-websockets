using dotnetDemo.E2ETests.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace dotnetDemo.E2ETests;

public class LanguageTests(CustomWebApplicationFactory fixture) : PageTest, IClassFixture<CustomWebApplicationFactory>
{
    private readonly string _serverAddress = fixture.ServerAddress;

    [Fact]
    public async Task HomepageWelcomeText_German_Firefox()
    {
        //Arrange
        using var browser = LocalizedBrowser.CreateGermanFirefox(_serverAddress);
        var page = browser.Page;

        //Act
        await page.GotoAsync(_serverAddress);
        await Task.Delay(1000); // wait until site is reloaded via websocket

        //Assert
        await Expect(page.GetByText("Willkommen bei deiner neuen App")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task HomepageWelcomeText_English_Firefox()
    {
        //Arrange
        using var browser = LocalizedBrowser.CreateEnglishFirefox(_serverAddress);
        var page = browser.Page;

        //Act
        await page.GotoAsync(_serverAddress);
        await Task.Delay(1000); // wait until site is reloaded via websocket

        //Assert
        await Expect(page.GetByText("Welcome to your new app")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task HomepageWelcomeText_German_Chrome()
    {
        //Arrange
        using var browser = LocalizedBrowser.CreateGermanChrome(_serverAddress);
        var page = browser.Page;

        //Act
        await page.GotoAsync(_serverAddress);
        await Task.Delay(1000); // wait until site is reloaded via websocket

        //Assert
        await Expect(page.GetByText("Willkommen bei deiner neuen App")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task HomepageWelcomeText_English_Chrome()
    {
        //Arrange
        using var browser = LocalizedBrowser.CreateEnglishChrome(_serverAddress);
        var page = browser.Page;

        //Act
        await page.GotoAsync(_serverAddress);
        await Task.Delay(1000); // wait until site is reloaded via websocket

        //Assert
        await Expect(page.GetByText("Welcome to your new app")).ToBeVisibleAsync();
    }
}