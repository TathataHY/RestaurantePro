namespace RestaurantePro.Domain.UnitTests.Inventario.Compras.OrdenesCompra.Repositories
{
    public class OrdenCompraRepositoryTests
    {
        private readonly Mock<IOrdenCompraRepository> _mockRepository;
        private readonly List<OrdenCompra> _ordenesCompra;

        public OrdenCompraRepositoryTests()
        {
            _mockRepository = new Mock<IOrdenCompraRepository>();

            // Crear datos de prueba
            var fechaHoy = DateTime.Now;
            
            _ordenesCompra = new List<OrdenCompra>
            {
                OrdenCompra.Crear(Guid.NewGuid(), "Orden 1", fechaHoy),
                OrdenCompra.Crear(Guid.NewGuid(), "Orden 2", fechaHoy),
                OrdenCompra.Crear(Guid.NewGuid(), "Orden 3", fechaHoy)
            };
            
            // Agregar algunos items a las órdenes
            _ordenesCompra[0].AgregarItem(Guid.NewGuid(), "Ingrediente 1", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            _ordenesCompra[0].AgregarItem(Guid.NewGuid(), "Ingrediente 2", 3.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            _ordenesCompra[1].AgregarItem(Guid.NewGuid(), "Ingrediente 3", 5.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            // Cambiar estados de algunas órdenes
            _ordenesCompra[0].Enviar(); // Enviada
            _ordenesCompra[2].Cancelar("Proveedor no disponible"); // Cancelada
        }

        [Fact]
        public async Task ObtenerPorIdAsync_IdExistente_DebeRetornarOrdenCompra()
        {
            // Arrange
            var ordenId = _ordenesCompra[0].Id;
            var ordenEsperada = _ordenesCompra[0];

            _mockRepository.Setup(repo => repo.ObtenerPorIdAsync(ordenId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenEsperada);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorIdAsync(ordenId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(ordenEsperada);
            _mockRepository.Verify(repo => repo.ObtenerPorIdAsync(ordenId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ObtenerTodasAsync_DebeRetornarTodasLasOrdenes()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.ObtenerTodasAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_ordenesCompra);

            // Act
            var resultado = await _mockRepository.Object.ObtenerTodasAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(3);
            resultado.Should().BeEquivalentTo(_ordenesCompra);
        }

        [Fact]
        public async Task ObtenerPorEstadoAsync_EstadoEnviada_DebeRetornarOrdenesEnviadas()
        {
            // Arrange
            var ordenesEnviadas = _ordenesCompra.Where(o => o.Estado == EstadoOrdenCompra.Enviada).ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorEstadoAsync(EstadoOrdenCompra.Enviada, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesEnviadas);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorEstadoAsync(EstadoOrdenCompra.Enviada);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1); // Solo hay 1 orden enviada (la primera)
            resultado.All(o => o.Estado == EstadoOrdenCompra.Enviada).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPorProveedorAsync_ProveedorExistente_DebeRetornarOrdenesDelProveedor()
        {
            // Arrange
            var proveedorId = _ordenesCompra[1].ProveedorId;
            var ordenesDelProveedor = _ordenesCompra.Where(o => o.ProveedorId == proveedorId).ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorProveedorAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesDelProveedor);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorProveedorAsync(proveedorId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1); // Solo hay 1 orden del proveedor (la segunda)
            resultado.All(o => o.ProveedorId == proveedorId).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPorIngredienteAsync_IngredienteExistente_DebeRetornarOrdenesConIngrediente()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            
            // Añadimos un item con el ingrediente específico a la primera orden
            _ordenesCompra[0].AgregarItem(ingredienteId, "Ingrediente Específico", 5.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            var ordenesConIngrediente = _ordenesCompra
                .Where(o => o.Items.Any(i => i.IngredienteId == ingredienteId))
                .ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorIngredienteAsync(ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesConIngrediente);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorIngredienteAsync(ingredienteId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            resultado.First().Items
                .Any(i => i.IngredienteId == ingredienteId)
                .Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPorRangoFechasAsync_FechasValidas_DebeRetornarOrdenesDentroDelRango()
        {
            // Arrange
            var fechaHoy = DateTime.Now;
            var fechaInicio = fechaHoy.AddDays(-1);
            var fechaFin = fechaHoy.AddDays(1);
            
            var ordenesDentroDelRango = _ordenesCompra
                .Where(o => o.FechaCreacion >= fechaInicio && o.FechaCreacion <= fechaFin)
                .ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesDentroDelRango);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(3); // Todas las órdenes deberían estar en este rango
            resultado.All(o => o.FechaCreacion >= fechaInicio && o.FechaCreacion <= fechaFin)
                .Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPendientesPorProveedorAsync_ProveedorConOrdenesPendientes_DebeRetornarOrdenesPendientes()
        {
            // Arrange
            var proveedorId = _ordenesCompra[1].ProveedorId;
            
            // Asegurarnos de que hay al menos una orden pendiente para este proveedor
            _ordenesCompra[1].Estado.Should().Be(EstadoOrdenCompra.Pendiente); // Verificamos que la orden 1 está pendiente
            
            var ordenesPendientesDelProveedor = _ordenesCompra
                .Where(o => o.ProveedorId == proveedorId && o.Estado == EstadoOrdenCompra.Pendiente)
                .ToList();

            _mockRepository.Setup(repo => repo.ObtenerPendientesPorProveedorAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesPendientesDelProveedor);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPendientesPorProveedorAsync(proveedorId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            resultado.All(o => o.ProveedorId == proveedorId && o.Estado == EstadoOrdenCompra.Pendiente)
                .Should().BeTrue();
        }

        [Fact]
        public async Task AgregarAsync_OrdenValida_DebeAgregarCorrectamente()
        {
            // Arrange
            var nuevaOrden = OrdenCompra.Crear(
                Guid.NewGuid(),
                "Nueva orden",
                DateTime.Now);

            _mockRepository.Setup(repo => repo.AgregarAsync(nuevaOrden, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(repo => repo.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _mockRepository.Object.AgregarAsync(nuevaOrden);
            var resultadoGuardado = await _mockRepository.Object.GuardarCambiosAsync();

            // Assert
            _mockRepository.Verify(repo => repo.AgregarAsync(nuevaOrden, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
            resultadoGuardado.Should().Be(1);
        }

        [Fact]
        public async Task ActualizarAsync_OrdenExistente_DebeActualizarCorrectamente()
        {
            // Arrange
            var orden = _ordenesCompra[1]; // Orden pendiente
            orden.AgregarItem(Guid.NewGuid(), "Nuevo Ingrediente", 2.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo); // Agregamos un item

            _mockRepository.Setup(repo => repo.ActualizarAsync(orden, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(repo => repo.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _mockRepository.Object.ActualizarAsync(orden);
            var resultadoGuardado = await _mockRepository.Object.GuardarCambiosAsync();

            // Assert
            _mockRepository.Verify(repo => repo.ActualizarAsync(orden, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
            resultadoGuardado.Should().Be(1);
        }
    }
}
