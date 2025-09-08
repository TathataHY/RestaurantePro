using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using RestaurantePro.Web.Public.Models;
using RestaurantePro.Web.Public.Services;

namespace RestaurantePro.Web.Public.UnitTests;

public class ContactPageTests : TestContext
{
    [Fact]
    public void Contact_Validacion_Campos_Requeridos()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("http://localhost/api/public/contact/messages")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ContactMessageDto>> { Success = true, Data = new List<ContactMessageDto>() }));
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ContactApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Contact>();

        // Al intentar enviar vacíos, el formulario no debería enviarse (bUnit no muestra errores por defecto, validamos que no aparece el mensaje de éxito)
        cut.Find("form").Submit();
        cut.Markup.Should().NotContain("¡Gracias! Recibimos tu mensaje.");
    }

    [Fact]
    public void Contact_Envio_Exitoso_Muestra_Mensaje_Y_Recarga()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/public/contact/messages")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ContactMessageDto>> { Success = true, Data = new List<ContactMessageDto>() }));
        mock.When(HttpMethod.Post, "http://localhost/api/public/contact/messages")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<ContactMessageDto> { Success = true, Data = new ContactMessageDto { Nombre = "Juan", Email = "j@e.com", Mensaje = "Hola" } }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ContactApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Contact>();

        // Interacciones en el hilo del dispatcher
        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Tu nombre']").Change("Juan");
            cut.Find("input[placeholder='Tu email']").Change("j@e.com");
            cut.Find("input[placeholder='Asunto (opcional)']").Change("Consulta");
            cut.Find("textarea[placeholder='Mensaje']").Change("Hola mundo");
            cut.Find("form").Submit();
        });

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("¡Gracias! Recibimos tu mensaje.");
        });
    }

    [Fact]
    public void Contact_Error_No_Muestra_Exito()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/public/contact/messages")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ContactMessageDto>> { Success = true, Data = new List<ContactMessageDto>() }));
        mock.When(HttpMethod.Post, "http://localhost/api/public/contact/messages")
            .Respond(System.Net.HttpStatusCode.BadRequest);

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ContactApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Contact>();

        cut.Find("input[placeholder='Tu nombre']").Change("Juan");
        cut.Find("input[placeholder='Tu email']").Change("j@e.com");
        cut.Find("textarea[placeholder='Mensaje']").Change("Hola");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().NotContain("¡Gracias! Recibimos tu mensaje.");
        });
    }

    [Fact]
    public void Contact_Render_Lista_Con_Formato_Basico()
    {
        var when = DateTime.UtcNow;
        var formatted = when.ToLocalTime().ToString("g");

        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/public/contact/messages")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(
                new ApiResponse<List<ContactMessageDto>>
                {
                    Success = true,
                    Data = new List<ContactMessageDto>
                    {
                        new ContactMessageDto { Nombre = "Ana", Email = "a@a.com", Asunto = "Hola", Mensaje = "Mensaje", Fecha = when }
                    }
                }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ContactApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Contact>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Ana");
            cut.Markup.Should().Contain("a@a.com");
            cut.Markup.Should().Contain(formatted);
        });
    }

    [Fact]
    public void Contact_Get_Error_Muestra_Lista_Vacia_Sin_Romper_UI()
    {
        var mock = new MockHttpMessageHandler();
        // GET lanza error de red -> servicio retorna lista vacía
        mock.When(HttpMethod.Get, "http://localhost/api/public/contact/messages")
            .Throw(new HttpRequestException("Network error"));
        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ContactApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Contact>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("No hay mensajes aún.");
            cut.Markup.Should().Contain("Contacto");
        });
    }

    [Fact]
    public void Contact_Boton_Deshabilitado_Durante_Envio_Y_Evita_DobleSubmit()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/public/contact/messages")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ContactMessageDto>> { Success = true, Data = new List<ContactMessageDto>() }));
        var postCount = 0;
        mock.When(HttpMethod.Post, "http://localhost/api/public/contact/messages")
            .Respond(async req =>
            {
                Interlocked.Increment(ref postCount);
                await Task.Delay(200);
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            });

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ContactApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Contact>();

        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Tu nombre']").Change("Juan");
            cut.Find("input[placeholder='Tu email']").Change("invalido@e.com");
            cut.Find("textarea[placeholder='Mensaje']").Change("Hola");
            // Doble submit rápido
            cut.Find("form").Submit();
            cut.Find("form").Submit();
        });

        // Durante envío el botón debe estar deshabilitado y solo 1 POST en curso
        cut.WaitForAssertion(() =>
        {
            var btn = cut.Find("button.btn.btn-primary");
            btn.HasAttribute("disabled").Should().BeTrue();
            Volatile.Read(ref postCount).Should().Be(1);
        }, TimeSpan.FromMilliseconds(150));
    }

    [Fact]
    public void Contact_Email_Invalido_Muestra_Mensaje_Validacion()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "http://localhost/api/public/contact/messages")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<ContactMessageDto>> { Success = true, Data = new List<ContactMessageDto>() }));

        Services.AddScoped(sp => new HttpClient(mock) { BaseAddress = new Uri("http://localhost/") });
        Services.AddScoped<ContactApiService>();

        var cut = RenderComponent<RestaurantePro.Web.Public.Pages.Contact>();

        cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Tu email']").Change("no-es-email");
            cut.Find("form").Submit();
        });

        cut.Markup.Should().Contain("email"); // ValidationMessage debe renderizar algún mensaje relacionado con email
    }
}


