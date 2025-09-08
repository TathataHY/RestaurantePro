using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace RestaurantePro.Web.Public.UITests;

public class PromocionesUiTests
{
    [Fact]
    public async Task Navegar_Promociones_Deberia_Mostrar_Titulo()
    {
        var baseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            // Si no hay servidor corriendo, no ejecutamos la navegación.
            return;
        }

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{baseUrl.TrimEnd('/')}/promociones");

        var titulo = await page.Locator("h3").First.InnerTextAsync();
        titulo.Should().Contain("Promociones");
    }
}


