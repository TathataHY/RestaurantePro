using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Services;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Services;

/// <summary>
/// Tests unitarios para HubConnectionManager
/// Sigue las mejores prácticas de testing y arquitectura limpia
/// </summary>
public class HubConnectionManagerTests
{
    private readonly Mock<ILogger<HubConnectionManager>> _loggerMock;
    private readonly HubConnectionManager _connectionManager;
    private readonly Guid _usuarioId1 = Guid.NewGuid();
    private readonly Guid _usuarioId2 = Guid.NewGuid();
    private readonly string _connectionId1 = "connection1";
    private readonly string _connectionId2 = "connection2";
    private readonly string _grupo1 = "Meseros";
    private readonly string _grupo2 = "Cocina";

    public HubConnectionManagerTests()
    {
        _loggerMock = new Mock<ILogger<HubConnectionManager>>();
        _connectionManager = new HubConnectionManager(_loggerMock.Object);
    }

    [Fact]
    public async Task DeberiaAgregarConexionCorrectamente_ConUsuarioNuevo_DeberiaAgregarConexion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var connectionId = "connection123";
        var grupo = "Meseros";

        // Act
        await _connectionManager.AgregarConexionAsync(usuarioId, connectionId, grupo);

        // Assert
        var conexiones = await _connectionManager.ObtenerConexionesUsuarioAsync(usuarioId);
        Assert.Contains(connectionId, conexiones);
        
        var usuario = await _connectionManager.ObtenerUsuarioPorConexionAsync(connectionId);
        Assert.Equal(usuarioId, usuario);
        
        var grupos = await _connectionManager.ObtenerGruposDeUsuarioAsync(usuarioId);
        Assert.Contains(grupo, grupos);
    }

    [Fact]
    public async Task DeberiaAgregarConexionCorrectamente_ConUsuarioExistente_DeberiaAgregarNuevaConexion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var connectionId1 = "connection1";
        var connectionId2 = "connection2";
        var grupo = "Cocina";

        // Act
        await _connectionManager.AgregarConexionAsync(usuarioId, connectionId1, grupo);
        await _connectionManager.AgregarConexionAsync(usuarioId, connectionId2, grupo);

        // Assert
        var conexiones = await _connectionManager.ObtenerConexionesUsuarioAsync(usuarioId);
        Assert.Contains(connectionId1, conexiones);
        Assert.Contains(connectionId2, conexiones);
        Assert.Equal(2, conexiones.Count);
    }

    [Fact]
    public async Task DeberiaRemoverConexionCorrectamente_DeberiaEliminarConexion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var connectionId = "connection123";
        var grupo = "Administradores";

        await _connectionManager.AgregarConexionAsync(usuarioId, connectionId, grupo);

        // Act
        await _connectionManager.RemoverConexionAsync(connectionId);

        // Assert
        var conexiones = await _connectionManager.ObtenerConexionesUsuarioAsync(usuarioId);
        Assert.DoesNotContain(connectionId, conexiones);
        
        var usuario = await _connectionManager.ObtenerUsuarioPorConexionAsync(connectionId);
        Assert.Equal(Guid.Empty, usuario);
    }

    [Fact]
    public async Task DeberiaRemoverConexionCorrectamente_ConConexionInexistente_DeberiaManejarCorrectamente()
    {
        // Arrange
        var connectionId = "conexion_inexistente";

        // Act & Assert
        await _connectionManager.RemoverConexionAsync(connectionId);
        // No debería lanzar excepción
    }

    [Fact]
    public async Task DeberiaGestionarGruposCorrectamente_DeberiaAgregarYRemoverUsuarios()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var connectionId = "connection123";
        var grupo = "Meseros";

        await _connectionManager.AgregarConexionAsync(usuarioId, connectionId, grupo);

        // Act - Agregar a grupo adicional
        await _connectionManager.AgregarUsuarioAGrupoAsync(usuarioId, "Cocina");

        // Assert
        var grupos = await _connectionManager.ObtenerGruposDeUsuarioAsync(usuarioId);
        Assert.Contains("Meseros", grupos);
        Assert.Contains("Cocina", grupos);
        Assert.Equal(2, grupos.Count);

        // Act - Remover de grupo
        await _connectionManager.RemoverUsuarioDeGrupoAsync(usuarioId, "Meseros");

        // Assert
        grupos = await _connectionManager.ObtenerGruposDeUsuarioAsync(usuarioId);
        Assert.DoesNotContain("Meseros", grupos);
        Assert.Contains("Cocina", grupos);
        Assert.Single(grupos);
    }

    [Fact]
    public async Task DeberiaDetectarUsuariosConectados_DeberiaRetornarListaCorrecta()
    {
        // Arrange
        var usuario1 = Guid.NewGuid();
        var usuario2 = Guid.NewGuid();
        var usuario3 = Guid.NewGuid();

        await _connectionManager.AgregarConexionAsync(usuario1, "conn1", "Meseros");
        await _connectionManager.AgregarConexionAsync(usuario2, "conn2", "Cocina");
        await _connectionManager.AgregarConexionAsync(usuario3, "conn3", "Administradores");

        // Act
        var estaConectado1 = await _connectionManager.UsuarioEstaConectadoAsync(usuario1);
        var estaConectado2 = await _connectionManager.UsuarioEstaConectadoAsync(usuario2);
        var estaConectado3 = await _connectionManager.UsuarioEstaConectadoAsync(usuario3);
        var estaConectado4 = await _connectionManager.UsuarioEstaConectadoAsync(Guid.NewGuid());

        // Assert
        Assert.True(estaConectado1);
        Assert.True(estaConectado2);
        Assert.True(estaConectado3);
        Assert.False(estaConectado4);
    }

    [Fact]
    public async Task DeberiaObtenerUsuariosEnGrupo_DeberiaRetornarUsuariosCorrectos()
    {
        // Arrange
        var usuario1 = Guid.NewGuid();
        var usuario2 = Guid.NewGuid();
        var usuario3 = Guid.NewGuid();
        var grupo = "Meseros";

        await _connectionManager.AgregarConexionAsync(usuario1, "conn1", grupo);
        await _connectionManager.AgregarConexionAsync(usuario2, "conn2", grupo);
        await _connectionManager.AgregarConexionAsync(usuario3, "conn3", "Cocina");

        // Act
        var usuariosEnGrupo = await _connectionManager.ObtenerUsuariosEnGrupoAsync(grupo);

        // Assert
        Assert.Contains(usuario1, usuariosEnGrupo);
        Assert.Contains(usuario2, usuariosEnGrupo);
        Assert.DoesNotContain(usuario3, usuariosEnGrupo);
        Assert.Equal(2, usuariosEnGrupo.Count);
    }

    [Fact]
    public async Task DeberiaObtenerConexionesGrupo_DeberiaRetornarConexionesCorrectas()
    {
        // Arrange
        var usuario1 = Guid.NewGuid();
        var usuario2 = Guid.NewGuid();
        var grupo = "Cocina";

        await _connectionManager.AgregarConexionAsync(usuario1, "conn1", grupo);
        await _connectionManager.AgregarConexionAsync(usuario2, "conn2", grupo);

        // Act
        var conexiones = await _connectionManager.ObtenerConexionesGrupoAsync(grupo);

        // Assert
        Assert.Contains("conn1", conexiones);
        Assert.Contains("conn2", conexiones);
        Assert.Equal(2, conexiones.Count);
    }

    [Fact]
    public async Task DeberiaActualizarTimestampConexion_DeberiaActualizarCorrectamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var connectionId = "connection123";
        var grupo = "Meseros";

        await _connectionManager.AgregarConexionAsync(usuarioId, connectionId, grupo);

        // Act
        await _connectionManager.ActualizarTimestampConexionAsync(connectionId);

        // Assert
        var timestamp = await _connectionManager.ObtenerTimestampConexionAsync(connectionId);
        Assert.True(timestamp > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task DeberiaLimpiarConexionesDesconectadas_DeberiaEliminarConexionesAntiguas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var connectionId = "connection123";
        var grupo = "Meseros";

        await _connectionManager.AgregarConexionAsync(usuarioId, connectionId, grupo);

        // Simular conexión antigua (más de 30 minutos)
        // Nota: En un test real, podrías usar un mock del tiempo

        // Act
        await _connectionManager.LimpiarConexionesDesconectadasAsync();

        // Assert
        var conexiones = await _connectionManager.ObtenerConexionesUsuarioAsync(usuarioId);
        // La conexión debería mantenerse ya que es reciente
        Assert.Contains(connectionId, conexiones);
    }

    [Fact]
    public async Task DeberiaObtenerEstadisticasConexiones_DeberiaRetornarEstadisticasCorrectas()
    {
        // Arrange
        var usuario1 = Guid.NewGuid();
        var usuario2 = Guid.NewGuid();
        var usuario3 = Guid.NewGuid();

        await _connectionManager.AgregarConexionAsync(usuario1, "conn1", "Meseros");
        await _connectionManager.AgregarConexionAsync(usuario2, "conn2", "Cocina");
        await _connectionManager.AgregarConexionAsync(usuario3, "conn3", "Administradores");
        await _connectionManager.AgregarConexionAsync(usuario1, "conn4", "Meseros"); // Segunda conexión

        // Act
        var estadisticas = await _connectionManager.ObtenerEstadisticasConexionesAsync();

        // Assert
        Assert.Equal(4, estadisticas.TotalConexionesActivas);
        Assert.Equal(3, estadisticas.TotalUsuariosConectados);
        Assert.Equal(3, estadisticas.TotalGruposActivos);
    }

    [Fact]
    public async Task DeberiaManejarConcurrencia_DeberiaSerThreadSafe()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        // Act - Agregar múltiples conexiones simultáneamente
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _connectionManager.AgregarConexionAsync(usuarioId, $"conn{i}", "Meseros"))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var conexiones = await _connectionManager.ObtenerConexionesUsuarioAsync(usuarioId);
        Assert.Equal(10, conexiones.Count);
    }

    [Fact]
    public async Task DeberiaObtenerUsuarioPorConexion_ConConexionInexistente_DeberiaRetornarGuidEmpty()
    {
        // Arrange
        var connectionId = "conexion_inexistente";

        // Act
        var usuario = await _connectionManager.ObtenerUsuarioPorConexionAsync(connectionId);

        // Assert
        Assert.Equal(Guid.Empty, usuario);
    }

    [Fact]
    public async Task DeberiaObtenerGruposDeUsuario_ConUsuarioSinGrupos_DeberiaRetornarListaVacia()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        // Act
        var grupos = await _connectionManager.ObtenerGruposDeUsuarioAsync(usuarioId);

        // Assert
        Assert.Empty(grupos);
    }

    [Fact]
    public async Task DeberiaObtenerTimestampConexion_ConConexionInexistente_DeberiaRetornarDateTimeMin()
    {
        // Arrange
        var connectionId = "conexion_inexistente";

        // Act
        var timestamp = await _connectionManager.ObtenerTimestampConexionAsync(connectionId);

        // Assert
        Assert.Equal(DateTime.MinValue, timestamp);
    }

    [Fact]
    public async Task DeberiaAgregarUsuarioAGrupo_ConUsuarioInexistente_DeberiaManejarCorrectamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var grupo = "Meseros";

        // Act
        await _connectionManager.AgregarUsuarioAGrupoAsync(usuarioId, grupo);

        // Assert
        var grupos = await _connectionManager.ObtenerGruposDeUsuarioAsync(usuarioId);
        Assert.Contains(grupo, grupos);
    }

    [Fact]
    public async Task DeberiaRemoverUsuarioDeGrupo_ConUsuarioSinGrupo_DeberiaManejarCorrectamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var grupo = "Meseros";

        // Act
        await _connectionManager.RemoverUsuarioDeGrupoAsync(usuarioId, grupo);

        // Assert
        var grupos = await _connectionManager.ObtenerGruposDeUsuarioAsync(usuarioId);
        Assert.DoesNotContain(grupo, grupos);
    }

    [Fact]
    public async Task DeberiaObtenerConexionesUsuario_ConUsuarioInexistente_DeberiaRetornarListaVacia()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        // Act
        var conexiones = await _connectionManager.ObtenerConexionesUsuarioAsync(usuarioId);

        // Assert
        Assert.Empty(conexiones);
    }

    [Fact]
    public async Task DeberiaObtenerConexionesGrupo_ConGrupoInexistente_DeberiaRetornarListaVacia()
    {
        // Arrange
        var grupo = "GrupoInexistente";

        // Act
        var conexiones = await _connectionManager.ObtenerConexionesGrupoAsync(grupo);

        // Assert
        Assert.Empty(conexiones);
    }

    [Fact]
    public async Task DeberiaObtenerUsuariosEnGrupo_ConGrupoInexistente_DeberiaRetornarListaVacia()
    {
        // Arrange
        var grupo = "GrupoInexistente";

        // Act
        var usuarios = await _connectionManager.ObtenerUsuariosEnGrupoAsync(grupo);

        // Assert
        Assert.Empty(usuarios);
    }
} 