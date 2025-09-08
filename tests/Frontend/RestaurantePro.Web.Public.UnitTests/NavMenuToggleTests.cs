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
}


