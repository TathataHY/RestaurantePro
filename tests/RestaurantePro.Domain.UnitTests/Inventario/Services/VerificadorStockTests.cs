namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
#pragma warning disable CS0854 // Un árbol de expresión no puede contener una llamada o invocación que use argumentos opcionales

    public class VerificadorStockTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly IVerificadorStock _verificadorService;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public VerificadorStockTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManagerMock = new Mock<INotificationManager>();

            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));
            
            // Configurar el mock del NotificationManager
            SetupNotificationManager();

            _verificadorService = new VerificadorStock(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManagerMock.Object);
        }

        [Fact]
        public async Task VerificarStock_SinIngredientesBajoMinimo_NoDebeGenerarOrdenesCompra()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>();

            SetupObtenerIngredientesConStockBajo(ingredientes);

            // Act
            var resultado = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.OrdenesGeneradas.Should().BeEmpty();
            VerificarNoHayErrores();
        }

        [Fact]
        public async Task VerificarStock_ConIngredientesBajoMinimo_DebeGenerarOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente = CrearIngrediente(proveedor.Id);

            SetupObtenerIngredientesConStockBajo(new List<Ingrediente> { ingrediente });

            // Usamos IsMatcher en lugar de It.IsAny para evitar problemas de árboles de expresión
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);

            // Act
            var resultado = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.OrdenesGeneradas.Should().HaveCount(1);
            resultado.Value.OrdenesGeneradas.First().ProveedorId.Should().Be(proveedor.Id);
            resultado.Value.OrdenesGeneradas.First().Items.Should().HaveCount(1);
            resultado.Value.OrdenesGeneradas.First().Items.First().IngredienteId.Should().Be(ingrediente.Id);
            VerificarNoHayErrores();
        }

        [Fact]
        public async Task VerificarStock_VariosIngredientesMismoProveedor_DebeGenerarUnaOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente1 = CrearIngrediente(proveedor.Id);
            var ingrediente2 = CrearIngrediente(proveedor.Id);

            SetupObtenerIngredientesConStockBajo(new List<Ingrediente> { ingrediente1, ingrediente2 });

            SetupProveedorPorId(proveedor);

            // Act
            var resultado = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.OrdenesGeneradas.Should().HaveCount(1);
            resultado.Value.OrdenesGeneradas.First().Items.Should().HaveCount(2);
            VerificarNoHayErrores();
        }

        [Fact]
        public async Task VerificarStock_IngredientesDistintosProveedores_DebeGenerarVariasOrdenesCompra()
        {
            // Arrange
            var proveedor1 = CrearProveedor();
            var proveedor2 = CrearProveedor();
            var ingrediente1 = CrearIngrediente(proveedor1.Id);
            var ingrediente2 = CrearIngrediente(proveedor2.Id);

            SetupObtenerIngredientesConStockBajo(new List<Ingrediente> { ingrediente1, ingrediente2 });

            // Configurar para devolver el proveedor correcto según el ID
            SetupProveedorPorIdEspecifico(proveedor1.Id, proveedor1);
            SetupProveedorPorIdEspecifico(proveedor2.Id, proveedor2);

            // Act
            var resultado = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.OrdenesGeneradas.Should().HaveCount(2);
            VerificarNoHayErrores();
        }

        [Fact]
        public async Task VerificarStock_ProveedorInactivo_NoDebeGenerarOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor(activo: false);
            var ingrediente = CrearIngrediente(proveedor.Id);

            SetupObtenerIngredientesConStockBajo(new List<Ingrediente> { ingrediente });

            SetupProveedorPorId(proveedor);

            // Act
            var resultado = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Value.OrdenesGeneradas.Should().BeEmpty();
            resultado.Value.Errores.Should().HaveCount(1);
            VerificarHayErrores();
        }

        [Fact]
        public async Task VerificarStock_OrdenCompraExistente_NoDebeGenerarNuevaOrden()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente = CrearIngrediente(proveedor.Id);
            var fecha = new DateTime(2023, 1, 1);

            // Crear una orden existente
            var ordenExistente = OrdenCompra.Crear(proveedor.Id, "Orden automática", fecha);
            
            // Establecer fecha de entrega estimada posterior a la fecha de emisión
            ordenExistente.EstablecerFechaEntrega(fecha.AddDays(7));

            var ordenesExistentes = new List<OrdenCompra> { ordenExistente };

            // Agregar el ingrediente al stock en la inicialización para evitar problemas de validación
            ingrediente.IncrementarStock(5m, "Stock inicial");

            SetupObtenerIngredientesConStockBajo(new List<Ingrediente> { ingrediente });

            SetupProveedorPorId(proveedor);

            SetupOrdenesPendientesPorProveedor(proveedor.Id, ordenesExistentes);

            // Act
            var resultado = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.OrdenesGeneradas.Should().BeEmpty();
            resultado.Value.OrdenesActualizadas.Should().HaveCount(1);
            VerificarNoHayErrores();
        }

        // Métodos auxiliares para configurar mocks sin problemas de árboles de expresión
        private void SetupNotificationManager()
        {
            // Configurar CreateNewNotification para que simplemente retorne
            _notificationManagerMock.Setup(m => m.CreateNewNotification())
                .Verifiable();
                
            // Configurar HasErrors para que retorne false por defecto
            _notificationManagerMock.Setup(m => m.HasErrors)
                .Returns(false);
                
            // Configurar RequireNotNull para evitar argumentos opcionales
            _notificationManagerMock
                .Setup(m => m.RequireNotNull(It.IsAny<object>(), It.IsAny<string>()))
                .Returns(_notificationManagerMock.Object);
                
            _notificationManagerMock
                .Setup(m => m.RequireNotNull(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_notificationManagerMock.Object);
                
            // Configurar ToResult para diferentes tipos
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<ResultadoVerificacionStock>()))
                .Returns<ResultadoVerificacionStock>(r => Result.Success(r));
                
            _notificationManagerMock
                .Setup(m => m.ToResult(It.IsAny<bool>()))
                .Returns<bool>(b => Result.Success(b));
        }
        
        private void VerificarNoHayErrores()
        {
            _notificationManagerMock.Verify(m => m.HasErrors, Times.AtLeastOnce);
            _notificationManagerMock.Verify(m => m.AddError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        
        private void VerificarHayErrores()
        {
            _notificationManagerMock.Verify(m => m.AddError(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        }

        private void SetupProveedorPorId(Proveedor proveedor)
        {
            // Usar Callback en lugar de usar directamente It.IsAny para el token de cancelación
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(proveedor);
        }

        private void SetupProveedorPorIdEspecifico(Guid proveedorId, Proveedor proveedor)
        {
            // Usar Callback en lugar de usar directamente It.IsAny para el token de cancelación
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(g => g == proveedorId), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(proveedor);
        }

        private void SetupOrdenesPendientesPorProveedor(Guid proveedorId, List<OrdenCompra> ordenes)
        {
            // Usar Callback en lugar de usar directamente It.IsAny para el token de cancelación
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(It.Is<Guid>(g => g == proveedorId), It.IsAny<CancellationToken>()))
                .Callback(() => { /* No hacer nada */ })
                .ReturnsAsync(ordenes);
        }

        // Método auxiliar para evitar problemas de árboles de expresión
        private void SetupObtenerIngredientesConStockBajo(List<Ingrediente> ingredientes)
        {
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientes ?? new List<Ingrediente>());
        }

        // Métodos auxiliares para crear objetos de prueba
        private Proveedor CrearProveedor(bool activo = true)
        {
            // Crear con un mock para que se pueda usar en las pruebas
            Proveedor proveedor = Proveedor.Crear(
                "Proveedor Test",
                "Contacto Test",
                "contacto@test.com",
                "123456789",
                "Dirección Test",
                "Ciudad Test",
                "12345",
                "País Test",
                "RFC12345678901", // RFC con más de 10 caracteres
                "Cuenta: 123456789",
                30);

            if (!activo)
            {
                proveedor.Desactivar("Proveedor inactivo para pruebas");
            }

            return proveedor;
        }

        private Ingrediente CrearIngrediente(Guid proveedorId)
        {
            // Usamos variables locales en lugar de argumentos nombrados
            string nombre = "Ingrediente" + Guid.NewGuid().ToString().Substring(0, 8);
            string codigo = "ING-" + Guid.NewGuid().ToString().Substring(0, 5);
            string descripcion = "Descripción ingrediente";
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            decimal stockMinimo = 10.0m;
            decimal stockActual = 0.0m;
            var rotacion = RestaurantePro.Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Media;
            var temporada = RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.TodoElAño;

            // Crear ingrediente sin usar argumentos nombrados
            var ingrediente = Ingrediente.Crear(
                nombre,
                codigo,
                descripcion,
                unidadMedida,
                stockMinimo,
                stockActual,
                rotacion,
                temporada);

            // Simulamos que este ingrediente está asociado al proveedor
            ingrediente.AsociarProveedorPrincipal(proveedorId);

            return ingrediente;
        }
    }

#pragma warning restore CS0854
}


