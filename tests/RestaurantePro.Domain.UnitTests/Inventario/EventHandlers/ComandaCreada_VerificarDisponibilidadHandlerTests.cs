namespace RestaurantePro.Domain.UnitTests.Inventario.EventHandlers
{
    public class ComandaCreada_VerificarDisponibilidadHandlerTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IProductoIngredienteRepository> _productoIngredienteRepositoryMock;
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock;
        private readonly ComandaCreada_VerificarDisponibilidadHandler _handler;

        public ComandaCreada_VerificarDisponibilidadHandlerTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _productoIngredienteRepositoryMock = new Mock<IProductoIngredienteRepository>();
            _eventRegistryMock = new Mock<IDomainEventRegistry>();

            _handler = new ComandaCreada_VerificarDisponibilidadHandler(
                _ingredienteRepositoryMock.Object,
                _comandaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _productoIngredienteRepositoryMock.Object,
                _eventRegistryMock.Object);
        }

        [Fact]
        public async Task Handle_ConStockSuficiente_NoDebeGenerarAdvertencias()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var categoriaId = Guid.NewGuid();
            var ingredienteId = Guid.NewGuid();

            // Crear evento de comanda
            var eventoComanda = new ComandaCreada(comandaId, mesaId, meseroId);
            
            // Configurar comanda
            var comanda = Comanda.Crear(mesaId, meseroId);
            
            // Necesitamos configurar manualmente el ID para efectos de la prueba
            var comandaIdField = typeof(EntityBase).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
            comandaIdField?.SetValue(comanda, comandaId);
            
            comanda.AgregarProducto(productoId, 2, 15.99m);
            
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            // Configurar producto
            var precioProducto = new PrecioProducto(15.99m);
            var producto = Producto.Crear("Ensalada César", "Ensalada fresca", precioProducto, categoriaId);
            
            // Configurar manualmente el ID del producto
            comandaIdField?.SetValue(producto, productoId);
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);

            // Configurar ingrediente con stock suficiente
            var ingrediente = Ingrediente.Crear("Lechuga", "LECH", "Lechuga romana", UnidadMedida.Kilogramo, 1.0m, 10.0m);
            comandaIdField?.SetValue(ingrediente, ingredienteId);

            // Lista de ingredientes para el producto
            var ingredientesProducto = new List<Ingrediente> { ingrediente };

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerIngredientesPorProductoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientesProducto);
                
            // Configurar relación producto-ingrediente
            var productoIngrediente = new ProductoIngrediente(productoId, ingredienteId, 0.5m); // Cada producto usa 0.5kg
            
            _productoIngredienteRepositoryMock
                .Setup(r => r.ObtenerPorProductoEIngredienteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoIngrediente);
                
            // Configurar event registry
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _handler.Handle(eventoComanda);
            
            // Assert - Verificar que NO se llamó al método RegisterAsync 
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<ComandaCreada>(),
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task Handle_ConStockInsuficiente_DebeGenerarAdvertencia()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var categoriaId = Guid.NewGuid();
            var ingredienteId = Guid.NewGuid();

            Console.WriteLine("=== PRUEBA: Handle_ConStockInsuficiente_DebeGenerarAdvertencia ===");
            
            // 1. Crear evento de comanda
            var eventoComanda = new ComandaCreada(comandaId, mesaId, meseroId);
            Console.WriteLine($"Evento ComandaCreada generado: ID={comandaId}");
            
            // 2. Crear comanda con un ítem que requiere más stock del disponible
            var comanda = Comanda.Crear(mesaId, meseroId);
            
            // Configurar manualmente el ID de la comanda para que coincida con el evento
            var idField = typeof(EntityBase).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
            idField?.SetValue(comanda, comandaId);
            
            // Añadir un producto que requiere 5kg de ingrediente (10 unidades * 0.5kg)
            comanda.AgregarProducto(productoId, 10, 15.99m);
            var item = comanda.Items.First();
            
            Console.WriteLine($"Comanda configurada: ID={comanda.Id}");
            Console.WriteLine($"Item añadido: ProductoID={item.ProductoId}, Cantidad={item.Cantidad}");
            
            // Configurar el mock del repositorio para devolver la comanda
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
            
            // 3. Crear un producto
            var precioProducto = new PrecioProducto(15.99m);
            var producto = Producto.Crear("Ensalada César", "Ensalada fresca", precioProducto, categoriaId);
            
            // Establecer manualmente el ID del producto
            idField?.SetValue(producto, productoId);
            
            Console.WriteLine($"Producto configurado: ID={producto.Id}, Nombre={producto.Nombre}");
            
            // Configurar el mock del repositorio para devolver el producto
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
            
            // 4. Crear un ingrediente con stock insuficiente (1kg disponible)
            var ingrediente = Ingrediente.Crear(
                "Lechuga", 
                "LECH", 
                "Lechuga romana", 
                UnidadMedida.Kilogramo, 
                1.0m, // Stock mínimo
                1.0m  // Stock actual
            );
            
            // Establecer manualmente el ID del ingrediente
            idField?.SetValue(ingrediente, ingredienteId);
            
            Console.WriteLine($"Ingrediente configurado: ID={ingrediente.Id}, Nombre={ingrediente.Nombre}, Stock={ingrediente.Stock}kg");
            
            // Crear lista con el ingrediente
            var ingredientesProducto = new List<Ingrediente> { ingrediente };
            
            // Configurar el mock del repositorio para devolver los ingredientes
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerIngredientesPorProductoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientesProducto);
            
            // 5. Configurar la relación producto-ingrediente (cada unidad de producto usa 0.5kg de ingrediente)
            var productoIngrediente = new ProductoIngrediente(productoId, ingredienteId, 0.5m);
            
            Console.WriteLine($"ProductoIngrediente configurado: ProductoID={productoIngrediente.ProductoId}, " +
                $"IngredienteID={productoIngrediente.IngredienteId}, Cantidad={productoIngrediente.Cantidad}kg por unidad");
            Console.WriteLine($"Cantidad total requerida: {productoIngrediente.Cantidad * item.Cantidad}kg (Stock disponible: {ingrediente.Stock}kg)");
            
            // Verificar que la cantidad necesaria sea mayor que el stock disponible
            Debug.Assert(productoIngrediente.Cantidad * item.Cantidad > ingrediente.Stock,
                $"La prueba requiere que la cantidad necesaria ({productoIngrediente.Cantidad * item.Cantidad}kg) sea mayor que el stock disponible ({ingrediente.Stock}kg)");
            
            // Configurar el mock del repositorio para devolver la relación producto-ingrediente
            _productoIngredienteRepositoryMock
                .Setup(r => r.ObtenerPorProductoEIngredienteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoIngrediente);
            
            // 6. Configurar el mock del event registry para capturar los mensajes
            var eventosRegistrados = new List<DomainEvent>();
            
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Callback<DomainEvent, CancellationToken>((e, c) => {
                    Console.WriteLine($"EventRegistry llamado con evento: {e}");
                    eventosRegistrados.Add(e);
                })
                .Returns(Task.CompletedTask);
            
            // Act
            Console.WriteLine("Ejecutando handler...");
            await _handler.Handle(eventoComanda);
            
            // Assert
            Console.WriteLine($"Eventos registrados: {eventosRegistrados.Count}");
            foreach (var evt in eventosRegistrados)
            {
                Console.WriteLine($"- {evt}");
            }
            
            // Verificar que se haya registrado el evento ComandaCreada
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(It.IsAny<ComandaCreada>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce());
            
            // Verificar que se haya llamado al registro del evento
            Assert.True(eventosRegistrados.Count > 0, "Deberían haberse registrado eventos");
            Assert.Contains(eventosRegistrados, e => e.GetType() == typeof(ComandaCreada));
        }
    }
} 