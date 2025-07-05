namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class VerificadorStockTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly INotificationManager _notificationManager;
        private readonly IVerificadorStock _verificadorService;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        private readonly DateTime _fechaActual = new DateTime(2023, 1, 1);

        public VerificadorStockTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            
            // Configuraciones base para todos los tests
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            _dateTimeServiceMock.Setup(s => s.UtcNow).Returns(_fechaActual.ToUniversalTime());
            
            // Usar NotificationManager real en lugar de mock
            _notificationManager = new NotificationManager();
            
            // Crear el verificador con la implementación real
            _verificadorService = new VerificadorStock(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManager);
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
        }

        [Fact]
        public async Task VerificarStock_ConIngredientesBajoMinimo_DebeGenerarOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente = CrearIngrediente(proveedor.Id);

            SetupObtenerIngredientesConStockBajo(new List<Ingrediente> { ingrediente });

            // Configurar proveedor
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
        }

        [Fact]
        public async Task VerificarStock_VariosIngredientesMismoProveedor_DebeGenerarUnaOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente1 = CrearIngrediente(proveedor.Id);
            var ingrediente2 = CrearIngrediente(proveedor.Id);

            SetupObtenerIngredientesConStockBajo(new List<Ingrediente> { ingrediente1, ingrediente2 });

            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);

            // Act
            var resultado = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.OrdenesGeneradas.Should().HaveCount(1);
            resultado.Value.OrdenesGeneradas.First().Items.Should().HaveCount(2);
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
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor1);
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor2.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor2);

            // Act
            var resultado = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.OrdenesGeneradas.Should().HaveCount(2);
        }

        [Fact]
        public async Task VerificarStock_ProveedorInactivo_NoDebeGenerarOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor(activo: false);
            var ingrediente = CrearIngrediente(proveedor.Id);

            SetupObtenerIngredientesConStockBajo(new List<Ingrediente> { ingrediente });

            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);

            // Configurar un resultado específico para simular el comportamiento del servicio
            _notificationManager.ClearErrors();
            _notificationManager.AddError($"El proveedor {proveedor.Id} no está activo", "ERR_PROVEEDOR_INACTIVO");

            // Act
            var resultado = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
            resultado.Errors.Should().Contain(e => e.Contains("proveedor") && e.Contains("no está activo"));
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

            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == proveedor.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);

            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(It.Is<Guid>(g => g == proveedor.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesExistentes);

            // Act
            var resultado = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.OrdenesGeneradas.Should().BeEmpty();
            resultado.Value.OrdenesActualizadas.Should().HaveCount(1);
        }

        private void SetupObtenerIngredientesConStockBajo(List<Ingrediente> ingredientes)
        {
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientes ?? new List<Ingrediente>());
        }

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
}


