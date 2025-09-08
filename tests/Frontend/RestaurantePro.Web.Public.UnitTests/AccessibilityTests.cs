using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Models;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.UnitTests;

public class AccessibilityTests : TestContext
{
    [Fact]
    public void Menu_Imagen_Tiene_Alt_Igual_A_Nombre()
    {
        var cat = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas", ProductosDisponibles = 1 };
        var pageData = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>
            {
                new ProductoDto { Id = Guid.NewGuid(), Nombre = "Pizza", Precio = 10, Activo = true, CategoriaNombre = "Entradas" }
            },
            PageNumber = 1,
            PageSize = 6,
            TotalPages = 1,
            TotalCount = 1
        };

        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = new List<CategoriaProductoDto> { cat } }));
        mock.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>> { Success = true, Data = pageData }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();
        cut.WaitForAssertion(() =>
        {
            var img = cut.Find("img.card-img-top");
            img.GetAttribute("alt").Should().Be("Pizza");
        });
    }

    [Fact]
    public void About_Galeria_Imgs_Tienen_Alt()
    {
        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.About>();
        var imgs = cut.FindAll("img");
        imgs.Should().NotBeEmpty();
        foreach (var i in imgs)
        {
            i.GetAttribute("alt").Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void NavMenu_Toggler_Tiene_AriaLabel()
    {
        var cut = RenderComponent<RestaurantePro.Web.Public.Layout.NavMenu>();
        var btn = cut.Find("button.navbar-toggler");
        btn.GetAttribute("aria-label").Should().Be("Navigation menu");
    }
}


