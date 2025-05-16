namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class VerificadorStockTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        
        public VerificadorStockTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));
        }

        [Fact]
        public async Task VerificarStock_SinIngredientesBajoMinimo_NoDebeGenerarOrdenesCompra()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>();
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(ingredientes);
            
            var verificador = new VerificadorStock(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var result = await verificador.VerificarYGenerarOrdenesCompraAsync();

            // Assert
            result.OrdenesGeneradas.Should().BeEmpty();
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>(), default), Times.Never);
        }

        [Fact]
        public async Task VerificarStock_ConIngredientesBajoMinimo_DebeGenerarOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente = CrearIngrediente(proveedor.Id);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente> { ingrediente });
                
            _proveedorRepositoryMock.Setup(r => r.ObtenerPorIdAsync(proveedor.Id, default))
                .ReturnsAsync(proveedor);
                
            var verificador = new VerificadorStock(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var result = await verificador.VerificarYGenerarOrdenesCompraAsync();

            // Assert
            result.OrdenesGeneradas.Should().HaveCount(1);
            result.OrdenesGeneradas.First().ProveedorId.Should().Be(proveedor.Id);
            result.OrdenesGeneradas.First().Items.Should().HaveCount(1);
            result.OrdenesGeneradas.First().Items.First().IngredienteId.Should().Be(ingrediente.Id);
            
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>(), default), Times.Once);
        }

        [Fact]
        public async Task VerificarStock_VariosIngredientesMismoProveedor_DebeGenerarUnaOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente1 = CrearIngrediente(proveedor.Id);
            var ingrediente2 = CrearIngrediente(proveedor.Id);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente> { ingrediente1, ingrediente2 });
                
            _proveedorRepositoryMock.Setup(r => r.ObtenerPorIdAsync(proveedor.Id, default))
                .ReturnsAsync(proveedor);
                
            var verificador = new VerificadorStock(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var result = await verificador.VerificarYGenerarOrdenesCompraAsync();

            // Assert
            result.OrdenesGeneradas.Should().HaveCount(1);
            result.OrdenesGeneradas.First().Items.Should().HaveCount(2);
            
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>(), default), Times.Once);
        }

        [Fact]
        public async Task VerificarStock_IngredientesDistintosProveedores_DebeGenerarVariasOrdenesCompra()
        {
            // Arrange
            var proveedor1 = CrearProveedor();
            var proveedor2 = CrearProveedor();
            var ingrediente1 = CrearIngrediente(proveedor1.Id);
            var ingrediente2 = CrearIngrediente(proveedor2.Id);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente> { ingrediente1, ingrediente2 });
                
            _proveedorRepositoryMock.Setup(r => r.ObtenerPorIdAsync(proveedor1.Id, default))
                .ReturnsAsync(proveedor1);
            _proveedorRepositoryMock.Setup(r => r.ObtenerPorIdAsync(proveedor2.Id, default))
                .ReturnsAsync(proveedor2);
                
            var verificador = new VerificadorStock(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var result = await verificador.VerificarYGenerarOrdenesCompraAsync();

            // Assert
            result.OrdenesGeneradas.Should().HaveCount(2);
            
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>(), default), Times.Exactly(2));
        }

        [Fact]
        public async Task VerificarStock_ProveedorInactivo_NoDebeGenerarOrdenCompra()
        {
            // Arrange
            var proveedor = CrearProveedor(activo: false);
            var ingrediente = CrearIngrediente(proveedor.Id);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente> { ingrediente });
                
            _proveedorRepositoryMock.Setup(r => r.ObtenerPorIdAsync(proveedor.Id, default))
                .ReturnsAsync(proveedor);
                
            var verificador = new VerificadorStock(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var result = await verificador.VerificarYGenerarOrdenesCompraAsync();

            // Assert
            result.OrdenesGeneradas.Should().BeEmpty();
            result.Errores.Should().HaveCount(1);
            
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>(), default), Times.Never);
        }

        [Fact]
        public async Task VerificarStock_OrdenCompraExistente_NoDebeGenerarNuevaOrden()
        {
            // Arrange
            var proveedor = CrearProveedor();
            var ingrediente = CrearIngrediente(proveedor.Id);
            var fecha = new DateTime(2023, 1, 1);
            
            var ordenesExistentes = new List<OrdenCompra> { 
                OrdenCompra.Crear(proveedor.Id, "Orden automática", fecha)
            };
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente> { ingrediente });
                
            _proveedorRepositoryMock.Setup(r => r.ObtenerPorIdAsync(proveedor.Id, default))
                .ReturnsAsync(proveedor);
                
            _ordenCompraRepositoryMock.Setup(r => r.ObtenerPendientesPorProveedorAsync(proveedor.Id, default))
                .ReturnsAsync(ordenesExistentes);
                
            var verificador = new VerificadorStock(
                _ingredienteRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var result = await verificador.VerificarYGenerarOrdenesCompraAsync();

            // Assert
            result.OrdenesGeneradas.Should().BeEmpty();
            result.OrdenesActualizadas.Should().HaveCount(1);
            
            _ordenCompraRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrdenCompra>(), default), Times.Never);
            _ordenCompraRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<OrdenCompra>()), Times.Once);
        }

        // Métodos auxiliares para crear objetos de prueba
        private Proveedor CrearProveedor(bool activo = true)
        {
            return Proveedor.Crear("Proveedor Test", "Contacto Test", "contacto@test.com", 
                "123456789", "Dirección Test", "Ciudad Test", "12345", "País Test", 
                "RFC123456", "Cuenta: 123456789", 30, activo);
        }

        private Ingrediente CrearIngrediente(Guid proveedorId)
        {
            var ingrediente = Ingrediente.Crear(
                nombre: $"Ingrediente {Guid.NewGuid()}",
                codigo: $"ING-{Guid.NewGuid().ToString().Substring(0, 8)}",
                descripcion: "Descripción del ingrediente",
                unidadMedida: RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 
                stockMinimo: 10,
                stockActual: 5);
                
            // Simulamos que este ingrediente está asociado al proveedor
            ingrediente.AsociarProveedorPrincipal(proveedorId);
            
            return ingrediente;
        }
    }
} 