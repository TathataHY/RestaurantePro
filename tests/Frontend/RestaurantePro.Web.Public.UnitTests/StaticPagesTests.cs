using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace RestaurantePro.Web.Public.UnitTests;

public class StaticPagesTests : TestContext
{
    public StaticPagesTests()
    {
        // JSInterop en modo relajado para PageTitle/HeadOutlet
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
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
    public void Promociones_HeadContent_Tiene_SEO_Basico()
    {
        // Registrar servicio requerido con HttpClient simulado
        var mock = new RichardSzalay.MockHttp.MockHttpMessageHandler();
        mock.When("http://localhost/api/public/promociones*")
            .Respond("application/json", "{ \"success\": true, \"data\": [] }");
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<RestaurantePro.Web.Public.Services.PromocionesApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Promociones>();
        cut.Markup.Should().Contain("Promociones");
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

    [Fact]
    public void Home_PageTitle_Renderiza_Title_Tag()
    {
        var head = RenderComponent<HeadOutlet>();
        RenderComponent<RestaurantePro.Web.Public.Pages.Home>();
        head.Markup.Should().Contain("<title>Inicio</title>");
    }

    [Fact]
    public void Home_No_Duplica_Meta_En_ReRender()
    {
        var head = RenderComponent<HeadOutlet>();
        var home = RenderComponent<RestaurantePro.Web.Public.Pages.Home>();
        // Forzar re-render
        home.Render();
        var markup = head.Markup;
        markup.Split("property=\"og:title\"").Length.Should().Be(2); // 1 ocurrencia -> 2 partes
        markup.Split("property=\"og:description\"").Length.Should().Be(2);
    }

    [Fact]
    public void SEO_About_No_Duplica_Meta_En_ReRender()
    {
        var head = RenderComponent<HeadOutlet>();
        var about = RenderComponent<RestaurantePro.Web.Public.Pages.About>();
        about.Render();
        var markup = head.Markup;
        markup.Split("property=\"og:title\"").Length.Should().Be(2);
        markup.Split("property=\"og:description\"").Length.Should().Be(2);
    }

    [Fact]
    public void SEO_Reservas_No_Duplica_Meta_En_ReRender()
    {
        var head = RenderComponent<HeadOutlet>();
        var res = RenderComponent<RestaurantePro.Web.Public.Pages.Reservas>();
        res.Render();
        var markup = head.Markup;
        markup.Split("property=\"og:title\"").Length.Should().Be(2);
        markup.Split("property=\"og:description\"").Length.Should().Be(2);
    }

    [Fact]
    public void SEO_Menu_No_Duplica_Meta_En_ReRender()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", "{ \"success\": true, \"data\": [] }");
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<RestaurantePro.Web.Public.Services.MenuApiService>();

        var head = RenderComponent<HeadOutlet>();
        var menu = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();
        menu.Render();
        var markup = head.Markup;
        markup.Split("property=\"og:title\"").Length.Should().Be(2);
        markup.Split("property=\"og:description\"").Length.Should().Be(2);
    }

    [Fact]
    public void SEO_Registro_No_Duplica_Meta_En_ReRender()
    {
        Services.AddScoped(sp => new HttpClient(new MockHttpMessageHandler()) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<RestaurantePro.Web.Public.Services.ClientesPublicApiService>();

        var head = RenderComponent<HeadOutlet>();
        var reg = RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();
        reg.Render();
        var markup = head.Markup;
        markup.Split("property=\"og:title\"").Length.Should().Be(2);
        markup.Split("property=\"og:description\"").Length.Should().Be(2);
    }

    [Fact]
    public void Reservas_WhatsApp_Tiene_TargetBlank_Y_Noopener()
    {
        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Reservas>();
        var wa = cut.FindAll("a").First(a => a.GetAttribute("href")!.StartsWith("https://wa.me/") && a.GetAttribute("href")!.Contains("text="));
        wa.GetAttribute("target").Should().Be("_blank");
        wa.GetAttribute("rel").Should().Contain("noopener");
    }

    [Fact]
    public void Reservas_Tiene_Tel_Y_Mailto_Validos()
    {
        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Reservas>();
        var tel = cut.FindAll("a").First(a => a.GetAttribute("href")!.StartsWith("tel:"));
        var mail = cut.FindAll("a").First(a => a.GetAttribute("href")!.StartsWith("mailto:"));
        tel.GetAttribute("href").Should().MatchRegex("^tel:\\+?\\d+");
        mail.GetAttribute("href").Should().Contain("@");
    }

    [Fact]
    public void Footer_Social_Tienen_Noopener_Y_TargetBlank()
    {
        var cut = RenderComponent<RestaurantePro.Web.Public.Shared.Footer>();
        var links = cut.FindAll("a").Where(a => a.GetAttribute("href")!.StartsWith("http"));
        links.Should().NotBeEmpty();
        foreach (var a in links)
        {
            a.GetAttribute("target").Should().Be("_blank");
            a.GetAttribute("rel").Should().Contain("noopener");
        }
    }

    [Fact]
    public void SEO_Home_HeadContent()
    {
        var head = RenderComponent<HeadOutlet>();
        RenderComponent<RestaurantePro.Web.Public.Pages.Home>();
        head.Markup.Should().Contain("href=\"/\"");
        head.Markup.Should().Contain("property=\"og:title\" content=\"RestaurantePro - Sabores que inspiran\"");
        head.Markup.Should().Contain("property=\"og:description\" content=\"Ingredientes frescos, técnica moderna y un servicio que te hará volver.\"");
    }

    [Fact]
    public void SEO_About_HeadContent()
    {
        var head = RenderComponent<HeadOutlet>();
        RenderComponent<RestaurantePro.Web.Public.Pages.About>();
        head.Markup.Should().Contain("href=\"/acerca\"");
        head.Markup.Should().Contain("property=\"og:title\" content=\"Acerca - RestaurantePro\"");
        head.Markup.Should().Contain("property=\"og:description\" content=\"Conoce nuestra historia, valores, horarios y ubicación.\"");
    }

    [Fact]
    public void SEO_Reservas_HeadContent()
    {
        var head = RenderComponent<HeadOutlet>();
        RenderComponent<RestaurantePro.Web.Public.Pages.Reservas>();
        head.Markup.Should().Contain("href=\"/reservas\"");
        head.Markup.Should().Contain("property=\"og:title\" content=\"Reservas - RestaurantePro\"");
        head.Markup.Should().Contain("property=\"og:description\" content=\"Reserva por teléfono o WhatsApp mientras habilitamos reservas en línea.\"");
    }

    [Fact]
    public void SEO_Menu_HeadContent()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", "{ \"success\": true, \"data\": [] }");
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<RestaurantePro.Web.Public.Services.MenuApiService>();

        var head = RenderComponent<HeadOutlet>();
        RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();
        head.Markup.Should().Contain("href=\"/menu\"");
        head.Markup.Should().Contain("property=\"og:title\" content=\"Menú - RestaurantePro\"");
        head.Markup.Should().Contain("property=\"og:description\" content=\"Explora nuestras categorías y platos destacados.\"");
    }

    [Fact]
    public void SEO_Registro_HeadContent()
    {
        // Registrar servicio requerido por la página ANTES de renderizar
        var mock = new MockHttpMessageHandler();
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<RestaurantePro.Web.Public.Services.ClientesPublicApiService>();
        var head = RenderComponent<HeadOutlet>();
        RenderComponent<RestaurantePro.Web.Public.Pages.Registro>();
        head.Markup.Should().Contain("href=\"/registro\"");
        head.Markup.Should().Contain("property=\"og:title\" content=\"Registro de clientes - RestaurantePro\"");
        head.Markup.Should().Contain("property=\"og:description\" content=\"Regístrate para recibir novedades y beneficios.\"");
    }
}


