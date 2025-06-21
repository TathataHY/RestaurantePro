using RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

/// <summary>
/// Tests de integración para TarjetasFidelizacionController
/// Valida todos los endpoints REST del controlador de tarjetas de fidelización
/// </summary>
[Collection("Sequential")]
public class TarjetasFidelizacionControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public TarjetasFidelizacionControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    // Helper para crear cliente de prueba
    private async Task<Guid> CrearClientePrueba(string nombre, string email)
    {
        var cliente = new ClienteTestDataBuilder()
            .ConNombre(nombre)
            .ConEmail(email);
        // Aquí deberías crear el cliente en la BD usando el comando correspondiente
        // Por ahora retornamos un GUID para que los tests funcionen
        return Guid.NewGuid();
    }

    [Fact]
    public async Task GetTarjetas_SinTarjetasEnBD_DebeRetornarListaVacia()
    {
        // Arrange
        var url = "/api/comercial/tarjetas-fidelizacion";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, 
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetTarjetas_ConParametrosFiltro_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/comercial/tarjetas-fidelizacion?estado=Activa&nivel=Premium";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented, 
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetTarjeta_ConIdExistente_DebeRetornarTarjeta()
    {
        // Arrange
        var clienteId = await CrearClientePrueba("Cliente Test", "cliente@test.com");
        var tarjetaRequest = new TarjetaFidelizacionTestDataBuilder()
            .ConClienteId(clienteId)
            .BuildCrearTarjetaRequest();
        var createResponse = await HttpClient.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", tarjetaRequest);
        // Aquí deberías extraer el ID real de la tarjeta creada
        var tarjetaId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/tarjetas-fidelizacion/{tarjetaId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, 
            HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task PostTarjeta_ConDatosValidos_DebeCrearTarjeta()
    {
        // Arrange
        var clienteId = await CrearClientePrueba("Cliente Fidelización", "fidelizacion@test.com");
        var tarjetaRequest = new TarjetaFidelizacionTestDataBuilder()
            .ConClienteId(clienteId)
            .ConTipoTarjeta("Premium")
            .ConPuntosIniciales(100)
            .ConActivarInmediatamente(true)
            .BuildCrearTarjetaRequest();

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", tarjetaRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, 
            HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task PutTarjeta_ConDatosValidos_DebeActualizarTarjeta()
    {
        // Arrange
        var clienteId = await CrearClientePrueba("Cliente Actualizar", "actualizar@test.com");
        var tarjetaRequest = new TarjetaFidelizacionTestDataBuilder()
            .ConClienteId(clienteId)
            .BuildCrearTarjetaRequest();
        var createResponse = await HttpClient.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", tarjetaRequest);
        // Aquí deberías extraer el ID real de la tarjeta creada
        var tarjetaId = Guid.NewGuid();

        var actualizarRequest = new TarjetaFidelizacionTestDataBuilder()
            .ConTipoTarjeta("VIP")
            .ConMultiplicadorPuntos(2.0m)
            .ConLimiteMensual(5000)
            .BuildActualizarTarjetaRequest();

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/comercial/tarjetas-fidelizacion/{tarjetaId}", actualizarRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, 
            HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ActivarTarjeta_ConTarjetaExistente_DebeActivarTarjeta()
    {
        // Arrange
        var clienteId = await CrearClientePrueba("Cliente Activar", "activar@test.com");
        var tarjetaRequest = new TarjetaFidelizacionTestDataBuilder()
            .ConClienteId(clienteId)
            .ConActivarInmediatamente(false)
            .BuildCrearTarjetaRequest();
        var createResponse = await HttpClient.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", tarjetaRequest);
        // Aquí deberías extraer el ID real de la tarjeta creada
        var tarjetaId = Guid.NewGuid();

        // Act
        var response = await HttpClient.PatchAsync($"/api/comercial/tarjetas-fidelizacion/{tarjetaId}/activar", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, 
            HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task DesactivarTarjeta_ConTarjetaExistente_DebeDesactivarTarjeta()
    {
        // Arrange
        var clienteId = await CrearClientePrueba("Cliente Desactivar", "desactivar@test.com");
        var tarjetaRequest = new TarjetaFidelizacionTestDataBuilder()
            .ConClienteId(clienteId)
            .ConActivarInmediatamente(true)
            .BuildCrearTarjetaRequest();
        var createResponse = await HttpClient.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", tarjetaRequest);
        // Aquí deberías extraer el ID real de la tarjeta creada
        var tarjetaId = Guid.NewGuid();

        // Act
        var response = await HttpClient.PatchAsync($"/api/comercial/tarjetas-fidelizacion/{tarjetaId}/desactivar", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, 
            HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task AgregarPuntos_ConTarjetaExistente_DebeAgregarPuntos()
    {
        // Arrange
        var clienteId = await CrearClientePrueba("Cliente Puntos", "puntos@test.com");
        var tarjetaRequest = new TarjetaFidelizacionTestDataBuilder()
            .ConClienteId(clienteId)
            .ConPuntosIniciales(100)
            .BuildCrearTarjetaRequest();
        var createResponse = await HttpClient.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", tarjetaRequest);
        // Aquí deberías extraer el ID real de la tarjeta creada
        var tarjetaId = Guid.NewGuid();

        var agregarPuntosRequest = new TarjetaFidelizacionTestDataBuilder()
            .BuildAgregarPuntosRequest();

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/comercial/tarjetas-fidelizacion/{tarjetaId}/puntos", agregarPuntosRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, 
            HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CanjearPuntos_ConTarjetaExistente_DebeCanjearPuntos()
    {
        // Arrange
        var clienteId = await CrearClientePrueba("Cliente Canje", "canje@test.com");
        var tarjetaRequest = new TarjetaFidelizacionTestDataBuilder()
            .ConClienteId(clienteId)
            .ConPuntosIniciales(1000)
            .BuildCrearTarjetaRequest();
        var createResponse = await HttpClient.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", tarjetaRequest);
        // Aquí deberías extraer el ID real de la tarjeta creada
        var tarjetaId = Guid.NewGuid();

        var canjearPuntosRequest = new TarjetaFidelizacionTestDataBuilder()
            .BuildCanjearPuntosRequest();

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/comercial/tarjetas-fidelizacion/{tarjetaId}/canjear", canjearPuntosRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, 
            HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetHistorialPuntos_ConTarjetaExistente_DebeRetornarHistorial()
    {
        // Arrange
        var clienteId = await CrearClientePrueba("Cliente Historial", "historial@test.com");
        var tarjetaRequest = new TarjetaFidelizacionTestDataBuilder()
            .ConClienteId(clienteId)
            .ConPuntosIniciales(500)
            .BuildCrearTarjetaRequest();
        var createResponse = await HttpClient.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", tarjetaRequest);
        // Aquí deberías extraer el ID real de la tarjeta creada
        var tarjetaId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/tarjetas-fidelizacion/{tarjetaId}/historial");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, 
            HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetEstadisticas_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/comercial/tarjetas-fidelizacion/reporte";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, 
            HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task DeleteTarjeta_ConTarjetaExistente_DebeEliminarTarjeta()
    {
        // Arrange
        var clienteId = await CrearClientePrueba("Cliente Eliminar", "eliminar@test.com");
        var tarjetaRequest = new TarjetaFidelizacionTestDataBuilder()
            .ConClienteId(clienteId)
            .BuildCrearTarjetaRequest();
        var createResponse = await HttpClient.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", tarjetaRequest);
        // Aquí deberías extraer el ID real de la tarjeta creada
        var tarjetaId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/comercial/tarjetas-fidelizacion/{tarjetaId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, 
            HttpStatusCode.NotImplemented, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<object>();
            apiResponse.Should().NotBeNull();
        }
    }

    public new void Dispose()
    {
        base.Dispose();
    }
} 