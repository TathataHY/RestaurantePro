using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using RestaurantePro.Domain.Core.Notificaciones.Enums;
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Core;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using Xunit;
using Xunit.Abstractions;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs.Core
{
    public class NotificationCleanupJobTests : IntegrationTestBase, IAsyncLifetime
    {
        private INotificacionRepository _notificacionRepository;
        private IUsuarioRepository _usuarioRepository;
        private IUnitOfWork _unitOfWork;
        private IOptions<NotificationCleanupOptions> _options;
        private ILogger<NotificationCleanupJob> _logger;
        private Mock<ILogger<NotificationCleanupJob>> _loggerMock;

        public NotificationCleanupJobTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _notificacionRepository = ServiceProvider.GetRequiredService<INotificacionRepository>();
            _usuarioRepository = ServiceProvider.GetRequiredService<IUsuarioRepository>();
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            _loggerMock = new Mock<ILogger<NotificationCleanupJob>>();
            _logger = _loggerMock.Object;

            var settings = new NotificationCleanupOptions { DaysToKeep = 30 };
            _options = Options.Create(settings);
        }

        public override Task DisposeAsync() => base.DisposeAsync();

        [Fact]
        public async Task ExecuteAsync_DeberiaEliminarNotificacionesAntiguas()
        {
            // Arrange
            var usuario = Usuario.Crear("testuser", "Test User", "test@test.com", RolUsuario.Mesero);
            await _usuarioRepository.AgregarAsync(usuario, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var diasAntiguos = _options.Value.DaysToKeep + 1;
            var fechaAntigua = DateTime.UtcNow.AddDays(-diasAntiguos);
            
            var notificacionAntigua = Notificacion.Crear(
                "Mensaje de prueba antiguo",
                "Detalles de prueba",
                TipoNotificacion.Informativa,
                usuario.Id,
                null,
                fechaAntigua
            );
            
            await _notificacionRepository.AgregarAsync(notificacionAntigua, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var job = new NotificationCleanupJob(_logger, _notificacionRepository, _unitOfWork, _options);

            // Act
            await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var notificacionEnDb = await _notificacionRepository.PrimeroODefaultAsync(n => n.Id == notificacionAntigua.Id);
            notificacionEnDb.Should().BeNull();
            
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Se eliminaron 1 notificaciones antiguas")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_NoDeberiaEliminarNotificacionesRecientes()
        {
            // Arrange
            var usuario = Usuario.Crear("testuser2", "Test User 2", "test2@test.com", RolUsuario.Mesero);
            await _usuarioRepository.AgregarAsync(usuario, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var notificacionReciente = Notificacion.Crear(
                "Mensaje de prueba reciente",
                "Detalles de prueba",
                TipoNotificacion.Informativa,
                usuario.Id
            );
            
            await _notificacionRepository.AgregarAsync(notificacionReciente, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            
            var job = new NotificationCleanupJob(_logger, _notificacionRepository, _unitOfWork, _options);

            // Act
            await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var notificacionEnDb = await _notificacionRepository.ObtenerPorIdAsync(notificacionReciente.Id);
            notificacionEnDb.Should().NotBeNull();
            
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Se eliminaron 0 notificaciones antiguas")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteInternalAsync_DeberiaRegistrarLogsCorrectamente()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NotificationCleanupJob>>();
            var job = new NotificationCleanupJob(mockLogger.Object, _notificacionRepository, _unitOfWork, _options);

            // Act
            await job.ExecuteAsync(CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Eliminando notificaciones anteriores a")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString().Contains("Se eliminaron")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
} 