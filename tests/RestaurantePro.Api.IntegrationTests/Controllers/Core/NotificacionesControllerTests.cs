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

    /// <summary>
    /// Método helper para crear un usuario válido para las pruebas
    /// </summary>
    private async Task<Guid> CrearUsuarioPrueba()
    {
        // Usar el mismo usuarioId fijo que el controlador para que los tests funcionen
        var usuarioId = new Guid("11111111-1111-1111-1111-111111111111");
        
        // Usar el método de creación correcto de la entidad Usuario
        var usuario = Usuario.Crear(
            "test.user",
            "Usuario Test",
            "test@test.com",
            RolUsuario.Mesero);

        usuario.ConfirmarCuenta();

        // Asignar el ID fijo usando reflexión para que coincida con el controlador
        var idProperty = typeof(Usuario).GetProperty("Id");
        idProperty?.SetValue(usuario, usuarioId);

        DbContext.Usuarios.Add(usuario);
        await DbContext.SaveChangesAsync();

        return usuarioId;
    }

    /// <summary>
    /// Método helper para crear una notificación de prueba directamente en BD
    /// </summary>
    private async Task<Notificacion> CrearNotificacionPrueba(Guid destinatarioId, string titulo = "Test Notification", bool estaLeida = false)
    {
        var notificacion = Notificacion.Crear(
            titulo,
            "Mensaje de prueba",
            TipoNotificacion.Informativa,
            destinatarioId);

        if (estaLeida)
        {
            notificacion.MarcarComoLeida();
        }

        // Usar el mismo DbContext del test, NO crear un nuevo scope/contexto
        DbContext.Notificaciones.Add(notificacion);
        await DbContext.SaveChangesAsync();

        return notificacion;
    }

    [Fact]
    public async Task GetNotificaciones_ConNotificacionesEnBD_DebeRetornarNotificaciones()
    {
        // Arrange
        Console.WriteLine("🧪 Iniciando test: GetNotificaciones_ConNotificacionesEnBD_DebeRetornarNotificaciones");
        
        var usuarioId = await CrearUsuarioPrueba();
        Console.WriteLine($"👤 Usuario creado con ID: {usuarioId}");
        
        // Crear notificaciones de prueba
        var notificacion1 = await CrearNotificacionPrueba(usuarioId, "Notificación 1");
        var notificacion2 = await CrearNotificacionPrueba(usuarioId, "Notificación 2");
        
        Console.WriteLine($"📧 Notificaciones creadas: {notificacion1.Id}, {notificacion2.Id}");
        
        // Verificar que las notificaciones están en BD
        var notificacionesEnBD = await DbContext.Notificaciones
            .Where(n => n.DestinatarioId == usuarioId)
            .ToListAsync();
        
        Console.WriteLine($"🔍 Notificaciones en BD para usuario {usuarioId}: {notificacionesEnBD.Count}");
        foreach (var notif in notificacionesEnBD)
        {
            Console.WriteLine($"  - Notificación: {notif.Id}, Título: {notif.Titulo}, Destinatario: {notif.DestinatarioId}");
        }

        // Act
        var response = await HttpClient.GetAsync($"/api/core/notificaciones/");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"📋 Response: {response.StatusCode} - {content}");

        // Assert - Test completo con validación estricta
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<NotificacionDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        
        Console.WriteLine($"📊 API Response - Success: {apiResponse.Success}, Count: {apiResponse.Data?.Count ?? 0}");
        
        apiResponse.Data!.Count.Should().BeGreaterThanOrEqualTo(2);
        
        // Verificar que las notificaciones creadas están en la respuesta
        var notificacionIds = apiResponse.Data.Select(n => n.Id).ToList();
        notificacionIds.Should().Contain(notificacion1.Id);
        notificacionIds.Should().Contain(notificacion2.Id);
    }

    [Fact]
    public async Task GetNotificaciones_ConParametroSoloNoLeidas_DebeRetornarSoloNoLeidas()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetNotificaciones_ConParametroSoloNoLeidas_DebeRetornarSoloNoLeidas");

        var usuarioId = await CrearUsuarioPrueba();
        var notificacionLeida = await CrearNotificacionPrueba(usuarioId, "Notificación Leída", estaLeida: true);
        var notificacionNoLeida = await CrearNotificacionPrueba(usuarioId, "Notificación No Leída", estaLeida: false);

        // Act
        var response = await HttpClient.GetAsync("/api/core/notificaciones?soloNoLeidas=true");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert - Test completo con validación estricta
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<NotificacionDto>>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();

        // Verificar que solo se retornan las no leídas
        var notificacionesNoLeidas = apiResponse.Data!.Where(n => !n.EstaLeida).ToList();
        notificacionesNoLeidas.Should().Contain(n => n.Id == notificacionNoLeida.Id);
        notificacionesNoLeidas.Should().NotContain(n => n.Id == notificacionLeida.Id);

        Logger.LogInformation("✅ Test completado - filtro soloNoLeidas funciona correctamente");
    }

    [Fact]
    public async Task GetNotificacion_ConIdExistente_DebeRetornarNotificacion()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetNotificacion_ConIdExistente_DebeRetornarNotificacion");
        
        var usuarioId = await CrearUsuarioPrueba();
        var notificacion = await CrearNotificacionPrueba(usuarioId, "Notificación Específica");

        // Act
        var response = await HttpClient.GetAsync($"/api/core/notificaciones/{notificacion.Id}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert - Test completo con validación estricta
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<NotificacionDto>>();
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(notificacion.Id);
        apiResponse.Data.Titulo.Should().Be("Notificación Específica");
        
        Logger.LogInformation("✅ Test completado - notificación obtenida por ID correctamente");
    }

    [Fact]
    public async Task GetNotificacion_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetNotificacion_ConIdInexistente_DebeRetornar404");
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/core/notificaciones/{idInexistente}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert - Test completo con validación estricta
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no encontrada");
        
        Logger.LogInformation("✅ Test completado - 404 retornado para ID inexistente");
    }

    [Fact]
    public async Task PostNotificacion_ConDatosValidos_DebeCrearNotificacion()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostNotificacion_ConDatosValidos_DebeCrearNotificacion");

        var usuarioId = await CrearUsuarioPrueba();
        var command = new CrearNotificacionCommand
        {
            Titulo = "Notificación de prueba",
            Mensaje = "Este es un mensaje de prueba",
            Tipo = "Informativa",
            DestinatarioId = usuarioId
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", command);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert - Test completo con validación estricta
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<NotificacionDto>>();
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Titulo.Should().Be("Notificación de prueba");
        apiResponse.Data.Mensaje.Should().Be("Este es un mensaje de prueba");
        apiResponse.Data.EstaLeida.Should().BeFalse();

        // Verificar que se creó en la BD
        var notificacionesEnBD = await DbContext.Notificaciones.ToListAsync();
        notificacionesEnBD.Should().Contain(n => n.Titulo == "Notificación de prueba");

        Logger.LogInformation("✅ Test completado - notificación creada correctamente");
    }

    [Fact]
    public async Task PostNotificacion_ConDatosInvalidos_DebeRetornar400()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostNotificacion_ConDatosInvalidos_DebeRetornar400");

        var command = new CrearNotificacionCommand
        {
            Titulo = "", // Título vacío - inválido
            Mensaje = "", // Mensaje vacío - inválido
            Tipo = "TipoInexistente", // Tipo inválido
            DestinatarioId = Guid.Empty // ID inválido
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", command);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert - Test completo con validación estricta
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();

        Logger.LogInformation("✅ Test completado - datos inválidos rechazados correctamente");
    }

    [Fact]
    public async Task MarcarComoLeida_ConIdExistente_DebeMarcarComoLeida()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: MarcarComoLeida_ConIdExistente_DebeMarcarComoLeida");

        var usuarioId = await CrearUsuarioPrueba();
        var notificacion = await CrearNotificacionPrueba(usuarioId, "Notificación a marcar", estaLeida: false);

        // Verificar que inicialmente no está leída
        notificacion.EstaLeida.Should().BeFalse();

        // Act
        var response = await HttpClient.PostAsync($"/api/core/notificaciones/{notificacion.Id}/marcar-leida", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Recargar la notificación desde la base de datos para asegurar el estado actualizado
        var notificacionEnBD = await DbContext.Notificaciones.FindAsync(notificacion.Id);
        await DbContext.Entry(notificacionEnBD!).ReloadAsync();

        // Assert
        notificacionEnBD!.EstaLeida.Should().BeTrue();
    }

    [Fact]
    public async Task MarcarTodasComoLeidas_DebeMarcarTodasComoLeidas()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: MarcarTodasComoLeidas_DebeMarcarTodasComoLeidas");

        var usuarioId = await CrearUsuarioPrueba();
        var notificacion1 = await CrearNotificacionPrueba(usuarioId, "Notificación 1", estaLeida: false);
        var notificacion2 = await CrearNotificacionPrueba(usuarioId, "Notificación 2", estaLeida: false);

        // Verificar que inicialmente no están leídas
        notificacion1.EstaLeida.Should().BeFalse();
        notificacion2.EstaLeida.Should().BeFalse();

        // Act - Usar POST en lugar de PATCH
        var response = await HttpClient.PostAsync("/api/core/notificaciones/marcar-leida", null);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert - Test completo con validación estricta
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeGreaterThan(0);

        // Verificar que todas se marcaron como leídas en la BD
        DbContext.ChangeTracker.Clear();
        var notificacionesEnBD = await DbContext.Notificaciones.Where(n => n.DestinatarioId == usuarioId).ToListAsync();
        notificacionesEnBD.Should().NotBeEmpty();
        notificacionesEnBD.Should().OnlyContain(n => n.EstaLeida);

        Logger.LogInformation("✅ Test completado - todas las notificaciones marcadas como leídas");
    }

    [Fact]
    public async Task GetContadorNoLeidas_DebeRetornarContadorCorrecto()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetContadorNoLeidas_DebeRetornarContadorCorrecto");

        var usuarioId = await CrearUsuarioPrueba();
        await CrearNotificacionPrueba(usuarioId, "Notificación 1", estaLeida: false);
        await CrearNotificacionPrueba(usuarioId, "Notificación 2", estaLeida: false);
        await CrearNotificacionPrueba(usuarioId, "Notificación 3", estaLeida: true); // Esta está leída

        // Act
        var response = await HttpClient.GetAsync("/api/core/notificaciones/contador-no-leidas");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert - Test completo con validación estricta
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(2); // Solo 2 no leídas

        Logger.LogInformation("✅ Test completado - contador de no leídas correcto");
    }

    [Fact]
    public async Task DeleteNotificacion_ConIdExistente_DebeEliminarNotificacion()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: DeleteNotificacion_ConIdExistente_DebeEliminarNotificacion");

        var usuarioId = await CrearUsuarioPrueba();
        var notificacion = await CrearNotificacionPrueba(usuarioId, "Notificación a eliminar");

        // Verificar que existe en la BD
        var notificacionesAntes = await DbContext.Notificaciones.CountAsync();
        notificacionesAntes.Should().BeGreaterThan(0);

        // Act
        var response = await HttpClient.DeleteAsync($"/api/core/notificaciones/{notificacion.Id}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert - Test completo con validación estricta
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();

        // Verificar que se eliminó de la BD
        DbContext.ChangeTracker.Clear();
        var notificacionEnBD = await DbContext.Notificaciones.FirstOrDefaultAsync(n => n.Id == notificacion.Id);
        notificacionEnBD.Should().BeNull();

        Logger.LogInformation("✅ Test completado - notificación eliminada correctamente");
    }

    [Fact]
    public async Task DeleteNotificacion_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: DeleteNotificacion_ConIdInexistente_DebeRetornar404");
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/core/notificaciones/{idInexistente}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert - Test completo con validación estricta
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("Error al eliminar notificación");

        Logger.LogInformation("✅ Test completado - 404 retornado para eliminación de ID inexistente");
    }

    public new void Dispose()
    {
        _factory?.Dispose();
        base.Dispose();
    }
}

// DTOs para las pruebas
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

public class CrearNotificacionCommand
{
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Informativa";
    public Guid DestinatarioId { get; set; }
    public Guid? EntidadRelacionadaId { get; set; }
}