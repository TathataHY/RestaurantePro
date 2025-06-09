using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.ConsumirPreparacion;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Operaciones.Preparaciones.Commands
{
    public class ConsumirPreparacionCommandTests
    {
        private readonly Mock<IServicioPreparaciones> _servicioPreparacionesMock;
        private readonly Mock<ILogger<ConsumirPreparacionCommandHandler>> _loggerMock;
        private readonly Fixture _fixture;
        private readonly ConsumirPreparacionCommandHandler _handler;

        public ConsumirPreparacionCommandTests()
        {
            _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();
            _loggerMock = new Mock<ILogger<ConsumirPreparacionCommandHandler>>();
            _fixture = new Fixture();
            
            _handler = new ConsumirPreparacionCommandHandler(
                _servicioPreparacionesMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_CuandoHayDisponibilidadYSeConsumeCorrectamente_DebeRetornarExito()
        {
            // Arrange
            var comando = _fixture.Create<ConsumirPreparacionCommand>();
            
            _servicioPreparacionesMock
                .Setup(s => s.VerificarDisponibilidadAsync(comando.ProductoId, comando.Cantidad))
                .ReturnsAsync(Result.Success(true));
                
            _servicioPreparacionesMock
                .Setup(s => s.ConsumirPreparacionAsync(comando.ProductoId, comando.Cantidad))
                .ReturnsAsync(Result.Success());

            // Act
            var resultado = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.True(resultado.Succeeded);
        }

        [Fact]
        public async Task Handle_CuandoNoHayDisponibilidad_DebeRetornarError()
        {
            // Arrange
            var comando = _fixture.Create<ConsumirPreparacionCommand>();
            
            _servicioPreparacionesMock
                .Setup(s => s.VerificarDisponibilidadAsync(comando.ProductoId, comando.Cantidad))
                .ReturnsAsync(Result.Success(false));

            // Act
            var resultado = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.False(resultado.Succeeded);
            Assert.Contains("No hay suficiente cantidad disponible", resultado.Error ?? string.Empty);
        }

        [Fact]
        public async Task Handle_CuandoVerificarDisponibilidadFalla_DebeRetornarError()
        {
            // Arrange
            var comando = _fixture.Create<ConsumirPreparacionCommand>();
            
            _servicioPreparacionesMock
                .Setup(s => s.VerificarDisponibilidadAsync(comando.ProductoId, comando.Cantidad))
                .ReturnsAsync(Result.Failure<bool>("Error al verificar disponibilidad"));

            // Act
            var resultado = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.False(resultado.Succeeded);
            Assert.Contains("Error al verificar disponibilidad", resultado.Error ?? string.Empty);
        }

        [Fact]
        public async Task Handle_CuandoConsumirPreparacionFalla_DebeRetornarError()
        {
            // Arrange
            var comando = _fixture.Create<ConsumirPreparacionCommand>();
            
            _servicioPreparacionesMock
                .Setup(s => s.VerificarDisponibilidadAsync(comando.ProductoId, comando.Cantidad))
                .ReturnsAsync(Result.Success(true));
                
            _servicioPreparacionesMock
                .Setup(s => s.ConsumirPreparacionAsync(comando.ProductoId, comando.Cantidad))
                .ReturnsAsync(Result.Failure("Error al consumir preparación"));

            // Act
            var resultado = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.False(resultado.Succeeded);
            Assert.Contains("Error al consumir preparación", resultado.Error ?? string.Empty);
        }
    }
} 