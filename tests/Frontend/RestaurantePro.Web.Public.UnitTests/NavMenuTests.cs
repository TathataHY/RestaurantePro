using Bunit;
using FluentAssertions;
using RestaurantePro.Web.Public.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantePro.Web.Public.UnitTests;

public class NavMenuTests : TestContext
{
    [Fact]
    public void NavMenu_Deberia_Renderizar_Enlaces_Basicos()
    {
        // Act
        var cut = RenderComponent<NavMenu>();

        // Assert
        cut.Markup.Should().Contain("Inicio");
        cut.Markup.Should().Contain("Menú");
        cut.Markup.Should().Contain("Reseñas");
        cut.Markup.Should().Contain("Registro");
        cut.Markup.Should().Contain("Promociones");
    }

    [Fact]
    public void NavMenu_Activo_En_Menu()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<NavMenu>();
        cut.InvokeAsync(() => nav.NavigateTo("menu"));

        cut.WaitForAssertion(() =>
        {
            var link = cut.FindAll("a").First(a => a.GetAttribute("href") == "menu");
            link.ClassList.Should().Contain("active");
        });
    }

    [Fact]
    public void NavMenu_Activo_En_Resenas()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<NavMenu>();
        cut.InvokeAsync(() => nav.NavigateTo("resenas"));

        cut.WaitForAssertion(() =>
        {
            var link = cut.FindAll("a").First(a => a.GetAttribute("href") == "resenas");
            link.ClassList.Should().Contain("active");
        });
    }

    [Fact]
    public void NavMenu_Activo_En_Registro()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<NavMenu>();
        cut.InvokeAsync(() => nav.NavigateTo("registro"));

        cut.WaitForAssertion(() =>
        {
            var link = cut.FindAll("a").First(a => a.GetAttribute("href") == "registro");
            link.ClassList.Should().Contain("active");
        });
    }

    [Fact]
    public void NavMenu_Activo_En_Promociones()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<NavMenu>();
        cut.InvokeAsync(() => nav.NavigateTo("promociones"));

        cut.WaitForAssertion(() =>
        {
            var link = cut.FindAll("a").First(a => a.GetAttribute("href") == "promociones");
            link.ClassList.Should().Contain("active");
        });
    }
}


