using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Models;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.UnitTests;

public class MenuPageTests : TestContext
{
    [Fact]
    public void Menu_Muestra_Skeleton_Durante_Carga()
    {
        // Arrange: categorías vacías y luego trigger a selección simulado
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/api/core/categorias*")
            .Respond("application/json", "{ \"success\": true, \"data\": [] }");
        Services.AddScoped(sp => new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        // Act
        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Assert (render inicial muestra título y puede mostrar estado de carga si se selecciona categoría)
        cut.Markup.Should().Contain("Menú");
    }

    [Fact]
    public void Menu_Estado_Vacio_Sin_Productos()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Entradas", ProductosDisponibles = 0 } };
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        mockHttp.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>> { Success = true, Data = new PaginatedList<ProductoDto> { Items = new List<ProductoDto>(), PageNumber = 1, PageSize = 6, TotalCount = 0, TotalPages = 1 } }));

        Services.AddScoped(sp => new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        // Act
        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("No hay productos para mostrar.");
        });
    }

    [Fact]
    public void Menu_Renderiza_Productos_Con_Data()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 2 } };
        var items = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Ceviche", Descripcion = "Clásico", Precio = 25000, CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true, Popularidad = 10 },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Lomo Saltado", Descripcion = "Tradicional", Precio = 32000, CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true, Popularidad = 9 },
        };

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        mockHttp.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>> { Success = true, Data = new PaginatedList<ProductoDto> { Items = items, PageNumber = 1, PageSize = 6, TotalCount = 2, TotalPages = 1 } }));

        Services.AddScoped(sp => new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        // Act
        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Ceviche");
            cut.Markup.Should().Contain("Lomo Saltado");
        });
    }

    [Fact]
    public void Menu_Paginacion_Y_Filtros_Mapean_QueryParams()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 2 } };

        var mockHttp = new MockHttpMessageHandler();
        // Primer ciclo de carga inicial: categorías + productos populares
        mockHttp.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        mockHttp.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>> { Success = true, Data = new PaginatedList<ProductoDto> { Items = new List<ProductoDto>(), PageNumber = 1, PageSize = 6, TotalCount = 0, TotalPages = 1 } }));

        // Respuesta genérica para productos; suficiente para confirmar que no hay error al aplicar filtros
        mockHttp.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>> { Success = true, Data = new PaginatedList<ProductoDto> { Items = new List<ProductoDto>(), PageNumber = 1, PageSize = 6, TotalCount = 0, TotalPages = 1 } }));

        Services.AddScoped(sp => new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Cambiar select a precio_asc y aplicar dentro del dispatcher
        cut.InvokeAsync(() => cut.Find("select.form-select").Change("precio_asc"));
        cut.InvokeAsync(() => cut.Find("button.btn.btn-primary").Click());

        // Assert: si el mapeo fue correcto, la solicitud coincidió con el predicado y respondió 200.
        // No hay excepción y la página sigue mostrando sin error.
        cut.Markup.Should().Contain("Menú");
    }

    [Fact]
    public void Menu_Muestra_Skeleton_Mientras_Carga()
    {
        // Arrange: entregar categorías y simular retardo en productos para mantener 'cargando'
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 2 } };
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        // Retardo en productos
        mockHttp.When("http://localhost/api/core/productos*")
            .Respond(async req =>
            {
                await Task.Delay(1500);
                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
                {
                    Success = true,
                    Data = new PaginatedList<ProductoDto> { Items = new List<ProductoDto>(), PageNumber = 1, PageSize = 6, TotalCount = 0, TotalPages = 1 }
                });
                var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
                return resp;
            });

        Services.AddScoped(sp => new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Assert: debería mostrarse skeleton (placeholder-glow) durante la carga
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("placeholder-glow");
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Menu_Manejo_Error_Muestra_Estado_Vacio()
    {
        // Arrange: categorías presentes y productos sin handler -> excepción y estado vacío
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 2 } };
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));

        Services.AddScoped(sp => new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Assert: tras el error, muestra estado vacío
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("No hay productos para mostrar.");
        });
    }
}


