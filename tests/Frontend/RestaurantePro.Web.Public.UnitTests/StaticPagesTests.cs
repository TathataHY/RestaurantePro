using Bunit;
using FluentAssertions;

namespace RestaurantePro.Web.Public.UnitTests;

public class StaticPagesTests : TestContext
{
    [Fact]
    public void Home_Renderiza_Hero_Y_CTA()
    {
        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Home>();
        cut.Markup.Should().Contain("Sabores que inspiran");
        cut.Markup.Should().Contain("Ver Menú");
        cut.Markup.Should().Contain("Reseñas");
    }

    [Fact]
    public void Reservas_Renderiza_Info_Y_Enlaces()
    {
        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Reservas>();
        cut.Markup.Should().Contain("Reservas");
        cut.Markup.Should().Contain("WhatsApp");
        cut.FindAll("a").Should().NotBeEmpty();
    }

    [Fact]
    public void Politicas_Renderiza_Titulos()
    {
        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Politicas>();
        cut.Markup.Should().Contain("Políticas del Restaurante");
        cut.Markup.Should().Contain("Reservas y cancelaciones");
        cut.Markup.Should().Contain("Privacidad y cookies");
    }

    [Fact]
    public void About_Renderiza_Secciones_Y_Galeria()
    {
        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.About>();
        cut.Markup.Should().Contain("Acerca de RestaurantePro");
        cut.FindAll("img").Should().NotBeEmpty();
        // Abrir y cerrar modal
        cut.Find("img").Click();
        cut.Markup.Should().Contain("modal");
        cut.Find("button.btn.btn-primary").Click();
    }
}


