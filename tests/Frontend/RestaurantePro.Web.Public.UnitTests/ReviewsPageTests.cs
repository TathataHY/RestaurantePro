using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Models;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.UnitTests;

public class ReviewsPageTests : TestContext
{
    [Fact]
    public void Reviews_ListaInicial_RenderizaResenas()
    {
        var reviews = new List<ReviewDto>
        {
            new ReviewDto { Nombre = "Ana", Comentario = "Excelente", Valoracion = 5, Fecha = DateTime.UtcNow },
            new ReviewDto { Nombre = "Luis", Comentario = "Muy bueno", Valoracion = 4, Fecha = DateTime.UtcNow }
        };

        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/public/reviews")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ReviewDto>> { Success = true, Data = reviews }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ReviewsApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Reviews>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Reseñas");
            cut.Markup.Should().Contain("Ana");
            cut.Markup.Should().Contain("Luis");
        });
    }

    [Fact]
    public void Reviews_Crear_Exito_ReseteaYRecarga()
    {
        var initial = new List<ReviewDto>
        {
            new ReviewDto { Nombre = "Ana", Comentario = "Excelente", Valoracion = 5, Fecha = DateTime.UtcNow }
        };
        var afterPost = new List<ReviewDto>
        {
            new ReviewDto { Nombre = "Juan", Comentario = "Muy rico", Valoracion = 5, Fecha = DateTime.UtcNow },
            initial[0]
        };

        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/public/reviews")
            .Respond(req =>
            {
                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ReviewDto>> { Success = true, Data = afterPost });
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });
            });
        mock.When(HttpMethod.Post, "http://localhost/api/public/reviews")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<ReviewDto> { Success = true, Data = new ReviewDto { Nombre = "Juan", Comentario = "Muy rico", Valoracion = 5, Fecha = DateTime.UtcNow } }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ReviewsApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Reviews>();

        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Tu nombre']").Change("Juan");
            cut.Find("input[placeholder='Valoración (1-5)']").Change("5");
            cut.Find("textarea[placeholder='Escribe tu comentario...']").Change("Muy rico");
            cut.Find("form").Submit();
        });

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Juan");
            cut.Markup.Should().Contain("Muy rico");
            // La primera reseña debe ser la nueva (API retorna lista con nuevo primero)
            var titulos = cut.FindAll("h5.card-title").Select(e => e.TextContent).ToList();
            titulos.First().Should().Contain("Juan");
        });
    }

    [Fact]
    public void Reviews_Crear_Error_NoCambiaLista()
    {
        var initial = new List<ReviewDto>
        {
            new ReviewDto { Nombre = "Ana", Comentario = "Excelente", Valoracion = 5, Fecha = DateTime.UtcNow }
        };

        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/public/reviews")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ReviewDto>> { Success = true, Data = initial }));
        mock.When(HttpMethod.Post, "http://localhost/api/public/reviews")
            .Respond(System.Net.HttpStatusCode.BadRequest);

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ReviewsApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Reviews>();

        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Tu nombre']").Change("Juan");
            cut.Find("input[placeholder='Valoración (1-5)']").Change("5");
            cut.Find("textarea[placeholder='Escribe tu comentario...']").Change("Muy rico");
            cut.Find("form").Submit();
        });

        cut.WaitForAssertion(() =>
        {
            // La lista debe seguir mostrando solo la reseña inicial "Ana" (evitar inputs con valor "Juan")
            var titulos = cut.FindAll("h5.card-title").Select(e => e.TextContent);
            titulos.Should().Contain(t => t.Contains("Ana"));
            titulos.Should().NotContain(t => t.Contains("Juan"));
        });
    }

    [Fact]
    public void Reviews_Crear_Error_RetornaFalse_Y_Boton_Rehabilitado()
    {
        var initial = new List<ReviewDto>
        {
            new ReviewDto { Nombre = "Ana", Comentario = "Excelente", Valoracion = 5, Fecha = DateTime.UtcNow }
        };

        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/public/reviews")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ReviewDto>> { Success = true, Data = initial }));
        mock.When(HttpMethod.Post, "http://localhost/api/public/reviews")
            .Respond(System.Net.HttpStatusCode.BadRequest);

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ReviewsApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Reviews>();

        // Llenar y enviar
        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Tu nombre']").Change("Juan");
            cut.Find("input[placeholder='Valoración (1-5)']").Change("5");
            cut.Find("textarea[placeholder='Escribe tu comentario...']").Change("Muy rico");
            cut.Find("form").Submit();
        });

        // Verifica que el botón quede habilitado (no sigue en estado 'Enviando...')
        cut.WaitForAssertion(() =>
        {
            var btn = cut.Find("button.btn.btn-primary");
            btn.HasAttribute("disabled").Should().BeFalse();
            btn.TextContent.Should().Be("Enviar reseña");
        });
    }

    [Fact]
    public void Reviews_Boton_Deshabilitado_Durante_Envio()
    {
        var mock = new MockHttpMessageHandler();
        // GET inicial vacío
        mock.When(HttpMethod.Get, "http://localhost/api/public/reviews")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ReviewDto>> { Success = true, Data = new List<ReviewDto>() }));
        // POST con retardo para verificar estado 'enviando'
        mock.When(HttpMethod.Post, "http://localhost/api/public/reviews")
            .Respond(async req =>
            {
                await Task.Delay(200);
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            });

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ReviewsApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Reviews>();

        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Tu nombre']").Change("Juan");
            cut.Find("input[placeholder='Valoración (1-5)']").Change("5");
            cut.Find("textarea[placeholder='Escribe tu comentario...']").Change("Muy rico");
            cut.Find("form").Submit();
        });

        // Durante envío
        cut.WaitForAssertion(() =>
        {
            var btn = cut.Find("button.btn.btn-primary");
            btn.HasAttribute("disabled").Should().BeTrue();
            btn.TextContent.Should().Contain("Enviando");
        }, TimeSpan.FromMilliseconds(150));
    }
}


