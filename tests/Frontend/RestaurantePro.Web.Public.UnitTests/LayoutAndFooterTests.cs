using Bunit;
using FluentAssertions;
using RestaurantePro.Web.Public.Layout;
using RestaurantePro.Web.Public.Shared;

namespace RestaurantePro.Web.Public.UnitTests;

public class LayoutAndFooterTests : TestContext
{
    [Fact]
    public void MainLayout_Renderiza_Footer()
    {
        var cut = RenderComponent<MainLayout>();
        cut.Markup.Should().Contain("Síguenos");
    }

    [Fact]
    public void Footer_Tiene_Enlaces_Sociales()
    {
        var cut = RenderComponent<Footer>();
        cut.FindAll("a").Select(a => a.GetAttribute("href")).Should().Contain(new[]
        {
            "https://instagram.com/restaurantepro",
            "https://facebook.com/restaurantepro",
            "https://x.com/restaurantepro"
        });
    }

    [Fact]
    public void Footer_Muestra_Anio_Actual()
    {
        var cut = RenderComponent<Footer>();
        cut.Markup.Should().Contain(DateTime.Now.Year.ToString());
    }

    [Fact]
    public void NavMenu_Inicio_Activo_En_Raiz()
    {
        var cut = RenderComponent<NavMenu>();
        var inicio = cut.Find("a.nav-link[href='']");
        inicio.ClassList.Should().Contain("active");
    }
}


