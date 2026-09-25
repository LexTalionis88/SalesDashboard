using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace SalesDashboard.E2ETests;

[TestFixture, Category("E2E"), Category("BrowserE2E"), NonParallelizable]
public sealed class FrontendJourneyTests
{
    private readonly string frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_E2E_URL") ?? "http://localhost:3000";
    private IPlaywright playwright = null!;
    private IBrowser browser = null!;
    private IBrowserContext context = null!;
    private IPage page = null!;

    /// <summary>Запускает Chromium для сценариев браузерного E2E.</summary>
    [OneTimeSetUp]
    public async Task StartBrowser()
    {
        playwright = await Playwright.CreateAsync();
        try
        {
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }
        catch (PlaywrightException exception)
        {
            playwright.Dispose();
            Assert.Ignore($"Chromium для браузерных E2E не установлен: {exception.Message}");
        }
    }

    /// <summary>Создаёт изолированную страницу и открывает dashboard.</summary>
    [SetUp]
    public async Task OpenDashboard()
    {
        context = await browser.NewContextAsync();
        page = await context.NewPageAsync();
        await page.GotoAsync(frontendUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    /// <summary>Проверяет загрузку показателей, рейтинга и истории продаж.</summary>
    [Test]
    public async Task Dashboard_displays_metrics_and_sales_history()
    {
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Обзор продаж" })).ToBeVisibleAsync();
        await Expect(page.GetByText("Выручка", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Рейтинг менеджеров" })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Последние продажи" })).ToBeVisibleAsync();
    }

    /// <summary>Проверяет смену периода и сортировки рейтинга через интерфейс.</summary>
    [Test]
    public async Task User_can_change_period_and_ranking()
    {
        await page.GetByRole(AriaRole.Button, new() { Name = "7 дней" }).ClickAsync();
        await Expect(page.Locator("input[type=date]").First).Not.ToHaveValueAsync("");

        await page.GetByLabel("Сортировка рейтинга").SelectOptionAsync("averageCheck");
        await Expect(page.GetByLabel("Сортировка рейтинга")).ToHaveValueAsync("averageCheck");
    }

    /// <summary>Проверяет понятное состояние интерфейса для периода без данных.</summary>
    [Test]
    public async Task Empty_period_displays_empty_state()
    {
        var dates = page.Locator("input[type=date]");
        await dates.First.FillAsync("2000-01-01");
        await dates.Nth(1).FillAsync("2000-01-02");

        await Expect(page.GetByText("За этот период оплаченных продаж нет.", new() { Exact = false })).ToBeVisibleAsync();
    }

    /// <summary>Закрывает страницу и контекст после каждого теста.</summary>
    [TearDown]
    public async Task ClosePage()
    {
        if (context is not null) await context.CloseAsync();
    }

    /// <summary>Освобождает ресурсы Playwright.</summary>
    [OneTimeTearDown]
    public async Task StopBrowser()
    {
        if (browser is not null) await browser.CloseAsync();
        playwright?.Dispose();
    }
}
