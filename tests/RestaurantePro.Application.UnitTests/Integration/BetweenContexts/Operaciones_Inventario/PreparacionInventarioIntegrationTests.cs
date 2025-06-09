using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.ConsumirPreparacion;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacion;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.Productos.Services;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Integration.BetweenContexts.Operaciones_Inventario
{
    /// <summary>
    /// Pruebas de integración entre los contextos de Operaciones (Preparaciones) e Inventario
    /// </summary>
    public class PreparacionInventarioIntegrationTests
    {
        private readonly Mock<IApplicationDbContext> _dbContextMock;
        private readonly Mock<IPreparacionRepository> _preparacionRepositoryMock;
        private readonly Mock<IInventarioServiceFacade> _inventarioServiceMock;
        private readonly Mock<IRecetaService> _recetaServiceMock;
        private readonly Mock<IServicioPreparaciones> _servicioPreparacionesMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CrearPreparacionCommandHandler>> _loggerCrearPreparacionMock;
        private readonly Mock<ILogger<ConsumirPreparacionCommandHandler>> _loggerConsumirPreparacionMock;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly Fixture _fixture;

        public PreparacionInventarioIntegrationTests()
        {
            _fixture = new Fixture();
            _dbContextMock = new Mock<IApplicationDbContext>();
            _preparacionRepositoryMock = new Mock<IPreparacionRepository>();
            _inventarioServiceMock = new Mock<IInventarioServiceFacade>();
            _recetaServiceMock = new Mock<IRecetaService>();
            _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();
            _mapperMock = new Mock<IMapper>();
            _loggerCrearPreparacionMock = new Mock<ILogger<CrearPreparacionCommandHandler>>();
            _loggerConsumirPreparacionMock = new Mock<ILogger<ConsumirPreparacionCommandHandler>>();
            _notificationManagerMock = new Mock<INotificationManager>();
        }

        [Fact]
        public async Task IntegrationTest_CrearPreparacion_DebeActualizarInventario_ConsumirPreparacion_NoDebeActualizarInventario()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var preparacionId = Guid.NewGuid();
            var cantidad = 5;

            // Preparación para el test
            var preparacion = PreparacionDiaria.Crear(
                productoId,
                "Producto Test",
                cantidad,
                DateTime.Now);
            preparacion.SetIdForTesting(preparacionId);

            // Ingredientes para el test
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();

            var ingrediente1 = Ingrediente.Crear(
                "Ingrediente 1",
                "Descripción 1",
                "kg",
                1.0m,
                10.0m,
                RotacionIngrediente.Alta);
            ingrediente1.SetIdForTesting(ingrediente1Id);

            var ingrediente2 = Ingrediente.Crear(
                "Ingrediente 2",
                "Descripción 2",
                "l",
                0.5m,
                5.0m,
                RotacionIngrediente.Media);
            ingrediente2.SetIdForTesting(ingrediente2Id);

            // Diccionario de ingredientes requeridos para el producto
            var ingredientesRequeridos = new Dictionary<Guid, decimal>
            {
                { ingrediente1Id, 0.2m },  // 0.2 kg por unidad
                { ingrediente2Id, 0.1m }   // 0.1 l por unidad
            };

            // Mock del DbSet de preparaciones
            var preparacionesDbSetMock = new Mock<DbSet<PreparacionDiaria>>();
            _dbContextMock.Setup(c => c.Preparaciones).Returns(preparacionesDbSetMock.Object);

            // Configurar los mocks para el flujo de integración
            // 1. Obtener ingredientes del producto al crear preparación
            _recetaServiceMock.Setup(s => s.ObtenerIngredientesParaProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(ingredientesRequeridos));

            // 2. Actualizar stock de ingredientes al crear preparación
            _inventarioServiceMock.Setup(s => s.ActualizarStockIngredienteAsync(
                    ingrediente1Id, 
                    It.Is<decimal>(c => c < 0), 
                    TipoMovimientoInventario.Preparacion, 
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(ingrediente1));

            _inventarioServiceMock.Setup(s => s.ActualizarStockIngredienteAsync(
                    ingrediente2Id, 
                    It.Is<decimal>(c => c < 0), 
                    TipoMovimientoInventario.Preparacion, 
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(ingrediente2));

            // 3. Al consumir preparación, no debe actualizar inventario
            _preparacionRepositoryMock.Setup(r => r.ObtenerPorIdAsync(preparacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(preparacion);

            _preparacionRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<PreparacionDiaria>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(preparacion);

            _preparacionRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<PreparacionDiaria>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(preparacion);

            // Crear los handlers
            var crearPreparacionHandler = new CrearPreparacionCommandHandler(
                _preparacionRepositoryMock.Object,
                _mapperMock.Object,
                _loggerCrearPreparacionMock.Object);

            var consumirPreparacionHandler = new ConsumirPreparacionCommandHandler(
                _preparacionRepositoryMock.Object,
                _loggerConsumirPreparacionMock.Object);

            // Act
            // 1. Crear la preparación (esto debe actualizar el inventario)
            var crearCommand = new CrearPreparacionCommand
            {
                ProductoId = productoId,
                NombreProducto = "Producto Test",
                Cantidad = cantidad,
                FechaElaboracion = DateTime.Now
            };
            var crearResult = await crearPreparacionHandler.Handle(crearCommand, CancellationToken.None);

            // 2. Consumir la preparación (esto NO debe actualizar el inventario)
            var consumirCommand = new ConsumirPreparacionCommand
            {
                PreparacionId = preparacionId,
                Cantidad = 2,
                Observaciones = "Prueba de consumo"
            };
            var consumirResult = await consumirPreparacionHandler.Handle(consumirCommand, CancellationToken.None);

            // Assert
            Assert.True(crearResult.Succeeded);
            Assert.True(consumirResult.Succeeded);

            // Verificar que se llamó a los métodos esperados
            _recetaServiceMock.Verify(s => s.ObtenerIngredientesParaProductoAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
            
            // Verificar que se actualizó el inventario para ambos ingredientes al crear la preparación
            _inventarioServiceMock.Verify(s => s.ActualizarStockIngredienteAsync(
                ingrediente1Id, 
                It.Is<decimal>(c => c < 0), 
                TipoMovimientoInventario.Preparacion, 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
                
            _inventarioServiceMock.Verify(s => s.ActualizarStockIngredienteAsync(
                ingrediente2Id, 
                It.Is<decimal>(c => c < 0), 
                TipoMovimientoInventario.Preparacion, 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            // Verificar que NO se actualizó el inventario al consumir la preparación
            // (Las verificaciones anteriores deben seguir siendo las mismas, sin llamadas adicionales)
            _inventarioServiceMock.Verify(s => s.ActualizarStockIngredienteAsync(
                It.IsAny<Guid>(), 
                It.IsAny<decimal>(), 
                It.IsAny<TipoMovimientoInventario>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()), 
                Times.Exactly(2)); // Solo las 2 llamadas anteriores
        }
    }
} 