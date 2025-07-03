using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Services;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Services;

/// <summary>
/// Tests unitarios para HubConnectionManager
/// </summary>
public class HubConnectionManagerTests
{
    private readonly Mock<ILogger<HubConnectionManager>> _loggerMock;
    private readonly HubConnectionManager _hubConnectionManager;
    private readonly Guid _usuarioId1 = Guid.NewGuid();
    private readonly Guid _usuarioId2 = Guid.NewGuid();
    private readonly string _connectionId1 = "connection1";
    private readonly string _connectionId2 = "connection2";
    private readonly string _grupo1 = "Meseros";
    private readonly string _grupo2 = "Cocina";

    public HubConnectionManagerTests()
    {
        _loggerMock = new Mock<ILogger<HubConnectionManager>>();
        _hubConnectionManager = new HubConnectionManager(_loggerMock.Object);
    }

    [Fact]
    public async Task DeberiaAgregarConexionCorrectamente()
    {
        // Act
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1);

        // Assert
        var conexiones = await _hubConnectionManager.ObtenerConexionesUsuarioAsync(_usuarioId1);
        Assert.Single(conexiones);
        Assert.Contains(_connectionId1, conexiones);
        Assert.True(await _hubConnectionManager.UsuarioEstaConectadoAsync(_usuarioId1));
    }

    [Fact]
    public async Task DeberiaAgregarConexionConGrupoCorrectamente()
    {
        // Act
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1, _grupo1);

        // Assert
        var conexiones = await _hubConnectionManager.ObtenerConexionesUsuarioAsync(_usuarioId1);
        var grupos = await _hubConnectionManager.ObtenerGruposDeUsuarioAsync(_usuarioId1);
        var usuariosEnGrupo = await _hubConnectionManager.ObtenerUsuariosEnGrupoAsync(_grupo1);

        Assert.Single(conexiones);
        Assert.Contains(_connectionId1, conexiones);
        Assert.Single(grupos);
        Assert.Contains(_grupo1, grupos);
        Assert.Single(usuariosEnGrupo);
        Assert.Contains(_usuarioId1, usuariosEnGrupo);
    }

    [Fact]
    public async Task DeberiaRemoverConexionCorrectamente()
    {
        // Arrange
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1);

        // Act
        await _hubConnectionManager.RemoverConexionAsync(_connectionId1);

        // Assert
        var conexiones = await _hubConnectionManager.ObtenerConexionesUsuarioAsync(_usuarioId1);
        Assert.Empty(conexiones);
        Assert.False(await _hubConnectionManager.UsuarioEstaConectadoAsync(_usuarioId1));
    }

    [Fact]
    public async Task DeberiaGestionarMultiplesConexionesPorUsuario()
    {
        // Arrange
        var connectionId3 = "connection3";

        // Act
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1);
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId2);
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, connectionId3);

        // Assert
        var conexiones = await _hubConnectionManager.ObtenerConexionesUsuarioAsync(_usuarioId1);
        Assert.Equal(3, conexiones.Count);
        Assert.Contains(_connectionId1, conexiones);
        Assert.Contains(_connectionId2, conexiones);
        Assert.Contains(connectionId3, conexiones);
    }

    [Fact]
    public async Task DeberiaGestionarGruposCorrectamente()
    {
        // Arrange
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1, _grupo1);
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId2, _connectionId2, _grupo1);

        // Act
        var usuariosEnGrupo = await _hubConnectionManager.ObtenerUsuariosEnGrupoAsync(_grupo1);
        var conexionesGrupo = await _hubConnectionManager.ObtenerConexionesGrupoAsync(_grupo1);

        // Assert
        Assert.Equal(2, usuariosEnGrupo.Count);
        Assert.Contains(_usuarioId1, usuariosEnGrupo);
        Assert.Contains(_usuarioId2, usuariosEnGrupo);
        Assert.Equal(2, conexionesGrupo.Count);
        Assert.Contains(_connectionId1, conexionesGrupo);
        Assert.Contains(_connectionId2, conexionesGrupo);
    }

    [Fact]
    public async Task DeberiaAgregarUsuarioAGrupoCorrectamente()
    {
        // Arrange
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1);

        // Act
        await _hubConnectionManager.AgregarUsuarioAGrupoAsync(_usuarioId1, _grupo1);

        // Assert
        var grupos = await _hubConnectionManager.ObtenerGruposDeUsuarioAsync(_usuarioId1);
        var usuariosEnGrupo = await _hubConnectionManager.ObtenerUsuariosEnGrupoAsync(_grupo1);

        Assert.Single(grupos);
        Assert.Contains(_grupo1, grupos);
        Assert.Single(usuariosEnGrupo);
        Assert.Contains(_usuarioId1, usuariosEnGrupo);
    }

    [Fact]
    public async Task DeberiaRemoverUsuarioDeGrupoCorrectamente()
    {
        // Arrange
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1, _grupo1);

        // Act
        await _hubConnectionManager.RemoverUsuarioDeGrupoAsync(_usuarioId1, _grupo1);

        // Assert
        var grupos = await _hubConnectionManager.ObtenerGruposDeUsuarioAsync(_usuarioId1);
        var usuariosEnGrupo = await _hubConnectionManager.ObtenerUsuariosEnGrupoAsync(_grupo1);

        Assert.Empty(grupos);
        Assert.Empty(usuariosEnGrupo);
    }

    [Fact]
    public async Task DeberiaObtenerUsuarioPorConexionCorrectamente()
    {
        // Arrange
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1);

        // Act
        var usuarioId = await _hubConnectionManager.ObtenerUsuarioPorConexionAsync(_connectionId1);

        // Assert
        Assert.NotNull(usuarioId);
        Assert.Equal(_usuarioId1, usuarioId);
    }

    [Fact]
    public async Task DeberiaRetornarNullParaConexionInexistente()
    {
        // Act
        var usuarioId = await _hubConnectionManager.ObtenerUsuarioPorConexionAsync("conexion_inexistente");

        // Assert
        Assert.Null(usuarioId);
    }

    [Fact]
    public async Task DeberiaObtenerEstadisticasCorrectamente()
    {
        // Arrange
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1, _grupo1);
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId2, _connectionId2, _grupo1);

        // Act
        var estadisticas = await _hubConnectionManager.ObtenerEstadisticasConexionesAsync();

        // Assert
        Assert.Equal(2, estadisticas.TotalUsuariosConectados);
        Assert.Equal(2, estadisticas.TotalConexionesActivas);
        Assert.Equal(1, estadisticas.TotalGruposActivos);
        Assert.Single(estadisticas.UsuariosPorGrupo);
        Assert.Equal(2, estadisticas.UsuariosPorGrupo[_grupo1]);
        Assert.True(estadisticas.UltimaActualizacion > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task DeberiaLimpiarConexionesDesconectadasCorrectamente()
    {
        // Arrange
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1);
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId2, _connectionId2);

        // Simular que una conexión está "desconectada" (timestamp antiguo)
        // Esto se maneja internamente en el método de limpieza

        // Act
        await _hubConnectionManager.LimpiarConexionesDesconectadasAsync();

        // Assert - Las conexiones activas deberían mantenerse
        Assert.True(await _hubConnectionManager.UsuarioEstaConectadoAsync(_usuarioId1));
        Assert.True(await _hubConnectionManager.UsuarioEstaConectadoAsync(_usuarioId2));
    }

    [Fact]
    public async Task DeberiaManejarConcurrenciaCorrectamente()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act - Agregar múltiples conexiones simultáneamente
        for (int i = 0; i < 10; i++)
        {
            var usuarioId = Guid.NewGuid();
            var connectionId = $"connection_{i}";
            tasks.Add(_hubConnectionManager.AgregarConexionAsync(usuarioId, connectionId, _grupo1));
        }

        await Task.WhenAll(tasks);

        // Assert
        var estadisticas = await _hubConnectionManager.ObtenerEstadisticasConexionesAsync();
        Assert.Equal(10, estadisticas.TotalUsuariosConectados);
        Assert.Equal(10, estadisticas.TotalConexionesActivas);
        Assert.Equal(10, estadisticas.UsuariosPorGrupo[_grupo1]);
    }

    [Fact]
    public async Task DeberiaActualizarTimestampConexionCorrectamente()
    {
        // Arrange
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1);

        // Act
        await _hubConnectionManager.ActualizarTimestampConexionAsync(_connectionId1);

        // Assert
        var timestamp = await _hubConnectionManager.ObtenerTimestampConexionAsync(_connectionId1);
        Assert.NotNull(timestamp);
        Assert.True(timestamp > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task DeberiaRetornarNullParaTimestampDeConexionInexistente()
    {
        // Act
        var timestamp = await _hubConnectionManager.ObtenerTimestampConexionAsync("conexion_inexistente");

        // Assert
        Assert.Null(timestamp);
    }

    [Fact]
    public async Task DeberiaManejarUsuarioEnMultiplesGrupos()
    {
        // Arrange
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1, _grupo1);

        // Act
        await _hubConnectionManager.AgregarUsuarioAGrupoAsync(_usuarioId1, _grupo2);

        // Assert
        var grupos = await _hubConnectionManager.ObtenerGruposDeUsuarioAsync(_usuarioId1);
        Assert.Equal(2, grupos.Count);
        Assert.Contains(_grupo1, grupos);
        Assert.Contains(_grupo2, grupos);

        var usuariosGrupo1 = await _hubConnectionManager.ObtenerUsuariosEnGrupoAsync(_grupo1);
        var usuariosGrupo2 = await _hubConnectionManager.ObtenerUsuariosEnGrupoAsync(_grupo2);

        Assert.Single(usuariosGrupo1);
        Assert.Single(usuariosGrupo2);
        Assert.Contains(_usuarioId1, usuariosGrupo1);
        Assert.Contains(_usuarioId1, usuariosGrupo2);
    }

    [Fact]
    public async Task DeberiaRemoverUsuarioCompletamenteAlRemoverUltimaConexion()
    {
        // Arrange
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId1, _grupo1);
        await _hubConnectionManager.AgregarConexionAsync(_usuarioId1, _connectionId2, _grupo2);

        // Act
        await _hubConnectionManager.RemoverConexionAsync(_connectionId1);
        await _hubConnectionManager.RemoverConexionAsync(_connectionId2);

        // Assert
        Assert.False(await _hubConnectionManager.UsuarioEstaConectadoAsync(_usuarioId1));
        var grupos = await _hubConnectionManager.ObtenerGruposDeUsuarioAsync(_usuarioId1);
        Assert.Empty(grupos);
    }
} 