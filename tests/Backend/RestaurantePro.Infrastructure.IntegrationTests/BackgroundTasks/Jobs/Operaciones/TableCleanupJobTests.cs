using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Operaciones;
using RestaurantePro.Infrastructure.BackgroundTasks.Settings;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs.Operaciones
{
    public class TableCleanupJobTests
    {
        private readonly IMesaRepository _mesaRepositoryMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IOptions<TableCleanupJobSettings> _settingsMock;
        private readonly ILogger<TableCleanupJob> _loggerMock;
        private readonly TableCleanupJob _job;

        public TableCleanupJobTests()
        {
            _mesaRepositoryMock = Substitute.For<IMesaRepository>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _settingsMock = Options.Create(new TableCleanupJobSettings { StaleTimeMinutes = 30 });
            _loggerMock = Substitute.For<ILogger<TableCleanupJob>>();
            
            _job = new TableCleanupJob(_loggerMock, _unitOfWorkMock, _mesaRepositoryMock, _settingsMock);
        }

        [Fact]
        public async Task ExecuteInternalAsync_NoStaleTables_ShouldLogAndDoNothing()
        {
            // Arrange
            _mesaRepositoryMock.ObtenerMesasSuciaPorAntiguedad(Arg.Any<int>())
                .Returns(Task.FromResult(Enumerable.Empty<Mesa>()));

            // Act
            await _job.DoWork(CancellationToken.None);

            // Assert
            _loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("No se encontraron mesas para limpiar")),
                null,
                Arg.Any<Func<object, Exception, string>>());

            await _unitOfWorkMock.DidNotReceive().GuardarCambiosAsync();
        }

        [Fact]
        public async Task ExecuteInternalAsync_WithStaleTables_ShouldUpdateAndSaveChanges()
        {
            // Arrange
            var mesaSucia = Mesa.Crear(1, 4, "Test");
            // Para simular una mesa sucia, la marcamos como EnLimpieza
            typeof(Mesa).GetProperty(nameof(Mesa.Estado)).SetValue(mesaSucia, EstadoMesa.EnLimpieza);

            var mesasSucias = new List<Mesa> { mesaSucia };

            _mesaRepositoryMock.ObtenerMesasSuciaPorAntiguedad(Arg.Any<int>())
                .Returns(Task.FromResult<IEnumerable<Mesa>>(mesasSucias));

            // Act
            await _job.DoWork(CancellationToken.None);

            // Assert
            _loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Se encontraron 1 mesas para limpiar")),
                null,
                Arg.Any<Func<object, Exception, string>>());

            _loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Trabajo de limpieza de mesas completado. Se actualizaron 1 mesas.")),
                null,
                Arg.Any<Func<object, Exception, string>>());
            
            await _unitOfWorkMock.Received(1).GuardarCambiosAsync(CancellationToken.None);
            Assert.Equal(EstadoMesa.Disponible, mesaSucia.Estado);
        }
    }
}