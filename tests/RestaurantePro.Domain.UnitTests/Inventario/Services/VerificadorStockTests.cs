namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class VerificadorStockTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly IVerificadorStock _verificadorService;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public VerificadorStockTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();

            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));

            _verificadorService = new VerificadorStock(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object);
        }

        [Fact]
        public async Task VerificarStock_SinIngredientesBajoMinimo_NoDebeGenerarOrdenesCompra()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>();

            SetupObtenerIngredientesConStockBajo(ingredientes);

            // Act
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            result.OrdenesGeneradas.Should().BeEmpty();
        }

        [Fact]
        public async Task VerificarStock_ConIngredientesBajoMinimo_DebeGenerarOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente = CrearIngrediente(proveedor.Id);

            SetupObtenerIngredientesConStockBajo(new List<Ingrediente> { ingrediente });

            // Usamos IsMatcher en lugar de It.IsAny para evitar problemas de árboles de expresión
            SetupProveedorPorId(proveedor);

            // Act
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            result.OrdenesGeneradas.Should().HaveCount(1);
            result.OrdenesGeneradas.First().ProveedorId.Should().Be(proveedor.Id);
            result.OrdenesGeneradas.First().Items.Should().HaveCount(1);
            result.OrdenesGeneradas.First().Items.First().IngredienteId.Should().Be(ingrediente.Id);
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
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            result.OrdenesGeneradas.Should().HaveCount(1);
            result.OrdenesGeneradas.First().Items.Should().HaveCount(2);
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
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            result.OrdenesGeneradas.Should().HaveCount(2);
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
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            result.OrdenesGeneradas.Should().BeEmpty();
            result.Errores.Should().HaveCount(1);
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
            var result = await _verificadorService.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);

            // Assert
            result.OrdenesGeneradas.Should().BeEmpty();
            result.OrdenesActualizadas.Should().HaveCount(1);
        }

        // Métodos auxiliares para configurar mocks sin problemas de árboles de expresión
        private void SetupProveedorPorId(Proveedor proveedor)
        {
            // En vez de usar It.IsAny<> que causa problemas con árboles de expresión
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(g => true), It.Is<CancellationToken>(t => true)))
                .ReturnsAsync(proveedor);
        }

        private void SetupProveedorPorIdEspecifico(Guid proveedorId, Proveedor proveedor)
        {
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(g => g == proveedorId), It.Is<CancellationToken>(t => true)))
                .ReturnsAsync(proveedor);
        }

        private void SetupOrdenesPendientesPorProveedor(Guid proveedorId, List<OrdenCompra> ordenes)
        {
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(It.Is<Guid>(g => g == proveedorId), It.Is<CancellationToken>(t => true)))
                .ReturnsAsync(ordenes);
        }

        // Método auxiliar para evitar problemas de árboles de expresión
        private void SetupObtenerIngredientesConStockBajo(List<Ingrediente> ingredientes)
        {
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.Is<CancellationToken>(t => true)))
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
                proveedor.Desactivar();
            }

            return proveedor;
        }

        private Ingrediente CrearIngrediente(Guid proveedorId)
        {
            // Proporcionamos todos los parámetros de forma explícita para evitar usar argumentos opcionales
            var ingrediente = Ingrediente.Crear(
                nombre: "Ingrediente" + Guid.NewGuid().ToString().Substring(0, 8),
                codigo: "ING-" + Guid.NewGuid().ToString().Substring(0, 5),
                descripcion: "Descripción ingrediente",
                unidadMedida: RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                stockMinimo: 10.0m,
                stockActual: 0.0m,
                rotacion: RestaurantePro.Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Media,
                temporada: RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.TodoElAño);

            // Simulamos que este ingrediente está asociado al proveedor
            ingrediente.AsociarProveedorPrincipal(proveedorId);

            return ingrediente;
        }
    }
}


