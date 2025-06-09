using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarItemComanda;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacion;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.MarcarComoDisponible;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Integration.WithinContext.Operaciones
{
    /// <summary>
    /// Pruebas de integración para el flujo completo de preparaciones y comandas
    /// </summary>
    public class PreparacionComandaIntegrationTests
    {
        private readonly Mock<IPreparacionRepository> _preparacionRepositoryMock;
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IServicioPreparaciones> _servicioPreparacionesMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CrearPreparacionCommandHandler>> _loggerCrearPreparacionMock;
        private readonly Mock<ILogger<MarcarComoDisponibleCommandHandler>> _loggerMarcarDisponibleMock;
        private readonly Mock<ILogger<AgregarItemComandaHandler>> _loggerAgregarItemMock;
        private readonly Fixture _fixture;

        public PreparacionComandaIntegrationTests()
        {
            _fixture = new Fixture();
            _preparacionRepositoryMock = new Mock<IPreparacionRepository>();
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();
            _mapperMock = new Mock<IMapper>();
            _loggerCrearPreparacionMock = new Mock<ILogger<CrearPreparacionCommandHandler>>();
            _loggerMarcarDisponibleMock = new Mock<ILogger<MarcarComoDisponibleCommandHandler>>();
            _loggerAgregarItemMock = new Mock<ILogger<AgregarItemComandaHandler>>();
        }

        [Fact]
        public async Task IntegrationTest_CrearPreparacion_MarcarDisponible_AgregarItemComanda_Success()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 10;
            var preparacionId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();

            // Preparación diaria para el test
            var preparacionDiaria = PreparacionDiaria.Crear(
                productoId,
                "Producto Test",
                cantidad,
                DateTime.Now);
            preparacionDiaria.SetIdForTesting(preparacionId);

            // Comanda para el test
            var comanda = Comanda.Crear(
                "Mesa 1",
                "Cliente Test",
                2,
                new List<ItemComanda>());
            comanda.SetIdForTesting(comandaId);

            // Configurar mocks para el flujo de la integración
            // 1. Crear preparación
            _preparacionRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<PreparacionDiaria>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(preparacionDiaria);

            // 2. Marcar como disponible
            _preparacionRepositoryMock.Setup(r => r.ObtenerPorIdAsync(preparacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(preparacionDiaria);

            // 3. Agregar item a comanda
            _comandaRepositoryMock.Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
            _servicioPreparacionesMock.Setup(s => s.VerificarDisponibilidadAsync(productoId, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(true));
            _comandaRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            // Crear los handlers
            var crearPreparacionHandler = new CrearPreparacionCommandHandler(
                _preparacionRepositoryMock.Object,
                _mapperMock.Object,
                _loggerCrearPreparacionMock.Object);

            var marcarDisponibleHandler = new MarcarComoDisponibleCommandHandler(
                _preparacionRepositoryMock.Object,
                _loggerMarcarDisponibleMock.Object);

            var agregarItemHandler = new AgregarItemComandaHandler(
                _comandaRepositoryMock.Object,
                _servicioPreparacionesMock.Object,
                _loggerAgregarItemMock.Object);

            // Act
            // 1. Crear la preparación
            var crearCommand = new CrearPreparacionCommand
            {
                ProductoId = productoId,
                NombreProducto = "Producto Test",
                Cantidad = cantidad,
                FechaElaboracion = DateTime.Now
            };
            var crearResult = await crearPreparacionHandler.Handle(crearCommand, CancellationToken.None);

            // 2. Marcar como disponible
            var marcarCommand = new MarcarComoDisponibleCommand
            {
                PreparacionId = preparacionId
            };
            var marcarResult = await marcarDisponibleHandler.Handle(marcarCommand, CancellationToken.None);

            // 3. Agregar item a la comanda
            var agregarItemCommand = new AgregarItemComandaCommand
            {
                ComandaId = comandaId,
                ProductoId = productoId,
                Cantidad = 1,
                Observaciones = "Prueba de integración",
                Personalizaciones = new List<PersonalizacionDto>()
            };
            var agregarItemResult = await agregarItemHandler.Handle(agregarItemCommand, CancellationToken.None);

            // Assert
            Assert.True(crearResult.Succeeded);
            Assert.True(marcarResult.Succeeded);
            Assert.True(agregarItemResult.Succeeded);

            // Verificar que los métodos fueron llamados con los parámetros correctos
            _preparacionRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<PreparacionDiaria>(), It.IsAny<CancellationToken>()), Times.Once);
            _preparacionRepositoryMock.Verify(r => r.ObtenerPorIdAsync(preparacionId, It.IsAny<CancellationToken>()), Times.Once);
            _preparacionRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<PreparacionDiaria>(), It.IsAny<CancellationToken>()), Times.Once);
            _comandaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()), Times.Once);
            _servicioPreparacionesMock.Verify(s => s.VerificarDisponibilidadAsync(productoId, 1, It.IsAny<CancellationToken>()), Times.Once);
            _comandaRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 