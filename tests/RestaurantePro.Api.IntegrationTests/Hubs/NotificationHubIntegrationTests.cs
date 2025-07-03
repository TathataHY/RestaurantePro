using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using RestaurantePro.Api.IntegrationTests.TestBase;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.SignalR; // Para HubException

namespace RestaurantePro.Api.IntegrationTests.Hubs;

[Collection("ApiTestCollection")]
public class NotificationHubIntegrationTests : ApiIntegrationTestBase
{
    public NotificationHubIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task DeberiaConectarAlNotificationHubConAutenticacion()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        // Act
        await connection.StartAsync();

        // Assert
        Assert.Equal(HubConnectionState.Connected, connection.State);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaResponderPingPongEnNotificationHub()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act
        var pongReceived = false;
        DateTime? pongTime = null;
        
        connection.On<DateTime>("Pong", (time) =>
        {
            pongReceived = true;
            pongTime = time;
        });

        await connection.InvokeAsync("Ping");

        // Assert
        await Task.Delay(1000); // Esperar a que llegue la respuesta
        Assert.True(pongReceived);
        Assert.NotNull(pongTime);
        Assert.True(pongTime.Value > DateTime.UtcNow.AddSeconds(-5));
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaUnirseAGrupoCorrectamenteEnNotificationHub()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act
        await connection.InvokeAsync("UnirseAGrupo", "TestGroup");

        // Assert - No debería lanzar excepción
        Assert.Equal(HubConnectionState.Connected, connection.State);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaRechazarConexionSinAutenticacionEnNotificationHub()
    {
        // Arrange
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => connection.StartAsync());
        Assert.Contains("401", exception.Message);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaEnviarNotificacionGlobalCorrectamente()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act
        var notificacionRecibida = false;
        object? notificacionData = null;
        
        connection.On<object>("RecibirNotificacion", (data) =>
        {
            notificacionRecibida = true;
            notificacionData = data;
        });

        await connection.InvokeAsync("EnviarNotificacionGlobal", "Test Global", "Mensaje de prueba", "info");

        // Assert
        await Task.Delay(1000);
        Assert.True(notificacionRecibida);
        Assert.NotNull(notificacionData);
        
        var jsonString = JsonSerializer.Serialize(notificacionData);
        var notificacion = JsonSerializer.Deserialize<JsonElement>(jsonString);
        
        // Debug: Imprimir todas las propiedades disponibles
        Console.WriteLine($"JSON recibido: {jsonString}");
        foreach (var property in notificacion.EnumerateObject())
        {
            Console.WriteLine($"Propiedad: {property.Name} = {property.Value}");
        }
        
        Assert.Equal("Test Global", notificacion.GetProperty("titulo").GetString());
        Assert.Equal("Mensaje de prueba", notificacion.GetProperty("mensaje").GetString());
        Assert.Equal("info", notificacion.GetProperty("tipo").GetString());
        // Verificar que tiene las propiedades adicionales del hub
        Assert.True(notificacion.TryGetProperty("fechaHora", out _));
        Assert.True(notificacion.TryGetProperty("enviadoPor", out _));
        Assert.True(notificacion.TryGetProperty("rolEnviadoPor", out _));
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaEnviarNotificacionARolCorrectamente()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act - Primero unirse al grupo del rol
        await connection.InvokeAsync("UnirseAGrupo", "Administrador");
        
        var notificacionRecibida = false;
        object? notificacionData = null;
        
        connection.On<object>("RecibirNotificacion", (data) =>
        {
            notificacionRecibida = true;
            notificacionData = data;
        });

        await connection.InvokeAsync("EnviarNotificacionARol", "Administrador", "Test Rol", "Mensaje para administradores", "warning");

        // Assert
        await Task.Delay(1000);
        Assert.True(notificacionRecibida);
        Assert.NotNull(notificacionData);
        
        var jsonString = JsonSerializer.Serialize(notificacionData);
        var notificacion = JsonSerializer.Deserialize<JsonElement>(jsonString);
        
        Assert.Equal("Test Rol", notificacion.GetProperty("titulo").GetString());
        Assert.Equal("Mensaje para administradores", notificacion.GetProperty("mensaje").GetString());
        Assert.Equal("warning", notificacion.GetProperty("tipo").GetString());
        Assert.Equal("Administrador", notificacion.GetProperty("rolDestinatario").GetString());
        // Verificar que tiene las propiedades adicionales del hub
        Assert.True(notificacion.TryGetProperty("fechaHora", out _));
        Assert.True(notificacion.TryGetProperty("enviadoPor", out _));
        Assert.True(notificacion.TryGetProperty("rolEnviadoPor", out _));
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaEnviarNotificacionAUsuarioCorrectamente()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        var usuarioDestino = Guid.NewGuid();
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act
        await connection.InvokeAsync("EnviarNotificacionAUsuario", usuarioDestino, "Test Usuario", "Mensaje personalizado", "success");

        // Assert - No debería lanzar excepción
        Assert.Equal(HubConnectionState.Connected, connection.State);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaGestionarGruposCorrectamenteEnNotificationHub()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert
        await connection.InvokeAsync("UnirseAGrupo", "GrupoTest1");
        await connection.InvokeAsync("UnirseAGrupo", "GrupoTest2");
        await connection.InvokeAsync("SalirDeGrupo", "GrupoTest1");
        
        // No debería lanzar excepción
        Assert.Equal(HubConnectionState.Connected, connection.State);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaRechazarMensajeAdminSinAutorizacion()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion(); // Token de usuario normal
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HubException>(() => 
            connection.InvokeAsync("EnviarMensajeAdmin", "Test Admin", "Mensaje administrativo"));
        
        Assert.Contains("Failed to invoke", exception.Message);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaRechazarAlertaSistemaSinAutorizacion()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion(); // Token de usuario normal
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HubException>(() => 
            connection.InvokeAsync("EnviarAlertaSistema", "Test Sistema", "Alerta del sistema"));
        
        Assert.Contains("Failed to invoke", exception.Message);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaRecibirNotificacionGlobalDeOtroUsuario()
    {
        // Arrange
        var token1 = await ObtenerTokenAutenticacion();
        var token2 = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        // Cliente 1: Enviará la notificación
        var connection1 = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token1);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        // Cliente 2: Recibirá la notificación
        var connection2 = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token2);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection1.StartAsync();
        await connection2.StartAsync();

        // Act
        var notificacionRecibida = false;
        object? notificacionData = null;
        
        connection2.On<object>("RecibirNotificacion", (data) =>
        {
            notificacionRecibida = true;
            notificacionData = data;
        });

        await connection1.InvokeAsync("EnviarNotificacionGlobal", "Test Multi-Usuario", "Mensaje para todos", "info");

        // Assert
        await Task.Delay(1000);
        Assert.True(notificacionRecibida);
        Assert.NotNull(notificacionData);
        
        var jsonString = JsonSerializer.Serialize(notificacionData);
        var notificacion = JsonSerializer.Deserialize<JsonElement>(jsonString);
        
        Assert.Equal("Test Multi-Usuario", notificacion.GetProperty("titulo").GetString());
        Assert.Equal("Mensaje para todos", notificacion.GetProperty("mensaje").GetString());
        Assert.Equal("info", notificacion.GetProperty("tipo").GetString());
        // Verificar que tiene las propiedades adicionales del hub
        Assert.True(notificacion.TryGetProperty("fechaHora", out _));
        Assert.True(notificacion.TryGetProperty("enviadoPor", out _));
        Assert.True(notificacion.TryGetProperty("rolEnviadoPor", out _));
        
        await connection1.DisposeAsync();
        await connection2.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaManejarErroresCorrectamenteEnNotificationHub()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert - Método que no existe
        var exception = await Assert.ThrowsAsync<HubException>(() => 
            connection.InvokeAsync("MetodoInexistente"));
        
        Assert.Contains("Method", exception.Message);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaValidarParametrosDeNotificacion()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert - Parámetros inválidos
        var exception = await Assert.ThrowsAsync<HubException>(() => 
            connection.InvokeAsync("EnviarNotificacionGlobal", "", "Mensaje", "info"));
        
        // Debería manejar el error de parámetros vacíos
        Assert.NotNull(exception);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaConectarMultiplesClientesSimultaneamente()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connections = new List<HubConnection>();
        
        // Act - Crear múltiples conexiones
        for (int i = 0; i < 5; i++)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                    options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                })
                .Build();
            
            await connection.StartAsync();
            connections.Add(connection);
        }

        // Assert
        foreach (var connection in connections)
        {
            Assert.Equal(HubConnectionState.Connected, connection.State);
        }
        
        // Cleanup
        foreach (var connection in connections)
        {
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task DeberiaMantenerConexionEstableEnNotificationHub()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/notifications");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act - Mantener conexión activa con pings
        for (int i = 0; i < 3; i++)
        {
            await connection.InvokeAsync("Ping");
            await Task.Delay(500);
        }

        // Assert
        Assert.Equal(HubConnectionState.Connected, connection.State);
        
        await connection.DisposeAsync();
    }

    /// <summary>
    /// Obtiene un token de autenticación para los tests
    /// </summary>
    private async Task<string> ObtenerTokenAutenticacion()
    {
        // Para los tests de SignalR, usar directamente el token del FakeJwtTokenService
        // que ya está configurado en el TestWebApplicationFactory
        return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbkB0ZXN0LmNvbSIsIm5hbWUiOiJBZG1pbiIsInJvbGUiOiJBZG1pbmlzdHJhZG9yIiwibmJmIjoxNzM1NzI4MDAwLCJleHAiOjE3MzU3MzE2MDAsImlhdCI6MTczNTcyODAwMH0.fake-signature";
    }
} 