// ✅ Usando GlobalUsings.cs - Las importaciones están disponibles globalmente

namespace RestaurantePro.Api.IntegrationTests.Controllers.Core;

/// <summary>
/// Tests de integración para NotificacionesController
/// Contexto: Core
/// </summary>
[Collection("Sequential")]
public class NotificacionesControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    
    public NotificacionesControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetNotificaciones_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetNotificaciones_DebeRetornar501NotImplemented");

        // Act
        var response = await HttpClient.GetAsync("/api/core/notificaciones");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<NotificacionDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementada");
        apiResponse.Errors.Should().Contain("Endpoint no implementado aún");
        
        Logger.LogInformation("✅ Test completado - endpoint correctamente marcado como no implementado");
    }

    [Fact]
    public async Task GetNotificaciones_ConParametroSoloNoLeidas_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetNotificaciones_ConParametroSoloNoLeidas_DebeRetornar501NotImplemented");

        // Act
        var response = await HttpClient.GetAsync("/api/core/notificaciones?soloNoLeidas=true");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<NotificacionDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementada");
        
        Logger.LogInformation("✅ Test completado - parámetro soloNoLeidas manejado correctamente");
    }

    [Fact]
    public async Task GetNotificacion_ConIdEspecifico_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetNotificacion_ConIdEspecifico_DebeRetornar501NotImplemented");
        var idNotificacion = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/core/notificaciones/{idNotificacion}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<NotificacionDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementada");
        
        Logger.LogInformation("✅ Test completado - GET por ID correctamente marcado como no implementado");
    }

    [Fact]
    public async Task PostNotificacion_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostNotificacion_DebeRetornar501NotImplemented");
        
        var command = new CrearNotificacionCommand
        {
            Titulo = "Notificación de prueba",
            Mensaje = "Este es un mensaje de prueba",
            Tipo = "Informativa",
            DestinatarioId = Guid.NewGuid()
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", command);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<NotificacionDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementada");
        
        Logger.LogInformation("✅ Test completado - POST correctamente marcado como no implementado");
    }

    [Fact]
    public async Task MarcarComoLeida_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: MarcarComoLeida_DebeRetornar501NotImplemented");
        var idNotificacion = Guid.NewGuid();

        // Act
        var response = await HttpClient.PatchAsync($"/api/core/notificaciones/{idNotificacion}/marcar-leida", null);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementada");
        
        Logger.LogInformation("✅ Test completado - PATCH marcar-leida correctamente marcado como no implementado");
    }

    [Fact]
    public async Task MarcarTodasComoLeidas_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: MarcarTodasComoLeidas_DebeRetornar501NotImplemented");

        // Act
        var response = await HttpClient.PatchAsync("/api/core/notificaciones/marcar-todas-leidas", null);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementada");
        
        Logger.LogInformation("✅ Test completado - PATCH marcar-todas-leidas correctamente marcado como no implementado");
    }

    [Fact]
    public async Task GetContadorNoLeidas_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetContadorNoLeidas_DebeRetornar501NotImplemented");

        // Act
        var response = await HttpClient.GetAsync("/api/core/notificaciones/contador-no-leidas");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementada");
        
        Logger.LogInformation("✅ Test completado - GET contador-no-leidas correctamente marcado como no implementado");
    }

    [Fact]
    public async Task DeleteNotificacion_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: DeleteNotificacion_DebeRetornar501NotImplemented");
        var idNotificacion = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/core/notificaciones/{idNotificacion}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementada");
        
        Logger.LogInformation("✅ Test completado - DELETE correctamente marcado como no implementado");
    }

    public new void Dispose()
    {
        // Cleanup específico para NotificacionesController si es necesario
        Logger.LogInformation("🧹 Limpieza de NotificacionesControllerTests completada");
        _factory?.Dispose();
        base.Dispose();
    }
}

// ===================================================================
// DTOs TEMPORALES PARA TESTS
// ===================================================================
// Estos deben coincidir con los DTOs temporales del controlador

/// <summary>
/// DTO temporal para tests de notificaciones
/// </summary>
public class NotificacionDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLectura { get; set; }
    public bool EstaLeida { get; set; }
    public Guid? EntidadRelacionadaId { get; set; }
}

/// <summary>
/// Command temporal para tests de crear notificaciones
/// </summary>
public class CrearNotificacionCommand
{
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Informativa";
    public Guid DestinatarioId { get; set; }
    public Guid? EntidadRelacionadaId { get; set; }
} 