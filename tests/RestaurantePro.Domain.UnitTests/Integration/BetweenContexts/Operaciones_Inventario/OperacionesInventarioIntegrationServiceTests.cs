using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace RestaurantePro.Domain.UnitTests.Integration.BetweenContexts.Operaciones_Inventario
{
    /// <summary>
    /// Pruebas unitarias para el servicio de integración entre Operaciones e Inventario
    /// </summary>
    public class OperacionesInventarioIntegrationServiceTests
    {
        private readonly Mock<RestaurantePro.Domain.Operaciones.Comandas.Interfaces.IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<RestaurantePro.Domain.Inventario.Ingredientes.Interfaces.IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<RestaurantePro.Domain.Core.Productos.Interfaces.IProductoIngredienteRepository> _productoIngredienteRepositoryMock;
        private readonly Mock<RestaurantePro.Domain.Core.Productos.Interfaces.IProductoRepository> _productoRepositoryMock;
        private readonly RestaurantePro.Domain.Core.SharedKernel.Validation.NotificationManager _notificationManager;
        private readonly Mock<RestaurantePro.Domain.Core.Base.Services.IDateTimeService> _dateTimeServiceMock;
        private readonly RestaurantePro.Domain.Operaciones.Services.OperacionesInventarioIntegrationService _sut;
        
        public OperacionesInventarioIntegrationServiceTests()
        {
            _comandaRepositoryMock = new Mock<RestaurantePro.Domain.Operaciones.Comandas.Interfaces.IComandaRepository>();
            _ingredienteRepositoryMock = new Mock<RestaurantePro.Domain.Inventario.Ingredientes.Interfaces.IIngredienteRepository>();
            _productoIngredienteRepositoryMock = new Mock<RestaurantePro.Domain.Core.Productos.Interfaces.IProductoIngredienteRepository>();
            _productoRepositoryMock = new Mock<RestaurantePro.Domain.Core.Productos.Interfaces.IProductoRepository>();
            _notificationManager = new RestaurantePro.Domain.Core.SharedKernel.Validation.NotificationManager();
            _dateTimeServiceMock = new Mock<RestaurantePro.Domain.Core.Base.Services.IDateTimeService>();
            
            _sut = new RestaurantePro.Domain.Operaciones.Services.OperacionesInventarioIntegrationService(
                _comandaRepositoryMock.Object,
                _ingredienteRepositoryMock.Object,
                _productoIngredienteRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _notificationManager,
                _dateTimeServiceMock.Object);
        }
        
        [Fact]
        public async Task VerificarDisponibilidadIngredientesComandaAsync_ComandaConStockSuficiente_DebeDevolverTodosDisponibles()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var ingredienteId1 = Guid.NewGuid();
            var ingredienteId2 = Guid.NewGuid();
            
            // Crear una comanda con un item
            var comanda = RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda.Crear(
                Guid.NewGuid(), // meseroId
                Guid.NewGuid(), // clienteId
                Guid.NewGuid()  // mesaId
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(comanda, comandaId);
            
            comanda.AgregarProducto(productoId, 2, 100m);
            
            // Crear el producto
            var categoriaId = Guid.NewGuid();
            var producto = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Ensalada",
                "Ensalada fresca",
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(100m),
                categoriaId, 
                "Ensaladas"
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(producto, productoId);
            
            // Crear los ingredientes con stock suficiente
            var ingrediente1 = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Tomate",
                "TOM001",
                "Tomate para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                1.0m, // Stock mínimo
                5.0m  // Stock inicial
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(ingrediente1, ingredienteId1);
            
            var ingrediente2 = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Lechuga",
                "LEC001",
                "Lechuga para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                0.5m, // Stock mínimo
                3.0m  // Stock inicial
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(ingrediente2, ingredienteId2);
            
            // Crear relaciones producto-ingrediente
            var productoIngrediente1 = new RestaurantePro.Domain.Core.Productos.Entities.ProductoIngrediente(
                productoId,
                ingredienteId1,
                0.2m, // 200g de tomate por ensalada
                false
            );
            
            var productoIngrediente2 = new RestaurantePro.Domain.Core.Productos.Entities.ProductoIngrediente(
                productoId,
                ingredienteId2,
                0.1m, // 100g de lechuga por ensalada
                false
            );
            
            // Configurar mocks
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerIngredientesPorProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente> { ingrediente1, ingrediente2 });
                
            _productoIngredienteRepositoryMock
                .Setup(r => r.ObtenerPorProductoEIngredienteAsync(productoId, ingredienteId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoIngrediente1);
                
            _productoIngredienteRepositoryMock
                .Setup(r => r.ObtenerPorProductoEIngredienteAsync(productoId, ingredienteId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoIngrediente2);
                
            // Act
            var resultado = await _sut.VerificarDisponibilidadIngredientesComandaAsync(comandaId);
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.TodosDisponibles.Should().BeTrue();
            resultado.Value.ProductosNoDisponibles.Should().BeEmpty();
            resultado.Value.IngredientesFaltantes.Should().BeEmpty();
        }
        
        [Fact]
        public async Task VerificarDisponibilidadIngredientesComandaAsync_StockInsuficiente_DebeDevolverProductosNoDisponibles()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var ingredienteId1 = Guid.NewGuid();
            var ingredienteId2 = Guid.NewGuid();
            
            // Crear una comanda con un item (2 unidades)
            var comanda = RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda.Crear(
                Guid.NewGuid(), // meseroId
                Guid.NewGuid(), // clienteId
                Guid.NewGuid()  // mesaId
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(comanda, comandaId);
            
            comanda.AgregarProducto(productoId, 5, 100m); // 5 unidades (más de lo que hay en stock)
            
            // Crear el producto
            var categoriaId = Guid.NewGuid();
            var producto = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Ensalada",
                "Ensalada fresca",
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(100m),
                categoriaId, 
                "Ensaladas"
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(producto, productoId);
            
            // Crear los ingredientes con stock insuficiente para 5 ensaladas
            var ingrediente1 = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Tomate",
                "TOM001",
                "Tomate para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                1.0m, // Stock mínimo
                0.5m  // Stock inicial (solo para 2.5 ensaladas)
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(ingrediente1, ingredienteId1);
            
            var ingrediente2 = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Lechuga",
                "LEC001",
                "Lechuga para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                0.5m, // Stock mínimo
                0.3m  // Stock inicial (solo para 3 ensaladas)
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(ingrediente2, ingredienteId2);
            
            // Crear relaciones producto-ingrediente
            var productoIngrediente1 = new RestaurantePro.Domain.Core.Productos.Entities.ProductoIngrediente(
                productoId,
                ingredienteId1,
                0.2m, // 200g de tomate por ensalada
                false
            );
            
            var productoIngrediente2 = new RestaurantePro.Domain.Core.Productos.Entities.ProductoIngrediente(
                productoId,
                ingredienteId2,
                0.1m, // 100g de lechuga por ensalada
                false
            );
            
            // Configurar mocks
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerIngredientesPorProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente> { ingrediente1, ingrediente2 });
                
            _productoIngredienteRepositoryMock
                .Setup(r => r.ObtenerPorProductoEIngredienteAsync(productoId, ingredienteId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoIngrediente1);
                
            _productoIngredienteRepositoryMock
                .Setup(r => r.ObtenerPorProductoEIngredienteAsync(productoId, ingredienteId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoIngrediente2);
                
            // Act
            var resultado = await _sut.VerificarDisponibilidadIngredientesComandaAsync(comandaId);
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.TodosDisponibles.Should().BeFalse();
            resultado.Value.ProductosNoDisponibles.Should().ContainKey(productoId);
            resultado.Value.IngredientesFaltantes.Should().ContainKey("Tomate");
            resultado.Value.IngredientesFaltantes.Should().ContainKey("Lechuga");
        }
        
        [Fact]
        public async Task ReservarIngredientesComandaAsync_StockSuficiente_DebeReservarCorrectamente()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var ingredienteId1 = Guid.NewGuid();
            var ingredienteId2 = Guid.NewGuid();
            
            // Crear una comanda con un item
            var comanda = RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda.Crear(
                Guid.NewGuid(), // meseroId
                Guid.NewGuid(), // clienteId
                Guid.NewGuid()  // mesaId
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(comanda, comandaId);
            
            comanda.AgregarProducto(productoId, 2, 100m);
            
            // Crear el producto
            var categoriaId = Guid.NewGuid();
            var producto = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Ensalada",
                "Ensalada fresca",
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(100m),
                categoriaId, 
                "Ensaladas"
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(producto, productoId);
            
            // Crear los ingredientes con stock suficiente
            var ingrediente1 = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Tomate",
                "TOM001",
                "Tomate para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                1.0m, // Stock mínimo
                5.0m  // Stock inicial
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(ingrediente1, ingredienteId1);
            
            var ingrediente2 = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Lechuga",
                "LEC001",
                "Lechuga para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                0.5m, // Stock mínimo
                3.0m  // Stock inicial
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(ingrediente2, ingredienteId2);
            
            // Crear relaciones producto-ingrediente
            var productoIngrediente1 = new RestaurantePro.Domain.Core.Productos.Entities.ProductoIngrediente(
                productoId,
                ingredienteId1,
                0.2m, // 200g de tomate por ensalada
                false
            );
            
            var productoIngrediente2 = new RestaurantePro.Domain.Core.Productos.Entities.ProductoIngrediente(
                productoId,
                ingredienteId2,
                0.1m, // 100g de lechuga por ensalada
                false
            );
            
            // Configurar mocks
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerIngredientesPorProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente> { ingrediente1, ingrediente2 });
                
            _productoIngredienteRepositoryMock
                .Setup(r => r.ObtenerPorProductoEIngredienteAsync(productoId, ingredienteId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoIngrediente1);
                
            _productoIngredienteRepositoryMock
                .Setup(r => r.ObtenerPorProductoEIngredienteAsync(productoId, ingredienteId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoIngrediente2);
                
            // Act
            var resultado = await _sut.ReservarIngredientesComandaAsync(comandaId);
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeTrue();
            
            // Verificar que se decrementó el stock
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente>(i => i.Id == ingredienteId1),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente>(i => i.Id == ingredienteId2),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task LiberarReservaIngredientesAsync_DebeIncrementarStock()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var ingredienteId1 = Guid.NewGuid();
            var ingredienteId2 = Guid.NewGuid();
            
            // Crear una comanda con un item
            var comanda = RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda.Crear(
                Guid.NewGuid(), // meseroId
                Guid.NewGuid(), // clienteId
                Guid.NewGuid()  // mesaId
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(comanda, comandaId);
            
            comanda.AgregarProducto(productoId, 2, 100m);
            
            // Crear el producto
            var categoriaId = Guid.NewGuid();
            var producto = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Ensalada",
                "Ensalada fresca",
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(100m),
                categoriaId, 
                "Ensaladas"
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(producto, productoId);
            
            // Crear los ingredientes
            var ingrediente1 = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Tomate",
                "TOM001",
                "Tomate para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                1.0m, // Stock mínimo
                4.6m  // Stock actual (ya se había decrementado)
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(ingrediente1, ingredienteId1);
            
            var ingrediente2 = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Lechuga",
                "LEC001",
                "Lechuga para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                0.5m, // Stock mínimo
                2.8m  // Stock actual (ya se había decrementado)
            );
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id").SetValue(ingrediente2, ingredienteId2);
            
            // Crear relaciones producto-ingrediente
            var productoIngrediente1 = new RestaurantePro.Domain.Core.Productos.Entities.ProductoIngrediente(
                productoId,
                ingredienteId1,
                0.2m, // 200g de tomate por ensalada
                false
            );
            
            var productoIngrediente2 = new RestaurantePro.Domain.Core.Productos.Entities.ProductoIngrediente(
                productoId,
                ingredienteId2,
                0.1m, // 100g de lechuga por ensalada
                false
            );
            
            // Configurar mocks
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerIngredientesPorProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente> { ingrediente1, ingrediente2 });
                
            _productoIngredienteRepositoryMock
                .Setup(r => r.ObtenerPorProductoEIngredienteAsync(productoId, ingredienteId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoIngrediente1);
                
            _productoIngredienteRepositoryMock
                .Setup(r => r.ObtenerPorProductoEIngredienteAsync(productoId, ingredienteId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoIngrediente2);
                
            // Act
            var resultado = await _sut.LiberarReservaIngredientesAsync(comandaId, "Cliente canceló pedido");
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeTrue();
            
            // Verificar que se incrementó el stock
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente>(i => i.Id == ingredienteId1),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente>(i => i.Id == ingredienteId2),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 