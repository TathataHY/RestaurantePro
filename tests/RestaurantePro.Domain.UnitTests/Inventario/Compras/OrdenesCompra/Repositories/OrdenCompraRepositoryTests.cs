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
            
            _ordenesCompra = new List<OrdenCompra>();
            
            // Crear primera orden
            var orden1 = OrdenCompra.Crear(Guid.NewGuid(), "Orden 1", fechaHoy);
            orden1.EstablecerFechaEntrega(fechaHoy.AddDays(5));
            _ordenesCompra.Add(orden1);
            
            // Crear segunda orden
            var orden2 = OrdenCompra.Crear(Guid.NewGuid(), "Orden 2", fechaHoy);
            orden2.EstablecerFechaEntrega(fechaHoy.AddDays(7));
            _ordenesCompra.Add(orden2);
            
            // Crear tercera orden
            var orden3 = OrdenCompra.Crear(Guid.NewGuid(), "Orden 3", fechaHoy);
            orden3.EstablecerFechaEntrega(fechaHoy.AddDays(3));
            _ordenesCompra.Add(orden3);
            
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

            _mockRepository.Setup(repo => repo.ObtenerPorIdAsync(ordenId, CancellationToken.None))
                .ReturnsAsync(ordenEsperada);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorIdAsync(ordenId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(ordenEsperada);
            _mockRepository.Verify(repo => repo.ObtenerPorIdAsync(ordenId, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task ObtenerTodasAsync_DebeRetornarTodasLasOrdenes()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.ObtenerTodasAsync(CancellationToken.None))
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

            _mockRepository.Setup(repo => repo.ObtenerPorEstadoAsync(EstadoOrdenCompra.Enviada, CancellationToken.None))
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

            _mockRepository.Setup(repo => repo.ObtenerPorProveedorAsync(proveedorId, CancellationToken.None))
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
            
            // Añadimos un item con el ingrediente específico a la segunda orden (que está en estado pendiente)
            _ordenesCompra[1].AgregarItem(ingredienteId, "Ingrediente Específico", 5.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            var ordenesConIngrediente = _ordenesCompra
                .Where(o => o.Items.Any(i => i.IngredienteId == ingredienteId))
                .ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorIngredienteAsync(ingredienteId, CancellationToken.None))
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

            _mockRepository.Setup(repo => repo.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin, CancellationToken.None))
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

            _mockRepository.Setup(repo => repo.ObtenerPendientesPorProveedorAsync(proveedorId, CancellationToken.None))
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
            var fechaEmision = DateTime.Now;
            var nuevaOrden = OrdenCompra.Crear(
                Guid.NewGuid(),
                "Nueva orden",
                fechaEmision);
                
            // Establecer fecha de entrega coherente
            nuevaOrden.EstablecerFechaEntrega(fechaEmision.AddDays(3));

            _mockRepository.Setup(repo => repo.AgregarAsync(nuevaOrden, CancellationToken.None))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(repo => repo.GuardarCambiosAsync(CancellationToken.None))
                .ReturnsAsync(1);

            // Act
            await _mockRepository.Object.AgregarAsync(nuevaOrden);
            var resultadoGuardado = await _mockRepository.Object.GuardarCambiosAsync();

            // Assert
            _mockRepository.Verify(repo => repo.AgregarAsync(nuevaOrden, CancellationToken.None), Times.Once);
            _mockRepository.Verify(repo => repo.GuardarCambiosAsync(CancellationToken.None), Times.Once);
            resultadoGuardado.Should().Be(1);
        }

        [Fact]
        public async Task ActualizarAsync_OrdenExistente_DebeActualizarCorrectamente()
        {
            // Arrange
            var orden = _ordenesCompra[0];

            _mockRepository.Setup(repo => repo.ActualizarAsync(orden, CancellationToken.None))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(repo => repo.GuardarCambiosAsync(CancellationToken.None))
                .ReturnsAsync(1);

            // Act
            await _mockRepository.Object.ActualizarAsync(orden);
            var resultadoGuardado = await _mockRepository.Object.GuardarCambiosAsync();

            // Assert
            _mockRepository.Verify(repo => repo.ActualizarAsync(orden, CancellationToken.None), Times.Once);
            _mockRepository.Verify(repo => repo.GuardarCambiosAsync(CancellationToken.None), Times.Once);
            resultadoGuardado.Should().Be(1);
        }
    }
}


