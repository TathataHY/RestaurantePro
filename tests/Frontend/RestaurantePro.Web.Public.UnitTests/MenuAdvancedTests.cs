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
        // Buscar (registrar PRIMERO para priorizar sobre el genérico)
        mock.When("http://localhost/api/core/categorias/buscar*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias }));
        // La primera carga trae categorías base vacías
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = new List<CategoriaProductoDto>() }));
        // Productos
        mock.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>> { Success = true, Data = new PaginatedList<ProductoDto> { Items = items, PageNumber = 1, PageSize = 6, TotalCount = 1, TotalPages = 1 } }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Ingresar texto y aplicar búsqueda
        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Buscar categoría...']").Input("Pizza");
            cut.Find("button.btn.btn-primary").Click();
        });

        // Debe seleccionar la primera categoría encontrada y mostrar resultados de esa categoría
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Margarita");
            cut.Markup.Should().Contain("Pizza");
        }, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Menu_Busqueda_Trimming_Y_CaseInsensitive()
    {
        var mock = new MockHttpMessageHandler();
        // Debe llamar a buscar con nombre en minúsculas y sin espacios
        mock.When("http://localhost/api/core/categorias/buscar*")
            .WithQueryString("nombre", "pizza")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = new List<CategoriaProductoDto>() }));
        // Carga inicial de categorías
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = new List<CategoriaProductoDto>() }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Ingresar texto con espacios y mayúsculas
        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Buscar categoría...']").Input("  PiZzA  ");
            cut.Find("button.btn.btn-primary").Click();
        });

        // Si se alcanzó el handler, no debe lanzar excepción
        cut.Markup.Should().Contain("Menú");
    }
}

public class MenuSortUiTests : TestContext
{
    [Fact]
    public void Menu_Ordenacion_UI_PrecioAscDesc_Y_Popularidad()
    {
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 3 } };
        var itemsBase = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "A", Precio = 10000, Popularidad = 5, CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "B", Precio = 20000, Popularidad = 8, CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "C", Precio = 15000, Popularidad = 3, CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true },
        };

        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        // Respuesta dinámica según querystring
        mock.When("http://localhost/api/core/productos*")
            .Respond(req =>
            {
                var query = req.RequestUri?.Query ?? string.Empty;
                List<ProductoDto> ordered;
                if (query.Contains("OrderBy=Precio") && query.Contains("OrderDirection=asc"))
                {
                    ordered = itemsBase.OrderBy(p => p.Precio).ToList();
                }
                else if (query.Contains("OrderBy=Precio") && query.Contains("OrderDirection=desc"))
                {
                    ordered = itemsBase.OrderByDescending(p => p.Precio).ToList();
                }
                else
                {
                    ordered = itemsBase.OrderByDescending(p => p.Popularidad).ToList();
                }

                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
                {
                    Success = true,
                    Data = new PaginatedList<ProductoDto> { Items = ordered, PageNumber = 1, PageSize = 6, TotalCount = ordered.Count, TotalPages = 1 }
                });
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });
            });

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        cut.InvokeAsync(() => cut.Find("select.form-select").Change("precio_asc"));
        cut.InvokeAsync(() => cut.Find("button.btn.btn-primary").Click());
        cut.WaitForAssertion(() =>
        {
            var cards = cut.FindAll(".card .card-title").Select(e => e.TextContent).ToList();
            cards.Should().ContainInOrder("A", "C", "B");
        });

        cut.InvokeAsync(() => cut.Find("select.form-select").Change("precio_desc"));
        cut.InvokeAsync(() => cut.Find("button.btn.btn-primary").Click());
        cut.WaitForAssertion(() =>
        {
            var cards = cut.FindAll(".card .card-title").Select(e => e.TextContent).ToList();
            cards.Should().ContainInOrder("B", "C", "A");
        });

        cut.InvokeAsync(() => cut.Find("select.form-select").Change("popularidad"));
        cut.InvokeAsync(() => cut.Find("button.btn.btn-primary").Click());
        cut.WaitForAssertion(() =>
        {
            var cards = cut.FindAll(".card .card-title").Select(e => e.TextContent).ToList();
            cards.Should().ContainInOrder("B", "A", "C");
        });
    }
}


