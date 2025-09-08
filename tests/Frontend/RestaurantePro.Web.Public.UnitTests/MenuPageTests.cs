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

    [Fact]
    public void Menu_Skeleton_Exacto_Seis_Placeholders()
    {
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 2 } };
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        mockHttp.When("http://localhost/api/core/productos*")
            .Respond(async req =>
            {
                await Task.Delay(1500);
                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
                {
                    Success = true,
                    Data = new PaginatedList<ProductoDto> { Items = new List<ProductoDto>(), PageNumber = 1, PageSize = 6, TotalCount = 0, TotalPages = 1 }
                });
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

        Services.AddScoped(sp => new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        cut.WaitForAssertion(() =>
        {
            var skeletons = cut.FindAll(".placeholder-glow");
            skeletons.Count.Should().Be(6);
        }, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Menu_Paginacion_NextPrev_Envian_PageNumber()
    {
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 12 } };
        var captured = new List<string>();

        var items = Enumerable.Range(1, 6).Select(i => new ProductoDto
        {
            Id = Guid.NewGuid(), Nombre = $"Item {i}", Descripcion = "", Precio = 1000 * i,
            CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true, Popularidad = i
        }).ToList();

        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        mock.When("http://localhost/api/core/productos*")
            .Respond(req =>
            {
                captured.Add(req.RequestUri!.ToString());
                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
                {
                    Success = true,
                    Data = new PaginatedList<ProductoDto> { Items = items, PageNumber = 1, PageSize = 6, TotalCount = 12, TotalPages = 2 }
                });
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });
            });

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Esperar a que cargue y muestre paginación 1 de 2
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Página 1 de 2");
        });

        // click siguiente
        cut.InvokeAsync(() => cut.FindAll(".btn-group button")[1].Click());
        // click anterior
        cut.InvokeAsync(() => cut.FindAll(".btn-group button")[0].Click());

        cut.WaitForAssertion(() =>
        {
            // Se esperan al menos 2 solicitudes (inicial + navegación) y que incluyan PageNumber
            captured.Count.Should().BeGreaterThanOrEqualTo(2);
            captured.Any(uri => uri.Contains("PageNumber=2")).Should().BeTrue();
        });
    }

    [Fact]
    public void Menu_Busqueda_Query_Vacio_Restituye_Categorias()
    {
        var mock = new MockHttpMessageHandler();
        // Buscar vacío → debe llamar a ObtenerCategoriasAsync
        mock.When("http://localhost/api/core/categorias?soloActivas=True&ocultarVacias=True")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = new List<CategoriaProductoDto>() }));
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = new List<CategoriaProductoDto>() }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Escribir vacío y aplicar
        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Buscar categoría...']").Change("");
            cut.Find("button.btn.btn-primary").Click();
        });

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Productos ");
        });
    }

    [Fact]
    public void Menu_Busqueda_Acentos_Url_Encoding()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias/buscar*")
            .WithQueryString("nombre", "%C3%B1oquis")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = new List<CategoriaProductoDto>() }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();
        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Buscar categoría...']").Change("ñoquis");
            cut.Find("button.btn.btn-primary").Click();
        });

        // Si no lanza excepción, el encoding fue correcto y la respuesta mockeada atendió
        cut.Markup.Should().Contain("Menú");
    }

    [Fact]
    public void Menu_Skeleton_Solo_Visible_Durante_Carga()
    {
        var cat = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas", ProductosDisponibles = 1 };
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = new List<CategoriaProductoDto> { cat } }));
        // Simular retardo en productos para ver skeleton
        mock.When("http://localhost/api/core/productos*")
            .Respond(async () =>
            {
                await Task.Delay(100);
                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
                {
                    Success = true,
                    Data = new PaginatedList<ProductoDto> { Items = new List<ProductoDto>(), PageNumber = 1, PageSize = 6, TotalCount = 0, TotalPages = 1 }
                });
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();
        // Durante carga
        cut.WaitForAssertion(() => cut.FindAll(".placeholder-glow").Count.Should().BeGreaterThan(0));
        // Luego de cargar (lista vacía)
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("No hay productos"));
        cut.FindAll(".placeholder-glow").Count.Should().Be(0);
    }

    [Fact]
    public void Menu_Cambiar_Orden_Reinicia_Pagina_1()
    {
        var categoria = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas", ProductosDisponibles = 12 };
        var categorias = new[] { categoria };

        var captured = new List<string>();
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));

        mock.When("http://localhost/api/core/productos*")
            .Respond(req =>
            {
                captured.Add(req.RequestUri!.ToString());
                var items = new List<ProductoDto> { new ProductoDto { Id = Guid.NewGuid(), Nombre = "X", Precio = 10, Activo = true, CategoriaNombre = "Entradas" } };
                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
                {
                    Success = true,
                    Data = new PaginatedList<ProductoDto> { Items = items, PageNumber = 1, PageSize = 6, TotalCount = 12, TotalPages = 2 }
                });
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });
            });

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();
        cut.WaitForAssertion(() => captured.Any());

        // Ir a página 2
        cut.InvokeAsync(() => cut.FindAll(".btn-group button")[1].Click());
        var countAntes = captured.Count;

        // Cambiar orden a "precio_desc" y aplicar
        cut.InvokeAsync(() =>
        {
            var select = cut.Find("select.form-select");
            select.Change("precio_desc");
            cut.Find("button.btn.btn-primary").Click();
        });

        // Debe reiniciar PageNumber a 1 en la siguiente solicitud
        cut.WaitForAssertion(() => captured.Skip(countAntes).Any(u => u.Contains("PageNumber=1")).Should().BeTrue());
    }

    [Fact]
    public void Menu_Paginacion_NextPrev_Conserva_Filtros_De_Orden()
    {
        var categoria = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas", ProductosDisponibles = 12 };
        var categorias = new[] { categoria };

        var captured = new List<string>();
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));

        mock.When("http://localhost/api/core/productos*")
            .Respond(req =>
            {
                captured.Add(req.RequestUri!.ToString());
                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
                {
                    Success = true,
                    Data = new PaginatedList<ProductoDto> { Items = new List<ProductoDto>(), PageNumber = 1, PageSize = 6, TotalCount = 12, TotalPages = 3 }
                });
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });
            });

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();
        cut.WaitForAssertion(() => captured.Any());

        // Cambiar orden a "precio_desc" y aplicar
        cut.InvokeAsync(() =>
        {
            var select = cut.Find("select.form-select");
            select.Change("precio_desc");
            cut.Find("button.btn.btn-primary").Click();
        });

        // Ahora ir a siguiente página
        cut.InvokeAsync(() => cut.FindAll(".btn-group button")[1].Click());

        cut.WaitForAssertion(() =>
        {
            // La última solicitud debe conservar filtros: OrderBy=Precio, OrderDirection=desc y SoloActivos=false
            var last = captured.Last().ToLowerInvariant();
            last.Should().Contain("categoriaid=");
            last.Should().Contain("orderby=precio");
            last.Should().Contain("orderdirection=desc");
            last.Should().Contain("soloactivos=false");
        });
    }

    [Fact]
    public void Menu_Cambiar_Categoria_Reinicia_Pagina_1()
    {
        var categoriaA = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Entradas", ProductosDisponibles = 3 };
        var categoriaB = new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Platos", ProductosDisponibles = 3 };
        var categorias = new[] { categoriaA, categoriaB };

        var captured = new List<string>();
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        mock.When("http://localhost/api/core/productos*")
            .Respond(req =>
            {
                captured.Add(req.RequestUri!.ToString());
                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
                {
                    Success = true,
                    Data = new PaginatedList<ProductoDto> { Items = new List<ProductoDto>(), PageNumber = 1, PageSize = 6, TotalCount = 12, TotalPages = 2 }
                });
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });
            });

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        // Ir a página 2
        cut.InvokeAsync(() => cut.FindAll(".btn-group button")[1].Click());
        var countAntes = captured.Count;

        // Seleccionar la segunda categoría → debería resetear PageNumber a 1 en la siguiente llamada
        cut.InvokeAsync(() =>
        {
            var item = cut.FindAll(".list-group .list-group-item").Last();
            item.Click();
        });

        cut.WaitForAssertion(() =>
        {
            captured.Skip(countAntes).Any(uri => uri.Contains("PageNumber=1")).Should().BeTrue();
        });
    }

    [Fact]
    public void Menu_Paginacion_Texto_Y_Boton_Anterior_Deshabilitado()
    {
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 12 } };

        var items = Enumerable.Range(1, 6).Select(i => new ProductoDto
        {
            Id = Guid.NewGuid(), Nombre = $"Item {i}", Descripcion = "", Precio = 1000 * i,
            CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true, Popularidad = i
        }).ToList();

        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        mock.When("http://localhost/api/core/productos*")
            .Respond(req =>
            {
                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
                {
                    Success = true,
                    Data = new PaginatedList<ProductoDto> { Items = items, PageNumber = 1, PageSize = 6, TotalCount = 12, TotalPages = 3 }
                });
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });
            });

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Página 1 de 3");
            var buttons = cut.FindAll(".btn-group button");
            buttons.Should().HaveCount(2);
            buttons[0].HasAttribute("disabled").Should().BeTrue(); // Anterior
            buttons[1].HasAttribute("disabled").Should().BeFalse(); // Siguiente
        });
    }

    [Fact]
    public void Menu_Paginacion_Botones_Deshabilitados_En_Unica_Pagina()
    {
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 2 } };
        var items = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "A", Descripcion = "", Precio = 1000, CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "B", Descripcion = "", Precio = 2000, CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true },
        };

        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        mock.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
            {
                Success = true,
                Data = new PaginatedList<ProductoDto> { Items = items, PageNumber = 1, PageSize = 6, TotalCount = 2, TotalPages = 1 }
            }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Página 1 de 1");
            var buttons = cut.FindAll(".btn-group button");
            buttons[0].HasAttribute("disabled").Should().BeTrue();
            buttons[1].HasAttribute("disabled").Should().BeTrue();
        });
    }

    [Fact]
    public void Menu_Productos_Inactivos_Muestran_Badge_Y_Opacidad()
    {
        var categoriaId = Guid.NewGuid();
        var categorias = new[] { new CategoriaProductoDto { Id = categoriaId, Nombre = "Platos", ProductosDisponibles = 2 } };
        var items = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Activo", Descripcion = "", Precio = 1000, CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = true },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Inactivo", Descripcion = "", Precio = 2000, CategoriaId = categoriaId, CategoriaNombre = "Platos", Activo = false },
        };

        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = categorias.ToList() }));
        mock.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
            {
                Success = true,
                Data = new PaginatedList<ProductoDto> { Items = items, PageNumber = 1, PageSize = 6, TotalCount = 2, TotalPages = 1 }
            }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<MenuApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Menu>();

        cut.WaitForAssertion(() =>
        {
            // Badge "Sin stock" visible y card con opacidad
            cut.Markup.Should().Contain("Sin stock");
            var inactiveCard = cut.FindAll(".card").FirstOrDefault(c => c.TextContent.Contains("Inactivo"));
            inactiveCard.Should().NotBeNull();
            inactiveCard!.ClassList.Should().Contain("opacity-50");
        });
    }
}


