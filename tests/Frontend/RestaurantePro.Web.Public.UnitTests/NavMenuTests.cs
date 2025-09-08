using Bunit;
using FluentAssertions;
using RestaurantePro.Web.Public.Layout;

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
}


