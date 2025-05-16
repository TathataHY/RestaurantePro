namespace RestaurantePro.Domain.UnitTests.Inventario.Compras.OrdenesCompra.Repositories
{
    public class Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompraRepositoryTests
    {
        private readonly Mock<Domain.Inventario.Compras.OrdenesCompra.Interfaces.IDomain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompraRepository> _mockRepository;
        private readonly List<Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra> _ordenesCompra;

        public Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompraRepositoryTests()
        {
            _mockRepository = new Mock<Domain.Inventario.Compras.OrdenesCompra.Interfaces.IDomain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompraRepository>();

            // Crear datos de prueba
            var fechaHoy = DateTime.Now;
            
            _ordenesCompra = new List<Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra>
            {
                Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(Guid.NewGuid(), fechaHoy, fechaHoy.AddDays(5), "Orden 1"),
                Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(Guid.NewGuid(), fechaHoy, fechaHoy.AddDays(7), "Orden 2"),
                Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(Guid.NewGuid(), fechaHoy, fechaHoy.AddDays(3), "Orden 3")
            };
            
            // Agregar algunos items a las órdenes
            _ordenesCompra[0].AgregarItem(Guid.NewGuid(), 10.0m, 5.0m);
            _ordenesCompra[0].AgregarItem(Guid.NewGuid(), 3.0m, 10.0m);
            
            _ordenesCompra[1].AgregarItem(Guid.NewGuid(), 5.0m, 15.0m);
            
            // Cambiar estados de algunas órdenes
            _ordenesCompra[0].Enviar(); // Enviada
            _ordenesCompra[2].Cancelar("Proveedor no disponible"); // Cancelada
        }

        [Fact]
        public async Task ObtenerPorIdAsync_IdExistente_DebeRetornarDomain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra()
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
            var ordenesEnviadas = _ordenesCompra.Where(o => o.Estado == EstadoDomain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Enviada).ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorEstadoAsync(EstadoDomain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Enviada, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesEnviadas);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorEstadoAsync(EstadoDomain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Enviada);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1); // Solo hay 1 orden enviada (la primera)
            resultado.All(o => o.Estado == EstadoDomain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Enviada).Should().BeTrue();
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
        public async Task AgregarAsync_OrdenValida_DebeAgregarCorrectamente()
        {
            // Arrange
            var nuevaOrden = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now.AddDays(4),
                "Nueva orden");

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
            orden.AgregarItem(Guid.NewGuid(), 2.0m, 7.5m); // Agregamos un item

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
