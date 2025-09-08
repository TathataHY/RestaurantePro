using Bunit;
using FluentAssertions;
using RestaurantePro.Web.Public.Layout;

namespace RestaurantePro.Web.Public.UnitTests;

public class NavMenuToggleTests : TestContext
{
    [Fact]
    public void Toggle_Cambia_Clase_Collapse()
    {
        var cut = RenderComponent<NavMenu>();
        var wrapper = cut.Find("div.nav-scrollable");
        wrapper.GetAttribute("class").Should().Contain("collapse");

        cut.Find("button.navbar-toggler").Click();

        wrapper = cut.Find("div.nav-scrollable");
        wrapper.GetAttribute("class").Should().NotContain("collapse");
    }

    [Fact]
    public void Click_En_Link_Colapsa_Menu()
    {
        var cut = RenderComponent<NavMenu>();
        cut.Find("button.navbar-toggler").Click();
        var wrapper = cut.Find("div.nav-scrollable");
        wrapper.GetAttribute("class").Should().NotContain("collapse");

        // Click en enlace "Menú" dispara Toggle por @onclick del contenedor
        cut.FindAll("a").First(a => a.TextContent.Contains("Menú")).Click();
        wrapper = cut.Find("div.nav-scrollable");
        wrapper.GetAttribute("class").Should().Contain("collapse");
    }

    [Fact]
    public void AriaExpanded_Toggle_Correcto()
    {
        var cut = RenderComponent<NavMenu>();
        var btn = cut.Find("button.navbar-toggler");
        btn.GetAttribute("aria-expanded").Should().Be("false");
        btn.Click();
        cut.WaitForAssertion(() => cut.Find("button.navbar-toggler").GetAttribute("aria-expanded").Should().Be("true"));
        btn.Click();
        cut.WaitForAssertion(() => cut.Find("button.navbar-toggler").GetAttribute("aria-expanded").Should().Be("false"));
    }
}


