using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using RestaurantePro.Api.IntegrationTests.TestBase;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.SignalR; // Para HubException

namespace RestaurantePro.Api.IntegrationTests.Hubs;

[Collection("ApiTestCollection")]
public class InventarioHubIntegrationTests : ApiIntegrationTestBase
{
    public InventarioHubIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task DeberiaConectarAlInventarioHubConAutenticacion()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
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
    public async Task DeberiaResponderPingPongEnInventarioHub()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
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
    public async Task DeberiaUnirseAGrupoCorrectamenteEnInventarioHub()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
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
    public async Task DeberiaRechazarConexionSinAutenticacionEnInventarioHub()
    {
        // Arrange
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
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
    public async Task DeberiaEnviarAlertaStockBajoCorrectamente()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act - Primero unirse al grupo de administradores
        await connection.InvokeAsync("UnirseAGrupo", "Administrador");
        
        var alertaRecibida = false;
        object? alertaData = null;
        
        connection.On<object>("RecibirAlertaStock", (data) =>
        {
            alertaRecibida = true;
            alertaData = data;
        });

        var ingredienteId = Guid.NewGuid();
        await connection.InvokeAsync("AlertaStockBajo", ingredienteId, "Tomates", 5.0m, 10.0m, "kg");

        // Assert
        await Task.Delay(1000);
        Assert.True(alertaRecibida);
        Assert.NotNull(alertaData);
        
        var jsonString = JsonSerializer.Serialize(alertaData);
        var alerta = JsonSerializer.Deserialize<JsonElement>(jsonString);
        
        Assert.Equal(ingredienteId.ToString(), alerta.GetProperty("ingredienteId").GetString());
        Assert.Equal("Tomates", alerta.GetProperty("nombreIngrediente").GetString());
        Assert.Equal(5.0m, alerta.GetProperty("stockActual").GetDecimal());
        Assert.Equal(10.0m, alerta.GetProperty("stockMinimo").GetDecimal());
        Assert.Equal("kg", alerta.GetProperty("unidadMedida").GetString());
        Assert.Equal("StockBajo", alerta.GetProperty("tipoAlerta").GetString());
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaEnviarAlertaStockAgotadoCorrectamente()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act - Primero unirse al grupo de administradores
        await connection.InvokeAsync("UnirseAGrupo", "Administrador");
        
        var alertaRecibida = false;
        object? alertaData = null;
        
        connection.On<object>("RecibirAlertaStock", (data) =>
        {
            alertaRecibida = true;
            alertaData = data;
        });

        var ingredienteId = Guid.NewGuid();
        await connection.InvokeAsync("AlertaStockAgotado", ingredienteId, "Lechuga", "unidades");

        // Assert
        await Task.Delay(1000);
        Assert.True(alertaRecibida);
        Assert.NotNull(alertaData);
        
        var jsonString = JsonSerializer.Serialize(alertaData);
        var alerta = JsonSerializer.Deserialize<JsonElement>(jsonString);
        
        Assert.Equal(ingredienteId.ToString(), alerta.GetProperty("ingredienteId").GetString());
        Assert.Equal("Lechuga", alerta.GetProperty("nombreIngrediente").GetString());
        Assert.Equal("unidades", alerta.GetProperty("unidadMedida").GetString());
        Assert.Equal("StockAgotado", alerta.GetProperty("tipoAlerta").GetString());
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaNotificarRecepcionMercanciaCorrectamente()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act - Primero unirse al grupo de administradores
        await connection.InvokeAsync("UnirseAGrupo", "Administrador");
        
        var recepcionRecibida = false;
        object? recepcionData = null;
        
        connection.On<object>("RecibirActualizacionInventario", (data) =>
        {
            recepcionRecibida = true;
            recepcionData = data;
        });

        var ordenCompraId = Guid.NewGuid();
        var items = new List<object>
        {
            new { IngredienteId = Guid.NewGuid(), Nombre = "Tomates", Cantidad = 50, Unidad = "kg" },
            new { IngredienteId = Guid.NewGuid(), Nombre = "Lechuga", Cantidad = 100, Unidad = "unidades" }
        };
        
        await connection.InvokeAsync("RecepcionMercancia", ordenCompraId, "OC-2024-001", items, DateTime.UtcNow);

        // Assert
        await Task.Delay(1000);
        Assert.True(recepcionRecibida);
        Assert.NotNull(recepcionData);
        
        var jsonString = JsonSerializer.Serialize(recepcionData);
        var recepcion = JsonSerializer.Deserialize<JsonElement>(jsonString);
        
        Assert.Equal(ordenCompraId.ToString(), recepcion.GetProperty("ordenCompraId").GetString());
        Assert.Equal("OC-2024-001", recepcion.GetProperty("numeroOrden").GetString());
        Assert.Equal(2, recepcion.GetProperty("cantidadItems").GetInt32());
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaEnviarAlertaVencimientoCorrectamente()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act - Primero unirse al grupo de administradores
        await connection.InvokeAsync("UnirseAGrupo", "Administrador");
        
        var alertaRecibida = false;
        object? alertaData = null;
        
        connection.On<object>("RecibirAlertaVencimiento", (data) =>
        {
            alertaRecibida = true;
            alertaData = data;
        });

        var ingredienteId = Guid.NewGuid();
        var fechaVencimiento = DateTime.UtcNow.AddDays(3);
        await connection.InvokeAsync("ProductoPorVencer", ingredienteId, "Yogurt", fechaVencimiento, 3);

        // Assert
        await Task.Delay(1000);
        Assert.True(alertaRecibida);
        Assert.NotNull(alertaData);
        
        var jsonString = JsonSerializer.Serialize(alertaData);
        var alerta = JsonSerializer.Deserialize<JsonElement>(jsonString);
        
        Assert.Equal(ingredienteId.ToString(), alerta.GetProperty("ingredienteId").GetString());
        Assert.Equal("Yogurt", alerta.GetProperty("nombreIngrediente").GetString());
        Assert.Equal(3, alerta.GetProperty("diasRestantes").GetInt32());
        Assert.Equal("PorVencer", alerta.GetProperty("tipoAlerta").GetString());
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaGestionarGruposCorrectamenteEnInventarioHub()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert - Unirse a grupo
        await connection.InvokeAsync("UnirseAGrupo", "TestGroup");
        Assert.Equal(HubConnectionState.Connected, connection.State);
        
        // Salir del grupo
        await connection.InvokeAsync("SalirDeGrupo", "TestGroup");
        Assert.Equal(HubConnectionState.Connected, connection.State);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaValidarParametrosDeAlertaStock()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert - Probar con parámetros inválidos
        var exception = await Assert.ThrowsAsync<HubException>(() => 
            connection.InvokeAsync("AlertaStockBajo", Guid.Empty, "", 0, 0, ""));
        
        Assert.Contains("no pueden estar vacíos", exception.Message);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaValidarParametrosDeRecepcionMercancia()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert - Probar con parámetros inválidos
        var exception = await Assert.ThrowsAsync<HubException>(() => 
            connection.InvokeAsync("RecepcionMercancia", Guid.Empty, "", null, DateTime.UtcNow));
        
        Assert.Contains("no pueden estar vacíos", exception.Message);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaRecibirAlertaStockDeOtroUsuario()
    {
        // Arrange - Dos conexiones diferentes
        var token1 = await ObtenerTokenAutenticacion();
        var token2 = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connection1 = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token1);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        var connection2 = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token2);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection1.StartAsync();
        await connection2.StartAsync();

        // Act - Unirse ambos al grupo de administradores
        await connection1.InvokeAsync("UnirseAGrupo", "Administrador");
        await connection2.InvokeAsync("UnirseAGrupo", "Administrador");
        
        var alertaRecibida = false;
        object? alertaData = null;
        
        connection2.On<object>("RecibirAlertaStock", (data) =>
        {
            alertaRecibida = true;
            alertaData = data;
        });

        // Enviar alerta desde connection1
        var ingredienteId = Guid.NewGuid();
        await connection1.InvokeAsync("AlertaStockBajo", ingredienteId, "Cebollas", 2.0m, 5.0m, "kg");

        // Assert
        await Task.Delay(1000);
        Assert.True(alertaRecibida);
        Assert.NotNull(alertaData);
        
        var jsonString = JsonSerializer.Serialize(alertaData);
        var alerta = JsonSerializer.Deserialize<JsonElement>(jsonString);
        
        Assert.Equal(ingredienteId.ToString(), alerta.GetProperty("ingredienteId").GetString());
        Assert.Equal("Cebollas", alerta.GetProperty("nombreIngrediente").GetString());
        
        await connection1.DisposeAsync();
        await connection2.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaManejarErroresCorrectamenteEnInventarioHub()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act & Assert - Probar con parámetros inválidos
        var exception = await Assert.ThrowsAsync<HubException>(() => 
            connection.InvokeAsync("UnirseAGrupo", ""));
        
        Assert.Contains("no puede estar vacío", exception.Message);
        
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task DeberiaConectarMultiplesClientesSimultaneamente()
    {
        // Arrange - Múltiples conexiones
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connections = new List<HubConnection>();
        
        // Crear 5 conexiones simultáneas
        for (int i = 0; i < 5; i++)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                    options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                })
                .Build();
            
            connections.Add(connection);
        }

        // Act - Conectar todas las conexiones
        foreach (var connection in connections)
        {
            await connection.StartAsync();
        }

        // Assert - Verificar que todas están conectadas
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
    public async Task DeberiaMantenerConexionEstableEnInventarioHub()
    {
        // Arrange
        var token = await ObtenerTokenAutenticacion();
        var hubUrl = new Uri(HttpClient.BaseAddress, "hubs/inventario");
        
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        // Act - Realizar múltiples operaciones para verificar estabilidad
        await connection.InvokeAsync("UnirseAGrupo", "TestGroup");
        await connection.InvokeAsync("Ping");
        await Task.Delay(500);
        await connection.InvokeAsync("SalirDeGrupo", "TestGroup");
        await connection.InvokeAsync("Ping");
        await Task.Delay(500);

        // Assert - La conexión debe mantenerse estable
        Assert.Equal(HubConnectionState.Connected, connection.State);
        
        await connection.DisposeAsync();
    }

    private async Task<string> ObtenerTokenAutenticacion()
    {
        // Crear un token JWT simulado para tests
        var claims = new Dictionary<string, object>
        {
            { "sub", "admin@test.com" },
            { "name", "Admin" },
            { "role", "Administrador" },
            { "nbf", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        var header = new { alg = "HS256", typ = "JWT" };
        var payload = claims;

        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);

        var headerBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(headerJson))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        var payloadBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var signature = "fake-signature";
        var signatureBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(signature))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        return $"{headerBase64}.{payloadBase64}.{signatureBase64}";
    }
} 