namespace RestaurantePro.Domain.UnitTests.Integration.BetweenContexts.Operaciones_Inventario
{
    /// <summary>
    /// Tests de integración para verificar la actualización del inventario cuando se modifica una comanda.
    /// Demuestra la interacción entre los contextos de Operaciones (Comandas) e Inventario.
    /// </summary>
    public class ComandaModificada_ActualizacionInventarioTests
    {
        private readonly Mock<IComandaRepository> _comandaRepositoryMock = new();
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock = new();
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock = new();
        private readonly Mock<IDateTimeService> _dateTimeServiceMock = new();
        
        private readonly ComandaModificada_ActualizarInventarioHandler _handler;
        private readonly DateTime _fechaActual = new DateTime(2023, 5, 15, 10, 0, 0);
        
        public ComandaModificada_ActualizacionInventarioTests()
        {
            // Configurar fecha actual
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            // Inicializar handler
            _handler = new ComandaModificada_ActualizarInventarioHandler(
                _comandaRepositoryMock.Object,
                _ingredienteRepositoryMock.Object,
                _eventRegistryMock.Object,
                _dateTimeServiceMock.Object);
        }
        
        [Fact]
        public async Task ProductoAgregadoAComanda_DebeDecrementarStockIngredientes()
        {
            // Arrange
            // 1. Crear IDs para el test
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            
            // 2. Crear comanda
            var comanda = Comanda.Crear(meseroId, clienteId, mesaId);
            typeof(EntityBase).GetProperty("Id").SetValue(comanda, comandaId);
            
            // 3. Agregar un producto con ingredientes a la comanda
            comanda.AgregarProducto(productoId, 2, 150.0m);
            
            // 4. Crear ingredientes asociados al producto
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente1 = Ingrediente.Crear(
                "Tomate", 
                "TOM001",
                "Tomate rojo para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                5.0m, // Stock mínimo
                0.0m  // Stock inicial en 0
            );
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente1, ingrediente1Id);
            
            // Agregar un movimiento inicial para establecer el stock en 10.0
            ingrediente1.IncrementarStock(10.0m, "Stock inicial para test");
            
            var ingrediente2Id = Guid.NewGuid();
            var ingrediente2 = Ingrediente.Crear(
                "Lechuga", 
                "LEC001",
                "Lechuga fresca para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                3.0m, // Stock mínimo
                0.0m  // Stock inicial en 0
            );
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente2, ingrediente2Id);
            
            // Agregar un movimiento inicial para establecer el stock en 8.0
            ingrediente2.IncrementarStock(8.0m, "Stock inicial para test");
            
            // 5. Configurar relación producto-ingredientes
            // Normalmente esto estaría en otra parte del sistema, usamos un diccionario para simularlo
            var recetaProducto = new Dictionary<Guid, decimal>
            {
                { ingrediente1Id, 0.2m }, // 200g de tomate por unidad
                { ingrediente2Id, 0.1m }  // 100g de lechuga por unidad
            };
            
            // 6. Configurar mocks
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente1);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente2);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // 7. Simular servicio que obtiene la receta de ingredientes para un producto
            Mock<IRecetaService> recetaServiceMock = new Mock<IRecetaService>();
            recetaServiceMock
                .Setup(s => s.ObtenerIngredientesParaProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recetaProducto);
                
            // Inyectar el servicio de recetas al handler (esto debería hacerse en el constructor)
            typeof(ComandaModificada_ActualizarInventarioHandler)
                .GetField("_recetaService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_handler, recetaServiceMock.Object);
                
            // 8. Crear evento ProductoAgregadoAComanda
            var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda.ItemComandaCreado(
                comandaId, 
                Guid.NewGuid(), // itemId
                productoId, 
                "Ensalada mixta", 
                2); // Cantidad
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se consultó la comanda
            _comandaRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 2. Verificar que se consultaron los ingredientes
            _ingredienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(ingrediente1Id, It.IsAny<CancellationToken>()),
                Times.Once);
                
            _ingredienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(ingrediente2Id, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 3. Verificar que se actualizó el stock de los ingredientes
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<Ingrediente>(i => i.Id == ingrediente1Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<Ingrediente>(i => i.Id == ingrediente2Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 4. Verificar que se registró el evento
            _eventRegistryMock.Verify(
                r => r.RegisterAsync(It.IsAny<RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda.ItemComandaCreado>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ProductoEliminadoDeComanda_DebeIncrementarStockIngredientes()
        {
            // Arrange
            // 1. Crear IDs para el test
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var itemComandaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            
            // 2. Crear comanda con un producto
            var comanda = Comanda.Crear(meseroId, clienteId, mesaId);
            typeof(EntityBase).GetProperty("Id").SetValue(comanda, comandaId);
            
            // 3. Crear ingredientes con stock reducido (simulando que ya se había agregado el producto)
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente1 = Ingrediente.Crear(
                "Tomate", 
                "TOM001",
                "Tomate rojo para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                5.0m, // Stock mínimo
                0.0m  // Stock inicial en 0
            );
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente1, ingrediente1Id);
            
            // Agregar un movimiento inicial y luego simular que ya se había reducido el stock
            ingrediente1.IncrementarStock(10.0m, "Stock inicial para test");
            ingrediente1.DecrementarStock(0.4m, "Consumo previo simulado");
            
            var ingrediente2Id = Guid.NewGuid();
            var ingrediente2 = Ingrediente.Crear(
                "Lechuga", 
                "LEC001",
                "Lechuga fresca para ensalada",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                3.0m, // Stock mínimo
                0.0m  // Stock inicial en 0
            );
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente2, ingrediente2Id);
            
            // Agregar un movimiento inicial y luego simular que ya se había reducido el stock
            ingrediente2.IncrementarStock(8.0m, "Stock inicial para test");
            ingrediente2.DecrementarStock(0.2m, "Consumo previo simulado");
            
            // 4. Configurar relación producto-ingredientes
            var recetaProducto = new Dictionary<Guid, decimal>
            {
                { ingrediente1Id, 0.2m }, // 200g de tomate por unidad
                { ingrediente2Id, 0.1m }  // 100g de lechuga por unidad
            };
            
            // 5. Configurar mocks
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente1);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente2);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // 6. Simular servicio que obtiene la receta de ingredientes para un producto
            Mock<IRecetaService> recetaServiceMock = new Mock<IRecetaService>();
            recetaServiceMock
                .Setup(s => s.ObtenerIngredientesParaProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recetaProducto);
                
            // Inyectar el servicio de recetas al handler
            typeof(ComandaModificada_ActualizarInventarioHandler)
                .GetField("_recetaService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_handler, recetaServiceMock.Object);
                
            // 7. Crear evento ProductoEliminadoDeComanda
            var evento = new ProductoEliminadoDeComanda(
                comandaId,
                itemComandaId,
                productoId,
                "Ensalada mixta",
                2, // Cantidad
                "Cancelación por cliente"); // Motivo
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se actualizó el stock de los ingredientes (reembolso)
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<Ingrediente>(i => i.Id == ingrediente1Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<Ingrediente>(i => i.Id == ingrediente2Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 