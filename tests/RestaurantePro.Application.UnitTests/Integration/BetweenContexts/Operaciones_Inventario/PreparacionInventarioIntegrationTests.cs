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
using RestaurantePro.Domain.Core.SharedKernel.Services;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
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
        private readonly Mock<ILogger<CrearPreparacionCommandHandler>> _loggerCrearPreparacionMock;
        private readonly Mock<ILogger<ConsumirPreparacionCommandHandler>> _loggerConsumirPreparacionMock;
        private readonly Fixture _fixture;

        public PreparacionInventarioIntegrationTests()
        {
            _fixture = new Fixture();
            _dbContextMock = new Mock<IApplicationDbContext>();
            _preparacionRepositoryMock = new Mock<IPreparacionRepository>();
            _inventarioServiceMock = new Mock<IInventarioServiceFacade>();
            _recetaServiceMock = new Mock<IRecetaService>();
            _loggerCrearPreparacionMock = new Mock<ILogger<CrearPreparacionCommandHandler>>();
            _loggerConsumirPreparacionMock = new Mock<ILogger<ConsumirPreparacionCommandHandler>>();
        }

        [Fact]
        public async Task IntegrationTest_CrearPreparacion_DebeActualizarInventario_ConsumirPreparacion_NoDebeActualizarInventario()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var preparacionId = Guid.NewGuid();
            var chefId = Guid.NewGuid();
            var cantidad = 5;

            // Preparación para el test
            var preparacion = PreparacionDiaria.Crear(
                productoId,
                cantidad,
                chefId,
                DateTime.Now.AddDays(1));
            preparacion.SetIdForTesting(preparacionId);

            // Ingredientes para el test
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();

            var ingrediente1 = Ingrediente.Crear(
                "Ingrediente 1",
                "Descripción 1",
                "kg",
                UnidadMedida.Kilogramo,
                10.0m,
                5.0m,
                RotacionIngrediente.Alta);
            ingrediente1.SetIdForTesting(ingrediente1Id);

            var ingrediente2 = Ingrediente.Crear(
                "Ingrediente 2",
                "Descripción 2",
                "l",
                UnidadMedida.Litro,
                5.0m,
                2.0m,
                RotacionIngrediente.Media);
            ingrediente2.SetIdForTesting(ingrediente2Id);

            // Diccionario de ingredientes requeridos para el producto
            var ingredientesRequeridos = new Dictionary<Guid, decimal>
            {
                { ingrediente1Id, 0.2m },  // 0.2 kg por unidad
                { ingrediente2Id, 0.1m }   // 0.1 l por unidad
            };

            // Configuración de mocks
            _preparacionRepositoryMock.Setup(r => r.AgregarAsync(It.Is<PreparacionDiaria>(p => p.ProductoId == productoId)))
                .Returns(Task.CompletedTask);
            
            _preparacionRepositoryMock.Setup(r => r.GuardarCambiosAsync())
                .ReturnsAsync(1);
            
            _preparacionRepositoryMock.Setup(r => r.ObtenerPorIdAsync(preparacionId))
                .ReturnsAsync(preparacion);
            
            // Obtener ingredientes del producto al crear preparación
            _recetaServiceMock.Setup(s => s.ObtenerIngredientesParaProductoAsync(
                productoId, 
                CancellationToken.None))
                .ReturnsAsync(Result.Success(ingredientesRequeridos));

            // Actualizar stock de ingredientes al crear preparación
            _inventarioServiceMock.Setup(s => s.ActualizarStockIngredienteAsync(
                    ingrediente1Id, 
                    0.2m * cantidad, 
                    TipoMovimientoInventario.Salida, 
                    "Preparación de producto"))
                .ReturnsAsync(Result.Success(ingrediente1));
                
            _inventarioServiceMock.Setup(s => s.ActualizarStockIngredienteAsync(
                    ingrediente2Id, 
                    0.1m * cantidad, 
                    TipoMovimientoInventario.Salida, 
                    "Preparación de producto"))
                .ReturnsAsync(Result.Success(ingrediente2));

            // Creación de handlers y servicios
            var mapperMock = new Mock<IMapper>();
            var dateTimeServiceMock = new Mock<IDateTimeService>();
            dateTimeServiceMock.Setup(d => d.Now).Returns(DateTime.Now);
            
            // Crear servicios de dominio
            var notificationManagerMock = new Mock<INotificationManager>();
            notificationManagerMock.Setup(n => n.HasErrors).Returns(false);
            
            var servicioPreparaciones = new ServicioPreparaciones(
                new Mock<ILogger<ServicioPreparaciones>>().Object,
                notificationManagerMock.Object,
                dateTimeServiceMock.Object,
                _preparacionRepositoryMock.Object);

            // Crear handlers
            var crearPreparacionHandler = new CrearPreparacionCommandHandler(
                _dbContextMock.Object,
                mapperMock.Object,
                dateTimeServiceMock.Object);
                
            var consumirPreparacionHandler = new ConsumirPreparacionCommandHandler(
                servicioPreparaciones,
                _loggerConsumirPreparacionMock.Object);

            // Act
            // 1. Crear la preparación (esto debe actualizar el inventario)
            var crearCommand = new CrearPreparacionCommand
            {
                ProductoId = productoId,
                Cantidad = cantidad,
                ChefId = chefId,
                FechaVencimiento = DateTime.Now.AddDays(1)
            };
            var crearResult = await crearPreparacionHandler.Handle(crearCommand, CancellationToken.None);

            // 2. Consumir la preparación (esto NO debe actualizar el inventario)
            var consumirCommand = new ConsumirPreparacionCommand
            {
                PreparacionId = preparacionId,
                Cantidad = 2
            };
            var consumirResult = await consumirPreparacionHandler.Handle(consumirCommand, CancellationToken.None);

            // Assert
            Assert.NotNull(crearResult);
            Assert.True(consumirResult.Succeeded);

            // Verificar que se llamó a los métodos esperados
            _preparacionRepositoryMock.Verify(r => r.AgregarAsync(It.Is<PreparacionDiaria>(p => p.ProductoId == productoId)), Times.Once);
            _preparacionRepositoryMock.Verify(r => r.GuardarCambiosAsync(), Times.AtLeastOnce);
            _preparacionRepositoryMock.Verify(r => r.ObtenerPorIdAsync(preparacionId), Times.AtLeastOnce);
            
            // Verificar los ingredientes se actualizaron en inventario
            _inventarioServiceMock.Verify(
                s => s.ActualizarStockIngredienteAsync(
                    ingrediente1Id,
                    0.2m * cantidad,
                    TipoMovimientoInventario.Salida, 
                    "Preparación de producto"),
                Times.AtLeastOnce);
                
            _inventarioServiceMock.Verify(
                s => s.ActualizarStockIngredienteAsync(
                    ingrediente2Id,
                    0.1m * cantidad,
                    TipoMovimientoInventario.Salida, 
                    "Preparación de producto"),
                Times.AtLeastOnce);
        }
    }
} 