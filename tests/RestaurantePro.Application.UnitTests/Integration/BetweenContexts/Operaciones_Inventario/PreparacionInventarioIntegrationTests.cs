using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.ConsumirPreparacion;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacion;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using RestaurantePro.Domain.Inventario.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Integration.BetweenContexts.Operaciones_Inventario
{
    /// <summary>
    /// Clase que representa la relación entre un producto y un ingrediente, necesaria para las pruebas
    /// </summary>
    public class IngredienteProducto
    {
        public Guid IngredienteId { get; set; }
        public Guid ProductoId { get; set; }
        public decimal Cantidad { get; set; }
        public bool EsOpcional { get; set; }
    }

    /// <summary>
    /// Pruebas de integración entre el contexto de Operaciones (Preparaciones) e Inventario
    /// </summary>
    public class PreparacionInventarioIntegrationTests
    {
        private readonly Mock<IPreparacionRepository> _preparacionRepositoryMock;
        private readonly Mock<IInventarioServiceFacade> _inventarioServiceMock;
        private readonly Mock<IServicioPreparaciones> _servicioPreparacionesMock;
        private readonly Mock<ILogger<CrearPreparacionCommandHandler>> _loggerMock;
        private readonly Fixture _fixture;

        public PreparacionInventarioIntegrationTests()
        {
            _preparacionRepositoryMock = new Mock<IPreparacionRepository>();
            _inventarioServiceMock = new Mock<IInventarioServiceFacade>();
            _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();
            _loggerMock = new Mock<ILogger<CrearPreparacionCommandHandler>>();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Preparacion_Debe_Afectar_Inventario_Correctamente()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var ingredienteId = Guid.NewGuid();
            var chefId = Guid.NewGuid();
            var cantidad = 5;
            var cantidadIngrediente = 0.5m; // Cantidad de ingrediente por unidad de producto

            // Configurar ingrediente
            var ingrediente = new Ingrediente
            {
                Id = ingredienteId,
                Nombre = "Ingrediente de prueba",
                UnidadMedida = "kg",
                CantidadDisponible = 10,
                PrecioUnitario = 5.5m
            };

            // Configurar preparación
            var preparacion = PreparacionDiaria.Crear(
                productoId,
                cantidad,
                chefId,
                DateTime.Now.AddDays(1),
                "Observaciones de prueba",
                DateTime.Now);
            preparacion.Id = Guid.NewGuid();

            // Configurar el servicio de inventario
            _inventarioServiceMock
                .Setup(s => s.RegistrarConsumoIngredienteAsync(
                    ingredienteId, 
                    It.IsAny<decimal>(), 
                    It.IsAny<Guid>(), 
                    TipoMovimientoIngrediente.Preparacion,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _inventarioServiceMock
                .Setup(s => s.ObtenerIngredientesProductoAsync(
                    productoId, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<IEnumerable<IngredienteProducto>>.Success(
                    new List<IngredienteProducto> 
                    {
                        new IngredienteProducto 
                        { 
                            IngredienteId = ingredienteId,
                            ProductoId = productoId,
                            Cantidad = cantidadIngrediente
                        }
                    }));

            // Configurar el servicio de preparaciones
            _servicioPreparacionesMock
                .Setup(s => s.PrepararProductoAsync(
                    productoId,
                    cantidad,
                    chefId,
                    It.IsAny<DateTime?>(),
                    It.IsAny<string>()))
                .ReturnsAsync(Result<PreparacionDiaria>.Success(preparacion));

            _servicioPreparacionesMock
                .Setup(s => s.ConsumirPreparacionAsync(
                    productoId,
                    It.IsAny<int>()))
                .ReturnsAsync(Result.Success());

            // Act
            // 1. Crear una preparación
            var crearPreparacionCommand = new CrearPreparacionCommand
            {
                ProductoId = productoId,
                Cantidad = cantidad,
                ChefId = chefId,
                FechaVencimiento = DateTime.Now.AddDays(1),
                Observaciones = "Preparación para prueba"
            };

            var crearPreparacionHandler = new CrearPreparacionCommandHandler(
                _servicioPreparacionesMock.Object,
                _inventarioServiceMock.Object,
                _loggerMock.Object);

            var crearResult = await crearPreparacionHandler.Handle(
                crearPreparacionCommand,
                CancellationToken.None);

            // 2. Consumir la preparación
            var consumirPreparacionCommand = new ConsumirPreparacionCommand
            {
                ProductoId = productoId,
                Cantidad = 2,
                ComandaId = Guid.NewGuid(),
                Observaciones = "Consumo para prueba"
            };

            var consumirPreparacionHandler = new ConsumirPreparacionCommandHandler(
                _servicioPreparacionesMock.Object,
                Mock.Of<ILogger<ConsumirPreparacionCommandHandler>>());

            var consumirResult = await consumirPreparacionHandler.Handle(
                consumirPreparacionCommand,
                CancellationToken.None);

            // Assert
            Assert.NotNull(crearResult);
            Assert.True(consumirResult.Succeeded);

            // Verificar que se llamaron los métodos correctos
            _servicioPreparacionesMock.Verify(
                s => s.PrepararProductoAsync(
                    productoId,
                    cantidad,
                    chefId,
                    It.IsAny<DateTime?>(),
                    It.IsAny<string>()),
                Times.Once);

            _servicioPreparacionesMock.Verify(
                s => s.ConsumirPreparacionAsync(
                    productoId,
                    2),
                Times.Once);

            _inventarioServiceMock.Verify(
                s => s.ObtenerIngredientesProductoAsync(
                    productoId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _inventarioServiceMock.Verify(
                s => s.RegistrarConsumoIngredienteAsync(
                    ingredienteId,
                    cantidadIngrediente * cantidad,
                    It.IsAny<Guid>(),
                    TipoMovimientoIngrediente.Preparacion,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 