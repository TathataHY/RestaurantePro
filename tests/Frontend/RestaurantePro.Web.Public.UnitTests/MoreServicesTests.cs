using FluentAssertions;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Models;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.UnitTests;

public class MoreServicesTests
{
    [Fact]
    public async Task ReviewsApiService_Get_Y_Post()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/public/reviews")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ReviewDto>> { Success = true, Data = new List<ReviewDto> { new ReviewDto { Nombre = "Ana", Comentario = "Ok", Valoracion = 5, Fecha = DateTime.UtcNow } } }));
        mock.When(HttpMethod.Post, "http://localhost/api/public/reviews")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<ReviewDto> { Success = true, Data = new ReviewDto { Nombre = "Juan", Comentario = "Nice", Valoracion = 5, Fecha = DateTime.UtcNow } }));

        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new ReviewsApiService(http);

        var list = await svc.ObtenerAsync();
        list.Should().HaveCount(1);
        list[0].Nombre.Should().Be("Ana");

        var ok = await svc.CrearAsync(new CreateReviewRequest { Nombre = "Juan", Comentario = "Nice", Valoracion = 5 });
        ok.Should().BeTrue();
    }

    [Fact]
    public async Task MenuApiService_Mapea_Query_Paginados_Y_Defaults()
    {
        var categoriaId = Guid.NewGuid();
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/core/productos*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>> { Success = true, Data = new PaginatedList<ProductoDto> { Items = new List<ProductoDto>(), PageNumber = 2, PageSize = 6, TotalCount = 0, TotalPages = 1 } }));

        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new MenuApiService(http);

        var page = await svc.ObtenerProductosPaginadosAsync(categoriaId, 2, 6, false, "Precio", "desc");
        page.PageNumber.Should().Be(2);
        page.Items.Should().NotBeNull();

        mock.ResetExpectations();
        mock.When("http://localhost/api/core/categorias*")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>> { Success = true, Data = null }));
        var categorias = await svc.ObtenerCategoriasAsync();
        categorias.Should().NotBeNull();
        categorias.Should().HaveCount(0);
    }

    [Fact]
    public async Task ContactApiService_Exito_Y_Error()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/api/public/contact/messages")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<ContactMessageDto> { Success = true, Data = new ContactMessageDto { Nombre = "Ana", Email = "a@a.com", Asunto = "Hi", Mensaje = "Hola", Fecha = DateTime.UtcNow } }));
        mock.When(HttpMethod.Get, "http://localhost/api/public/contact/messages")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ContactMessageDto>> { Success = true, Data = new List<ContactMessageDto>() }));

        var http = new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") };
        var svc = new ContactApiService(http);

        var ok = await svc.EnviarAsync(new CreateContactMessageRequest { Nombre = "Ana", Email = "a@a.com", Mensaje = "Hola mundo" });
        ok.Should().BeTrue();

        // Nuevo handler para simular error en una nueva instancia de servicio
        var mockError = new MockHttpMessageHandler();
        mockError.When(HttpMethod.Post, "http://localhost/api/public/contact/messages")
            .Respond(System.Net.HttpStatusCode.BadRequest);
        var httpError = new HttpClient(mockError) { BaseAddress = new Uri("http://localhost/") };
        var svcError = new ContactApiService(httpError);

        ok = await svcError.EnviarAsync(new CreateContactMessageRequest { Nombre = "Ana", Email = "a@a.com", Mensaje = "Hola mundo" });
        ok.Should().BeFalse();
    }
}


