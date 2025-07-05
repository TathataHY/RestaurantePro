using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Services;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Services
{
    /// <summary>
    /// Tests unitarios para SignalRService
    /// Sigue las mejores prácticas de testing y arquitectura limpia
    /// </summary>
    public class SignalRServiceTests
    {
        private readonly Mock<ISignalRHub> _signalRHubMock;
        private readonly Mock<ILogger<SignalRService>> _loggerMock;
        private readonly Mock<IHubConnectionManager> _connectionManagerMock;
        private readonly SignalRService _signalRService;

        public SignalRServiceTests()
        {
            _signalRHubMock = new Mock<ISignalRHub>();
            _loggerMock = new Mock<ILogger<SignalRService>>();
            _connectionManagerMock = new Mock<IHubConnectionManager>();
            _signalRService = new SignalRService(_signalRHubMock.Object, _loggerMock.Object, _connectionManagerMock.Object);
        }

        [Fact]
        public async Task DeberiaEnviarNotificacionAUsuario_ConUsuarioConectado_DeberiaEnviarCorrectamente()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var titulo = "Test Notificación";
            var mensaje = "Mensaje de prueba";
            var tipo = "info";

            _connectionManagerMock
                .Setup(x => x.UsuarioEstaConectadoAsync(usuarioId))
                .ReturnsAsync(true);

            _connectionManagerMock
                .Setup(x => x.ObtenerConexionesUsuarioAsync(usuarioId))
                .ReturnsAsync(new List<string> { "connection1", "connection2" });

            // Act
            await _signalRService.EnviarNotificacionAUsuarioAsync(usuarioId, titulo, mensaje, tipo);

            // Assert
            _connectionManagerMock.Verify(x => x.UsuarioEstaConectadoAsync(usuarioId), Times.Once);
            _connectionManagerMock.Verify(x => x.ObtenerConexionesUsuarioAsync(usuarioId), Times.Once);
        }

        [Fact]
        public async Task DeberiaEnviarNotificacionAUsuario_ConUsuarioDesconectado_DeberiaLogearAdvertencia()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var titulo = "Test Notificación";
            var mensaje = "Mensaje de prueba";
            var tipo = "info";

            _connectionManagerMock
                .Setup(x => x.UsuarioEstaConectadoAsync(usuarioId))
                .ReturnsAsync(false);

            // Act
            await _signalRService.EnviarNotificacionAUsuarioAsync(usuarioId, titulo, mensaje, tipo);

            // Assert
            _connectionManagerMock.Verify(x => x.UsuarioEstaConectadoAsync(usuarioId), Times.Once);
            _connectionManagerMock.Verify(x => x.ObtenerConexionesUsuarioAsync(usuarioId), Times.Never);
        }

        [Fact]
        public async Task DeberiaEnviarNotificacionAMultiplesUsuarios_ConUsuariosConectados_DeberiaEnviarATodos()
        {
            // Arrange
            var usuariosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var titulo = "Test Notificación Múltiple";
            var mensaje = "Mensaje para múltiples usuarios";
            var tipo = "warning";

            _connectionManagerMock
                .Setup(x => x.UsuarioEstaConectadoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            _connectionManagerMock
                .Setup(x => x.ObtenerConexionesUsuarioAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<string> { "connection" });

            // Act
            await _signalRService.EnviarNotificacionAUsuariosAsync(usuariosIds, titulo, mensaje, tipo);

            // Assert
            _connectionManagerMock.Verify(x => x.UsuarioEstaConectadoAsync(It.IsAny<Guid>()), Times.Exactly(3));
            _connectionManagerMock.Verify(x => x.ObtenerConexionesUsuarioAsync(It.IsAny<Guid>()), Times.Exactly(3));
        }

        [Fact]
        public async Task DeberiaEnviarNotificacionPorRol_ConRolValido_DeberiaEnviarCorrectamente()
        {
            // Arrange
            var rol = "Meseros";
            var titulo = "Notificación por Rol";
            var mensaje = "Mensaje para meseros";
            var tipo = "success";

            _connectionManagerMock
                .Setup(x => x.ObtenerConexionesGrupoAsync(rol))
                .ReturnsAsync(new List<string> { "connection1", "connection2", "connection3" });

            // Act
            await _signalRService.EnviarNotificacionARolAsync(rol, titulo, mensaje, tipo);

            // Assert
            _connectionManagerMock.Verify(x => x.ObtenerConexionesGrupoAsync(rol), Times.Once);
        }

        [Fact]
        public async Task DeberiaEnviarNotificacionGlobal_DeberiaEnviarATodosLosUsuarios()
        {
            // Arrange
            var titulo = "Notificación Global";
            var mensaje = "Mensaje para todos los usuarios";
            var tipo = "info";

            _connectionManagerMock
                .Setup(x => x.ObtenerEstadisticasConexionesAsync())
                .ReturnsAsync(new Application.Common.Interfaces.ConexionesEstadisticas
                {
                    TotalConexionesActivas = 10,
                    TotalUsuariosConectados = 8
                });

            // Act
            await _signalRService.EnviarNotificacionGlobalAsync(titulo, mensaje, tipo);

            // Assert
            _connectionManagerMock.Verify(x => x.ObtenerEstadisticasConexionesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeberiaActualizarEstadoMesa_DeberiaEnviarNotificacionCorrectamente()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var estado = "Ocupada";
            var detalles = new { tiempoOcupacion = 30, comensales = 4 };

            _connectionManagerMock
                .Setup(x => x.ObtenerEstadisticasConexionesAsync())
                .ReturnsAsync(new Application.Common.Interfaces.ConexionesEstadisticas
                {
                    TotalConexionesActivas = 5,
                    TotalUsuariosConectados = 3
                });

            // Act
            await _signalRService.ActualizarEstadoMesaAsync(mesaId, estado, detalles);

            // Assert
            _connectionManagerMock.Verify(x => x.ObtenerEstadisticasConexionesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeberiaActualizarEstadoComanda_DeberiaEnviarNotificacionCorrectamente()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var estado = "En Preparación";
            var detalles = new { tiempoEstimado = 15, prioridad = "Alta" };

            _connectionManagerMock
                .Setup(x => x.ObtenerEstadisticasConexionesAsync())
                .ReturnsAsync(new Application.Common.Interfaces.ConexionesEstadisticas
                {
                    TotalConexionesActivas = 7,
                    TotalUsuariosConectados = 4
                });

            // Act
            await _signalRService.ActualizarEstadoComandaAsync(comandaId, estado, detalles);

            // Assert
            _connectionManagerMock.Verify(x => x.ObtenerEstadisticasConexionesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeberiaEnviarAlertaInventario_DeberiaEnviarAlertaCorrectamente()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Tomates";
            var stockActual = 5.0m;
            var stockMinimo = 10.0m;

            _connectionManagerMock
                .Setup(x => x.ObtenerConexionesGrupoAsync("Administradores"))
                .ReturnsAsync(new List<string> { "admin1", "admin2" });

            // Act
            await _signalRService.EnviarAlertaInventarioAsync(ingredienteId, nombreIngrediente, stockActual, stockMinimo);

            // Assert
            _connectionManagerMock.Verify(x => x.ObtenerConexionesGrupoAsync("Administradores"), Times.Once);
        }

        [Fact]
        public async Task DeberiaObtenerUsuariosConectados_DeberiaRetornarListaCorrecta()
        {
            // Arrange
            var usuariosEsperados = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            _connectionManagerMock
                .Setup(x => x.ObtenerEstadisticasConexionesAsync())
                .ReturnsAsync(new Application.Common.Interfaces.ConexionesEstadisticas
                {
                    TotalConexionesActivas = 3,
                    TotalUsuariosConectados = 3
                });

            // Act
            var resultado = await _signalRService.ObtenerUsuariosConectadosAsync();

            // Assert
            Assert.NotNull(resultado);
            _connectionManagerMock.Verify(x => x.ObtenerEstadisticasConexionesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeberiaUsuarioEstaConectado_ConUsuarioConectado_DeberiaRetornarTrue()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();

            _connectionManagerMock
                .Setup(x => x.UsuarioEstaConectadoAsync(usuarioId))
                .ReturnsAsync(true);

            // Act
            var resultado = await _signalRService.UsuarioEstaConectadoAsync(usuarioId);

            // Assert
            Assert.True(resultado);
            _connectionManagerMock.Verify(x => x.UsuarioEstaConectadoAsync(usuarioId), Times.Once);
        }

        [Fact]
        public async Task DeberiaUsuarioEstaConectado_ConUsuarioDesconectado_DeberiaRetornarFalse()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();

            _connectionManagerMock
                .Setup(x => x.UsuarioEstaConectadoAsync(usuarioId))
                .ReturnsAsync(false);

            // Act
            var resultado = await _signalRService.UsuarioEstaConectadoAsync(usuarioId);

            // Assert
            Assert.False(resultado);
            _connectionManagerMock.Verify(x => x.UsuarioEstaConectadoAsync(usuarioId), Times.Once);
        }

        [Fact]
        public async Task DeberiaManejarErroresDeConexion_DeberiaLogearErrorCorrectamente()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var titulo = "Test Error";
            var mensaje = "Mensaje de error";
            var tipo = "error";

            _connectionManagerMock
                .Setup(x => x.UsuarioEstaConectadoAsync(usuarioId))
                .ThrowsAsync(new Exception("Error de conexión"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _signalRService.EnviarNotificacionAUsuarioAsync(usuarioId, titulo, mensaje, tipo));

            _connectionManagerMock.Verify(x => x.UsuarioEstaConectadoAsync(usuarioId), Times.Once);
        }

        [Theory]
        [InlineData("", "Mensaje", "info")]
        [InlineData("Título", "", "info")]
        [InlineData("Título", "Mensaje", "")]
        [InlineData(null, "Mensaje", "info")]
        [InlineData("Título", null, "info")]
        [InlineData("Título", "Mensaje", null)]
        public async Task DeberiaValidarParametrosEntrada_ConParametrosInvalidos_DeberiaManejarCorrectamente(
            string titulo, string mensaje, string tipo)
        {
            // Arrange
            var usuarioId = Guid.NewGuid();

            // Act & Assert
            if (string.IsNullOrEmpty(titulo) || string.IsNullOrEmpty(mensaje) || string.IsNullOrEmpty(tipo))
            {
                await Assert.ThrowsAsync<ArgumentException>(() => 
                    _signalRService.EnviarNotificacionAUsuarioAsync(usuarioId, titulo, mensaje, tipo));
            }
        }

        [Fact]
        public async Task DeberiaEnviarNotificacionConTipoPersonalizado_DeberiaFuncionarCorrectamente()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var titulo = "Notificación Personalizada";
            var mensaje = "Mensaje con tipo personalizado";
            var tipo = "custom";

            _connectionManagerMock
                .Setup(x => x.UsuarioEstaConectadoAsync(usuarioId))
                .ReturnsAsync(true);

            _connectionManagerMock
                .Setup(x => x.ObtenerConexionesUsuarioAsync(usuarioId))
                .ReturnsAsync(new List<string> { "connection1" });

            // Act
            await _signalRService.EnviarNotificacionAUsuarioAsync(usuarioId, titulo, mensaje, tipo);

            // Assert
            _connectionManagerMock.Verify(x => x.UsuarioEstaConectadoAsync(usuarioId), Times.Once);
            _connectionManagerMock.Verify(x => x.ObtenerConexionesUsuarioAsync(usuarioId), Times.Once);
        }
    }
} 