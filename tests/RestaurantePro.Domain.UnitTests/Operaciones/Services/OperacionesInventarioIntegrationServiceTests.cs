namespace RestaurantePro.Domain.UnitTests.Operaciones.Services
{
    public class OperacionesInventarioIntegrationServiceTests
    {
        private readonly Mock<RestaurantePro.Domain.Operaciones.Comandas.Interfaces.IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<RestaurantePro.Domain.Inventario.Ingredientes.Interfaces.IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces.IMovimientoInventarioRepository> _movimientoRepositoryMock;
        private readonly Mock<RestaurantePro.Domain.Core.Productos.Services.IRecetaService> _recetaServiceMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<ILogger<OperacionesInventarioIntegrationService>> _loggerMock;
        private readonly OperacionesInventarioIntegrationService _service;

        public OperacionesInventarioIntegrationServiceTests()
        {
            _comandaRepositoryMock = new Mock<RestaurantePro.Domain.Operaciones.Comandas.Interfaces.IComandaRepository>();
            _ingredienteRepositoryMock = new Mock<RestaurantePro.Domain.Inventario.Ingredientes.Interfaces.IIngredienteRepository>();
            _movimientoRepositoryMock = new Mock<RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces.IMovimientoInventarioRepository>();
            _recetaServiceMock = new Mock<RestaurantePro.Domain.Core.Productos.Services.IRecetaService>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _loggerMock = new Mock<ILogger<OperacionesInventarioIntegrationService>>();

            _service = new OperacionesInventarioIntegrationService(
                _comandaRepositoryMock.Object,
                _ingredienteRepositoryMock.Object,
                _movimientoRepositoryMock.Object,
                _recetaServiceMock.Object,
                _dateTimeServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesComandaAsync_ComandaNoExiste_RetornaError()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            _comandaRepositoryMock.Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda)null);

            // Act
            var resultado = await _service.VerificarDisponibilidadIngredientesComandaAsync(comandaId);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().Contain($"No se encontró la comanda con ID {comandaId}");
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesComandaAsync_TodosLosIngredientesDisponibles_RetornaExito()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            
            // Crear comanda real usando el método factory con mesa asignada
            var comanda = RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda.Crear(meseroId, null, mesaId, "Observaciones iniciales");
            
            // Para simular una comanda con items, usar reflection para establecer el ID
            var idProperty = typeof(RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda)
                .GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            idProperty?.SetValue(comanda, comandaId);
            
            // Agregar producto a la comanda
            comanda.AgregarProducto(productoId, 2, 100m, "Observaciones de prueba");
            
            _comandaRepositoryMock.Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            // Configurar que la receta tiene ingredientes disponibles
            _recetaServiceMock.Setup(s => s.VerificarDisponibilidadIngredientesAsync(
                    productoId, 
                    2, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(true));

            // Act
            var resultado = await _service.VerificarDisponibilidadIngredientesComandaAsync(comandaId);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.TodosDisponibles.Should().BeTrue();
            resultado.Value.IngredientesFaltantes.Should().BeEmpty();
            resultado.Value.ProductosNoDisponibles.Should().BeEmpty();
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesComandaAsync_IngredientesFaltantes_RetornaListaFaltantes()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            
            // Crear comanda real usando el método factory con mesa asignada
            var comanda = RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda.Crear(meseroId, null, mesaId, "Observaciones iniciales");
            
            // Para simular una comanda con items, usar reflection para establecer el ID
            var idProperty = typeof(RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda)
                .GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            idProperty?.SetValue(comanda, comandaId);
            
            // Agregar producto a la comanda
            comanda.AgregarProducto(productoId, 2, 100m, "Observaciones de prueba");
            
            _comandaRepositoryMock.Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            // Configurar que la receta NO tiene ingredientes disponibles
            _recetaServiceMock.Setup(s => s.VerificarDisponibilidadIngredientesAsync(
                    productoId, 
                    2, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(false));
                
            // Configurar los ingredientes faltantes
            var ingredientesFaltantes = new Dictionary<Guid, decimal>
            {
                { Guid.NewGuid(), 0.5m },
                { Guid.NewGuid(), 0.25m }
            };
            
            _recetaServiceMock.Setup(s => s.ObtenerIngredientesFaltantesAsync(
                    productoId, 
                    2, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(ingredientesFaltantes));
                
            // Configurar los ingredientes para obtener sus nombres
            foreach (var ingredienteId in ingredientesFaltantes.Keys)
            {
                var nombreIngrediente = ingredienteId == ingredientesFaltantes.Keys.First() ? "Tomate" : "Cebolla";
                var ingrediente = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                    nombreIngrediente,
                    "ING-" + ingredienteId.ToString().Substring(0, 5),
                    "Descripción del ingrediente",
                    RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                    1m, // stockMinimo
                    0.5m, // stockActual
                    RestaurantePro.Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Media,
                    RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.TodoElAño
                );
                
                _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingredienteId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(ingrediente);
            }

            // Act
            var resultado = await _service.VerificarDisponibilidadIngredientesComandaAsync(comandaId);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.TodosDisponibles.Should().BeFalse();
            resultado.Value.IngredientesFaltantes.Should().HaveCount(2);
            resultado.Value.IngredientesFaltantes.Should().ContainKey("Tomate");
            resultado.Value.ProductosNoDisponibles.Should().ContainKey(productoId);
        }

        [Fact]
        public async Task ReservarIngredientesComandaAsync_IngredientesDisponibles_ReservaExitosa()
        {
            // Arrange
            var now = DateTime.Now;
            _dateTimeServiceMock.Setup(d => d.Now).Returns(now);
            
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            
            // Crear comanda real usando el método factory con mesa asignada
            var comanda = RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda.Crear(meseroId, null, mesaId, "Observaciones iniciales");
            
            // Para simular una comanda con items, usar reflection para establecer el ID
            var idProperty = typeof(RestaurantePro.Domain.Operaciones.Comandas.Entities.Comanda)
                .GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            idProperty?.SetValue(comanda, comandaId);
            
            // Agregar producto a la comanda
            comanda.AgregarProducto(productoId, 2, 100m, "Observaciones de prueba");
            
            _comandaRepositoryMock.Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            // Configurar disponibilidad - El método ReservarIngredientesComandaAsync llama a VerificarDisponibilidadIngredientesAsync
            _recetaServiceMock.Setup(s => s.VerificarDisponibilidadIngredientesAsync(
                    productoId, 
                    2, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(true));
            
            // Configurar ingredientes para la receta
            var tomateId = Guid.NewGuid();
            var cebollaId = Guid.NewGuid();
            
            var ingredientesReceta = new Dictionary<Guid, decimal>
            {
                { tomateId, 0.5m },
                { cebollaId, 0.25m }
            };
            
            _recetaServiceMock.Setup(s => s.ObtenerIngredientesParaProductoAsync(
                    productoId, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(ingredientesReceta));
                
            // Configurar los ingredientes
            var tomate = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Tomate",
                "ING-TOM",
                "Tomate fresco",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                2m, // stockMinimo
                0m, // stockActual inicial en 0
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Alta,
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.TodoElAño
            );
            
            // Agregar stock inicial a través de un movimiento para mantener consistencia
            tomate.IncrementarStock(10m, "Stock inicial para pruebas");
            
            var cebolla = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Cebolla",
                "ING-CEB",
                "Cebolla blanca",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                1m, // stockMinimo
                0m, // stockActual inicial en 0
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Media,
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.TodoElAño
            );
            
            // Agregar stock inicial a través de un movimiento para mantener consistencia
            cebolla.IncrementarStock(5m, "Stock inicial para pruebas");
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(tomateId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tomate);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(cebollaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cebolla);
                
            // Configurar movimientos de inventario
            _movimientoRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.ReservarIngredientesComandaAsync(comandaId);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeTrue();
            
            // Verificar que se actualizaron los ingredientes
            _ingredienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
                
            // Verificar que se registraron los movimientos
            _movimientoRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
} 