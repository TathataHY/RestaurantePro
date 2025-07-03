using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Api.Hubs;
using RestaurantePro.Application.Common.Notifications;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RestaurantePro.Api.IntegrationTests.Hubs;

/// <summary>
/// Tests de integración para el ComandaHub
/// Valida la funcionalidad básica de comunicación en tiempo real
/// </summary>
public class ComandaHubIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public ComandaHubIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task DeberiaConectarAlHubConAutenticacion()
    {
        // Arrange
        Console.WriteLine($"DEBUG: BaseAddress del HttpClient: {_httpClient.BaseAddress}");
        var token = await ObtenerTokenAutenticacion();
        
        // Para SignalR en tests, usar el HttpClient del TestServer
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        Console.WriteLine($"DEBUG: Hub URL construida: {hubUrl}");
        
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        // Act
        await hubConnection.StartAsync();

        // Assert
        Assert.Equal(HubConnectionState.Connected, hubConnection.State);

        // Cleanup
        await hubConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaResponderPingPong()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        
        // Para SignalR en tests, usar el HttpClient del TestServer
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        Console.WriteLine($"DEBUG: Hub URL construida: {hubUrl}");
        
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await hubConnection.StartAsync();

        DateTime? pongReceived = null;
        hubConnection.On<DateTime>("Pong", (timestamp) => pongReceived = timestamp);

        // Act
        await hubConnection.InvokeAsync("Ping");

        // Assert
        await Task.Delay(1000); // Esperar respuesta
        Assert.NotNull(pongReceived);
        Assert.True(pongReceived.Value > DateTime.UtcNow.AddSeconds(-5));

        // Cleanup
        await hubConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaUnirseAGrupoCorrectamente()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        
        // Para SignalR en tests, usar el HttpClient del TestServer
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        Console.WriteLine($"DEBUG: Hub URL construida: {hubUrl}");
        
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await hubConnection.StartAsync();

        string? grupoJoined = null;
        hubConnection.On<string>("GrupoJoined", (groupName) => grupoJoined = groupName);

        // Act
        await hubConnection.InvokeAsync("JoinGroup", "TestGroup");

        // Assert
        await Task.Delay(1000); // Esperar respuesta
        Assert.Equal("TestGroup", grupoJoined);

        // Cleanup
        await hubConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaRechazarConexionSinAutenticacion()
    {
        // Arrange
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        Console.WriteLine($"DEBUG: Hub URL construida: {hubUrl}");
        
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => hubConnection.StartAsync());
        
        // Cleanup
        await hubConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaRecibirNotificacionDeNuevaComanda()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        
        // Para SignalR en tests, usar el HttpClient del TestServer
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        Console.WriteLine($"DEBUG: Hub URL construida: {hubUrl}");
        
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        NuevaComandaNotificationDto? comandaRecibida = null;
        hubConnection.On<NuevaComandaNotificationDto>("NuevaComanda", (comanda) => 
        {
            comandaRecibida = comanda;
        });

        await hubConnection.StartAsync();

        // Act - Simular unirse al grupo de cocina
        await hubConnection.InvokeAsync("JoinGroup", "Cocina");

        // Assert - Verificar que la conexión está establecida
        Assert.Equal(HubConnectionState.Connected, hubConnection.State);

        // Cleanup
        await hubConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaEnviarNuevaComandaACocina()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        
        // Crear dos conexiones: una para mesero y otra para cocina
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        
        // Cliente 1: Simula ser mesero
        var meseroConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        // Cliente 2: Simula ser cocina
        var cocinaConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        // Variables para capturar eventos recibidos
        ComandaSummaryDto? comandaRecibida = null;
        var comandaRecibidaEvent = new TaskCompletionSource<bool>();

        // Configurar listener en cocina para recibir nueva comanda
        cocinaConnection.On<ComandaSummaryDto>("RecibirNuevaComanda", (comanda) =>
        {
            comandaRecibida = comanda;
            comandaRecibidaEvent.SetResult(true);
        });

        // Conectar ambos clientes
        await meseroConnection.StartAsync();
        await cocinaConnection.StartAsync();

        // Unir cocina al grupo correspondiente
        await cocinaConnection.InvokeAsync("JoinGroup", "Cocina");

        // Crear comanda de prueba
        var nuevaComanda = new ComandaSummaryDto
        {
            Id = Guid.NewGuid(),
            NumeroComanda = "CMD-001",
            MesaId = Guid.NewGuid(),
            NumeroMesa = "Mesa 5",
            Estado = "Pendiente",
            FechaCreacion = DateTime.UtcNow,
            TotalItems = 3,
            CantidadTotal = 5,
            Subtotal = 45.50m,
            Total = 52.33m,
            Observaciones = "Sin cebolla en la hamburguesa",
            NombreMesero = "Juan Pérez",
            Prioridad = "Normal",
            TipoServicio = "Mesa"
        };

        // Act: Mesero envía nueva comanda
        await meseroConnection.InvokeAsync("NuevaComanda", nuevaComanda);

        // Assert: Cocina debe recibir la comanda
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
        var completedTask = await Task.WhenAny(comandaRecibidaEvent.Task, timeoutTask);

        Assert.True(completedTask == comandaRecibidaEvent.Task, "La cocina no recibió la notificación de nueva comanda en el tiempo esperado");
        Assert.NotNull(comandaRecibida);
        Assert.Equal(nuevaComanda.Id, comandaRecibida.Id);
        Assert.Equal(nuevaComanda.NumeroComanda, comandaRecibida.NumeroComanda);
        Assert.Equal(nuevaComanda.MesaId, comandaRecibida.MesaId);
        Assert.Equal(nuevaComanda.NumeroMesa, comandaRecibida.NumeroMesa);
        Assert.Equal(nuevaComanda.Estado, comandaRecibida.Estado);
        Assert.Equal(nuevaComanda.Observaciones, comandaRecibida.Observaciones);
        Assert.Equal(nuevaComanda.NombreMesero, comandaRecibida.NombreMesero);

        // Cleanup
        await meseroConnection.StopAsync();
        await cocinaConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaActualizarEstadoComandaEnTiempoReal()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        
        // Cliente 1: Simula ser cocina
        var cocinaConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        // Cliente 2: Simula ser mesero
        var meseroConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        // Variables para capturar eventos
        object? actualizacionRecibida = null;
        var actualizacionEvent = new TaskCompletionSource<bool>();

        // Configurar listeners para recibir actualización de estado
        cocinaConnection.On<object>("ComandaActualizada", (actualizacion) =>
        {
            actualizacionRecibida = actualizacion;
            actualizacionEvent.SetResult(true);
        });

        meseroConnection.On<object>("ComandaActualizada", (actualizacion) =>
        {
            actualizacionRecibida = actualizacion;
            actualizacionEvent.SetResult(true);
        });

        // Conectar clientes
        await cocinaConnection.StartAsync();
        await meseroConnection.StartAsync();

        // Unir a grupos correspondientes
        await cocinaConnection.InvokeAsync("JoinGroup", "Cocina");
        await meseroConnection.InvokeAsync("JoinGroup", "Meseros");

        var comandaId = Guid.NewGuid();
        var nuevoEstado = "En Preparación";
        var detalles = new { TiempoEstimado = 15, Prioridad = "Alta" };

        // Act: Cocina actualiza estado de comanda
        await cocinaConnection.InvokeAsync("ActualizarEstadoComanda", comandaId, nuevoEstado, detalles);

        // Assert: Ambos deben recibir la actualización
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
        var completedTask = await Task.WhenAny(actualizacionEvent.Task, timeoutTask);

        Assert.True(completedTask == actualizacionEvent.Task, "No se recibió la actualización de estado en el tiempo esperado");
        Assert.NotNull(actualizacionRecibida);

        // Cleanup
        await cocinaConnection.StopAsync();
        await meseroConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaGestionarGruposCorrectamente()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await hubConnection.StartAsync();

        string? grupoJoined = null;
        string? grupoLeft = null;
        hubConnection.On<string>("GrupoJoined", (groupName) => grupoJoined = groupName);
        hubConnection.On<string>("GrupoLeft", (groupName) => grupoLeft = groupName);

        // Act - Unirse al grupo Cocina
        await hubConnection.InvokeAsync("JoinGroup", "Cocina");
        await Task.Delay(500);

        // Assert - Verificar que se unió al grupo
        Assert.Equal("Cocina", grupoJoined);

        // Act - Salir del grupo Cocina
        await hubConnection.InvokeAsync("LeaveGroup", "Cocina");
        await Task.Delay(500);

        // Assert - Verificar que salió del grupo
        Assert.Equal("Cocina", grupoLeft);

        // Cleanup
        await hubConnection.StopAsync();
    }



    [Fact]
    public async Task DeberiaCancelarComandaYNotificarATodos()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        
        // Cliente 1: Simula ser mesero
        var meseroConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        // Cliente 2: Simula ser cocina
        var cocinaConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await meseroConnection.StartAsync();
        await cocinaConnection.StartAsync();

        // Unirse a grupos correspondientes
        await meseroConnection.InvokeAsync("JoinGroup", "Meseros");
        await cocinaConnection.InvokeAsync("JoinGroup", "Cocina");

        // Variables para capturar eventos
        object? cancelacionRecibidaMesero = null;
        object? cancelacionRecibidaCocina = null;

        meseroConnection.On<object>("ComandaCancelada", (cancelacion) => cancelacionRecibidaMesero = cancelacion);
        cocinaConnection.On<object>("ComandaCancelada", (cancelacion) => cancelacionRecibidaCocina = cancelacion);

        var comandaId = Guid.NewGuid();
        var motivo = "Cliente canceló la orden";
        var canceladoPor = "admin@test.com";

        // Act
        await meseroConnection.InvokeAsync("CancelarComanda", comandaId, motivo, canceladoPor);

        // Assert
        await Task.Delay(1000);
        Assert.NotNull(cancelacionRecibidaMesero);
        Assert.NotNull(cancelacionRecibidaCocina);

        // Cleanup
        await meseroConnection.StopAsync();
        await cocinaConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaCambiarPrioridadYNotificarACocina()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        
        var cocinaConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await cocinaConnection.StartAsync();
        await cocinaConnection.InvokeAsync("JoinGroup", "Cocina");

        object? prioridadCambiada = null;
        cocinaConnection.On<object>("PrioridadCambiada", (cambio) => prioridadCambiada = cambio);

        var comandaId = Guid.NewGuid();
        var nuevaPrioridad = "Alta";
        var cambiadoPor = "admin@test.com";

        // Act
        await cocinaConnection.InvokeAsync("AsignarPrioridad", comandaId, nuevaPrioridad, cambiadoPor);

        // Assert
        await Task.Delay(1000);
        Assert.NotNull(prioridadCambiada);

        // Cleanup
        await cocinaConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaNotificarRetrasoAMeseros()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        
        var meseroConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await meseroConnection.StartAsync();
        await meseroConnection.InvokeAsync("JoinGroup", "Meseros");

        object? retrasoNotificado = null;
        meseroConnection.On<object>("RetrasoNotificado", (retraso) => retrasoNotificado = retraso);

        var comandaId = Guid.NewGuid();
        var motivo = "Falta de ingredientes";
        var minutosRetraso = 15;

        // Act
        await meseroConnection.InvokeAsync("NotificarRetraso", comandaId, motivo, minutosRetraso);

        // Assert
        await Task.Delay(1000);
        Assert.NotNull(retrasoNotificado);

        // Cleanup
        await meseroConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaSolicitarAyudaYNotificarASupervisor()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        
        var supervisorConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await supervisorConnection.StartAsync();
        await supervisorConnection.InvokeAsync("JoinGroup", "Administradores");

        object? ayudaSolicitada = null;
        supervisorConnection.On<object>("AyudaSolicitada", (ayuda) => ayudaSolicitada = ayuda);

        var comandaId = Guid.NewGuid();
        var tipoAyuda = "Problema con cliente";
        var solicitadoPor = "mesero@test.com";

        // Act
        await supervisorConnection.InvokeAsync("SolicitarAyuda", comandaId, tipoAyuda, solicitadoPor);

        // Assert
        await Task.Delay(1000);
        Assert.NotNull(ayudaSolicitada);

        // Cleanup
        await supervisorConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaConfirmarRecepcionDeComanda()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        
        var meseroConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await meseroConnection.StartAsync();
        await meseroConnection.InvokeAsync("JoinGroup", "Meseros");

        object? recepcionConfirmada = null;
        meseroConnection.On<object>("RecepcionConfirmada", (confirmacion) => recepcionConfirmada = confirmacion);

        var comandaId = Guid.NewGuid();
        var confirmadoPor = "cocinero@test.com";

        // Act
        await meseroConnection.InvokeAsync("ConfirmarRecepcion", comandaId, confirmadoPor);

        // Assert
        await Task.Delay(1000);
        Assert.NotNull(recepcionConfirmada);

        // Cleanup
        await meseroConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaActualizarTiempoEstimadoYNotificar()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        
        var meseroConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await meseroConnection.StartAsync();
        await meseroConnection.InvokeAsync("JoinGroup", "Meseros");

        object? tiempoActualizado = null;
        meseroConnection.On<object>("TiempoEstimadoActualizado", (tiempo) => tiempoActualizado = tiempo);

        var comandaId = Guid.NewGuid();
        var minutosEstimados = 25;
        var actualizadoPor = "cocinero@test.com";

        // Act
        await meseroConnection.InvokeAsync("ActualizarTiempoEstimado", comandaId, minutosEstimados, actualizadoPor);

        // Assert
        await Task.Delay(1000);
        Assert.NotNull(tiempoActualizado);

        // Cleanup
        await meseroConnection.StopAsync();
    }

    [Fact]
    public async Task DeberiaManejarConexionesMultiplesDelMismoUsuario()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(_httpClient.BaseAddress, "hubs/comandas");
        
        // Crear dos conexiones para el mismo usuario (simula app móvil + web)
        var conexion1 = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        var conexion2 = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        // Act
        await conexion1.StartAsync();
        await conexion2.StartAsync();

        // Assert - Ambas conexiones deberían estar activas
        Assert.Equal(HubConnectionState.Connected, conexion1.State);
        Assert.Equal(HubConnectionState.Connected, conexion2.State);

        // Cleanup
        await conexion1.StopAsync();
        await conexion2.StopAsync();
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