namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class GeneradorOrdenesCompraTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly GeneradorOrdenesCompra _generador;
        private readonly DateTime _fechaActual = new DateTime(2023, 10, 15);

        public GeneradorOrdenesCompraTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManagerMock = new Mock<INotificationManager>();
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            // Configurar el NotificationManager
            SetupNotificationManager();
            
            _generador = new GeneradorOrdenesCompra(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManagerMock.Object
            );
        }
        
        [Fact]
        public async Task GenerarOrdenesCompraAutomaticas_SinIngredientesConStockBajo_RetornaListaVacia()
        {
            // Arrange
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Ingrediente>());
                
            // Act
            var resultado = await _generador.GenerarOrdenesCompraAutomaticas();
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.Should().BeEmpty();
        }
        
        [Fact]
        public async Task GenerarOrdenCompraParaIngrediente_IngredienteInexistente_RetornaError()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ingrediente)null);
                
            // Act
            var resultado = await _generador.GenerarOrdenCompraParaIngrediente(ingredienteId);
            
            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
        }
        
        [Fact]
        public async Task GenerarOrdenCompraParaIngrediente_StockSuficiente_NoDebeGenerarOrden()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var ingrediente = CrearIngrediente(stockActual: 15, stockMinimo: 10, proveedorId: proveedorId);
            var proveedor = CrearProveedor(proveedorId);
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            // Act
            var resultado = await _generador.GenerarOrdenCompraParaIngrediente(ingredienteId);
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeNull(); // No se debe generar orden
        }
        
        [Fact]
        public async Task GenerarOrdenCompraParaIngrediente_ConStockBajo_DebeGenerarOrden()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var ingrediente = CrearIngrediente(stockActual: 5, stockMinimo: 10, proveedorId: proveedorId);
            var proveedor = CrearProveedor(proveedorId);
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            // Act
            var resultado = await _generador.GenerarOrdenCompraParaIngrediente(ingredienteId);
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull(); // Se debe haber generado una orden
        }
        
        private void SetupNotificationManager()
        {
            // Configuración simple sin métodos problemáticos
            _notificationManagerMock.Setup(m => m.HasErrors)
                .Returns(false);
        }
        
        // Métodos auxiliares para crear objetos de prueba
        private Ingrediente CrearIngrediente(decimal stockActual = 0, decimal stockMinimo = 10, Guid? proveedorId = null)
        {
            var ingrediente = Ingrediente.Crear(
                $"Ingrediente {Guid.NewGuid()}",
                $"ING-{Guid.NewGuid()}",
                "Descripción de prueba",
                Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                stockMinimo,
                stockActual,
                Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Media,
                Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.TodoElAño
            );
            
            if (proveedorId.HasValue)
            {
                ingrediente.AsociarProveedorPrincipal(proveedorId.Value);
            }
            
            return ingrediente;
        }
        
        private Proveedor CrearProveedor(Guid? id = null)
        {
            var proveedor = Proveedor.Crear(
                $"Proveedor {Guid.NewGuid()}",
                "Contacto Prueba",
                "contacto@proveedor.test",
                "123456789",
                "Dirección Prueba",
                "Ciudad",
                "12345",
                "País",
                "RFC123456789",
                "Cuenta 123456789",
                30 // Días de crédito
            );
            
            if (id.HasValue)
            {
                // Asignar ID mediante reflexión
                var idProperty = typeof(EntityBase).GetProperty("Id");
                if (idProperty != null)
                {
                    idProperty.SetValue(proveedor, id.Value);
                }
            }
            
            return proveedor;
        }
    }
}
