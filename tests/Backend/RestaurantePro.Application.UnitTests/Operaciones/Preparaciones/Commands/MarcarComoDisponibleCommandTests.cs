using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.MarcarComoDisponible;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Operaciones.Preparaciones.Commands
{
    public class MarcarComoDisponibleCommandTests
    {
        private readonly Mock<IServicioPreparaciones> _servicioPreparacionesMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<MarcarComoDisponibleCommandHandler>> _loggerMock;
        private readonly Fixture _fixture;
        private readonly MarcarComoDisponibleCommandHandler _handler;

        public MarcarComoDisponibleCommandTests()
        {
            _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<MarcarComoDisponibleCommandHandler>>();
            _fixture = new Fixture();
            
            _handler = new MarcarComoDisponibleCommandHandler(
                _servicioPreparacionesMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenPreparacionMarkedAsDisponible()
        {
            // Arrange
            var command = _fixture.Create<MarcarComoDisponibleCommand>();
            var preparacionId = command.PreparacionId;

            _servicioPreparacionesMock
                .Setup(x => x.MarcarComoDisponibleAsync(preparacionId))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            _servicioPreparacionesMock.Verify(x => x.MarcarComoDisponibleAsync(preparacionId), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenMarcarComoDisponibleFails()
        {
            // Arrange
            var command = _fixture.Create<MarcarComoDisponibleCommand>();
            var preparacionId = command.PreparacionId;
            var errorMessage = "Error al marcar como disponible";

            _servicioPreparacionesMock
                .Setup(x => x.MarcarComoDisponibleAsync(preparacionId))
                .ReturnsAsync(Result.Failure(errorMessage));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(errorMessage, result.Error);
            _servicioPreparacionesMock.Verify(x => x.MarcarComoDisponibleAsync(preparacionId), Times.Once);
        }

        [Fact]
        public async Task Handle_CuandoLaPreparacionNoExiste_DebeRetornarError()
        {
            // Arrange
            var comando = _fixture.Create<MarcarComoDisponibleCommand>();
            
            _servicioPreparacionesMock
                .Setup(s => s.MarcarComoDisponibleAsync(comando.PreparacionId))
                .ReturnsAsync(Result.Failure("La preparación no existe"));

            // Act
            var resultado = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            Assert.False(resultado.Succeeded);
            Assert.Contains("La preparación no existe", resultado.Error ?? string.Empty);
        }

        [Fact]
        public async Task Handle_CuandoNoSePuedeObtenerLaPreparacion_DebeRetornarError()
        {
            // Arrange
            var command = _fixture.Create<MarcarComoDisponibleCommand>();
            var errorMessage = "No se pudo obtener la preparación";

            _servicioPreparacionesMock
                .Setup(x => x.MarcarComoDisponibleAsync(command.PreparacionId))
                .ReturnsAsync(Result.Failure(errorMessage));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(errorMessage, result.Error);
        }
    }
} 