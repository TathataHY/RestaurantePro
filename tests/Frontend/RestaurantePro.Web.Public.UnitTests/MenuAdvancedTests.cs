using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Models;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.UnitTests;

public class MenuAdvancedTests : TestContext
{
    [Fact]
    public void Menu_Formatea_Precio_Con_Miles_Y_Simbolo()
    {
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 1 } };
        var items = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Ceviche", Descripcion = "Clásico", Precio = 25000, CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true, Popularidad = 10 },
        };

        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        mock.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>> { Success = true, Data = new PaginatedList<ProductoDto> { Items = items, PageNumber = 1, PageSize = 6, TotalCount = 1, TotalPages = 1 } }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("$ 25.000");
        });
    }

    [Fact]
    public void Menu_Buscar_Categorias_Muestra_Resultados()
    {
        var categoriaId = Guid.NewGuid();
        var categorias = new List<CategoriaProductoDto> { new() { Id = categoriaId, Nombre = "Pizza", ProductosDisponibles = 1 } };
        var items = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Margarita", Descripcion = "", Precio = 12000, CategoriaId = categoriaId, CategoriaNombre = "Pizza", Activo = true, Popularidad = 5 },
        };

        var mock = new MockHttpMessageHandler();
        // La primera carga trae categorías base vacías
        mock.When("http://localhost/api/core/categorias?*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = new List<CategoriaProductoDto>() }));
        // Buscar
        mock.When("http://localhost/api/core/categorias/buscar*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias }));
        // Productos
        mock.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>> { Success = true, Data = new PaginatedList<ProductoDto> { Items = items, PageNumber = 1, PageSize = 6, TotalCount = 1, TotalPages = 1 } }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Ingresar texto y aplicar búsqueda
        cut.Find("input[placeholder='Buscar categoría...']").Input("Pizza");
        cut.Find("button.btn.btn-primary").Click();

        // Debe seleccionar la primera categoría encontrada y mostrar encabezado con su nombre
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Productos  - Pizza");
            cut.Markup.Should().Contain("Margarita");
        });
    }
}


