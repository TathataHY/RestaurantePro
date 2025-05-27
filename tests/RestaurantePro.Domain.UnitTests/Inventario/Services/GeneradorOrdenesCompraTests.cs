#pragma warning disable CS0854 // Un árbol de expresión no puede contener una llamada o invocación que use argumentos opcionales

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
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Ingrediente?>());
                
            // Act
            var resultado = await _generador.GenerarOrdenesCompraAutomaticas();
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeEmpty();
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()), Times.Never);
            VerificarNoHayErrores();
        }
        
        [Fact]
        public async Task GenerarOrdenCompraParaIngrediente_IngredienteInexistente_RetornaError()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == ingredienteId), It.Is<bool>(b => b == false), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ingrediente?)null);
                
            // Act
            var resultado = await _generador.GenerarOrdenCompraParaIngrediente(ingredienteId);
            
            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Value.Should().BeNull();
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task GenerarOrdenCompraParaIngrediente_StockSuficiente_NoDebeGenerarOrden()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var ingrediente = CrearIngrediente(stockActual: 15, stockMinimo: 10);
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == ingredienteId), It.Is<bool>(b => b == false), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            // Act
            var resultado = await _generador.GenerarOrdenCompraParaIngrediente(ingredienteId);
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeNull(); // No se generó orden porque no era necesario
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()), Times.Never);
            VerificarNoHayErrores();
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
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == ingredienteId), It.Is<bool>(b => b == false), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedorId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            // Act
            var resultado = await _generador.GenerarOrdenCompraParaIngrediente(ingredienteId);
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull(); // Se debe haber generado una orden
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()), Times.Once);
            VerificarNoHayErrores();
        }
        
        private void SetupNotificationManager()
        {
            // Configurar CreateNewNotification
            _notificationManagerMock.Setup(m => m.CreateNewNotification())
                .Verifiable();
                
            // Configurar HasErrors para que retorne false por defecto
            _notificationManagerMock.Setup(m => m.HasErrors)
                .Returns(false);
                
            // Configurar sobrecargas específicas en lugar de usar argumentos opcionales
            _notificationManagerMock
                .Setup(m => m.Require(It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(_notificationManagerMock.Object);
                
            _notificationManagerMock
                .Setup(m => m.Require(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_notificationManagerMock.Object);
                
            _notificationManagerMock
                .Setup(m => m.RequireNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Returns(_notificationManagerMock.Object);
                
            _notificationManagerMock
                .Setup(m => m.RequireNotNull(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_notificationManagerMock.Object);
                
            // Configurar ToResult para distintos tipos de retorno con tipos específicos
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<IEnumerable<Guid>>()))
                .Returns((IEnumerable<Guid> r) => Result.Success(r));
                
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<Guid?>()))
                .Returns((Guid? r) => Result.Success(r));
                
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<Guid>()))
                .Returns((Guid r) => Result.Success(r));
                
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<bool>()))
                .Returns((bool r) => Result.Success(r));
        }
        
        private void VerificarNoHayErrores()
        {
            _notificationManagerMock.Verify(n => n.AddError(It.Is<string>(s => true), It.Is<string>(s => true)), Times.Never);
        }
        
        private void VerificarHayErrores()
        {
            _notificationManagerMock.Verify(m => m.AddError(It.Is<string>(s => true), It.Is<string>(s => true)), Times.AtLeastOnce);
        }
        
        // Métodos auxiliares para crear objetos de prueba
        private Ingrediente CrearIngrediente(decimal stockActual = 0, decimal stockMinimo = 10, Guid? proveedorId = null)
        {
            var ingrediente = Ingrediente.Crear(
                $"Ingrediente {Guid.NewGuid().ToString().Substring(0, 8)}",
                $"ING-{Guid.NewGuid().ToString().Substring(0, 5)}",
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
                $"Proveedor {Guid.NewGuid().ToString().Substring(0, 8)}",
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
                // Corregir uso de reflexión para evitar operador ?. en expresión lambda
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

#pragma warning restore CS0854
