namespace RestaurantePro.Domain.UnitTests.Inventario.Policies
{
    public class StockBajoPolicyTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IServicioNotificacionesInventario> _servicioNotificacionesMock;
        private readonly Mock<IVerificadorStock> _verificadorStockMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly NotificationManager _notificationManager;
        private readonly StockBajoPolicy _sut;

        public StockBajoPolicyTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _servicioNotificacionesMock = new Mock<IServicioNotificacionesInventario>();
            _verificadorStockMock = new Mock<IVerificadorStock>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManager = new NotificationManager();

            _sut = new StockBajoPolicy(
                _ingredienteRepositoryMock.Object,
                _servicioNotificacionesMock.Object,
                _verificadorStockMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManager);
        }

        [Fact]
        public async Task EjecutarPolicy_SinIngredientesConStockBajo_DebeRetornarResultadoVacio()
        {
            // Arrange
            var fechaActual = new DateTime(2025, 7, 15);
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);

            var resultadoPriorizacion = new StockBajoPolicyData();

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(new List<Ingrediente>());

            // Act
            var result = await _sut.EjecutarPolicy();

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.IngredientesPriorizados.Should().BeEmpty();
            result.Value.Notificaciones.Should().BeEmpty();
            result.Value.OrdenesCompraGeneradas.Should().BeEmpty();
        }

        [Fact]
        public async Task EjecutarPolicyParaIngrediente_ConIngredienteExistente_DebeRetornarResultadoExitoso()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var fechaActual = new DateTime(2025, 7, 15);
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);

            var ingrediente = new Ingrediente
            {
                Id = ingredienteId,
                Nombre = "Tomate",
                Stock = 5,
                StockMinimo = 10,
                Rotacion = RotacionIngrediente.Alta,
                Temporada = TemporadaIngrediente.Verano,
                CostoPromedio = 100,
                ProveedorPrincipalId = Guid.NewGuid()
            };

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);

            var notificacionId = Guid.NewGuid();
            _servicioNotificacionesMock
                .Setup(s => s.NotificarStockBajo(
                    ingredienteId,
                    "Tomate",
                    5,
                    10))
                .ReturnsAsync(notificacionId);

            var ordenCompraId = Guid.NewGuid();
            var ordenCompra = new OrdenCompra
            {
                Id = ordenCompraId,
                ProveedorId = ingrediente.ProveedorPrincipalId.Value,
                Items = new List<ItemOrdenCompra>
                {
                    new ItemOrdenCompra
                    {
                        IngredienteId = ingredienteId,
                        Cantidad = 10,
                        PrecioUnitario = 100
                    }
                }
            };

            var resultadoVerificacion = new ResultadoVerificacionStock
            {
                OrdenesGeneradas = new List<OrdenCompra> { ordenCompra }
            };

            _verificadorStockMock
                .Setup(v => v.VerificarYGenerarOrdenesCompraAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultadoVerificacion);

            // Act
            var result = await _sut.EjecutarPolicyParaIngrediente(ingredienteId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.IngredientesPriorizados.Should().ContainSingle();
            result.Value.Notificaciones.Should().ContainSingle();
            result.Value.Notificaciones.Should().Contain(notificacionId);
            result.Value.OrdenesCompraGeneradas.Should().ContainSingle();
            result.Value.OrdenesCompraGeneradas.Should().Contain(ordenCompraId);
        }

        [Fact]
        public async Task EjecutarPolicyParaIngrediente_ConIngredienteInexistente_DebeRetornarFallo()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ingrediente)null);

            // Act
            var result = await _sut.EjecutarPolicyParaIngrediente(ingredienteId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("No existe un ingrediente");
        }

        [Fact]
        public async Task PriorizarIngredientesParaReposicion_ConIngredientes_DebeOrdenarPorPrioridad()
        {
            // Arrange
            var fechaActual = new DateTime(2025, 7, 15); // Invierno en hemisferio sur
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);

            var ingredientes = new List<Ingrediente>
            {
                new Ingrediente
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Tomate",
                    Stock = 5,
                    StockMinimo = 10,
                    Rotacion = RotacionIngrediente.Alta,
                    Temporada = TemporadaIngrediente.Verano, // Fuera de temporada
                    CostoPromedio = 100
                },
                new Ingrediente
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Papa",
                    Stock = 2,
                    StockMinimo = 20,
                    Rotacion = RotacionIngrediente.Critica,
                    Temporada = TemporadaIngrediente.TodoElAño,
                    CostoPromedio = 50
                }
            };

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(ingredientes);

            // Act
            var result = await _sut.PriorizarIngredientesParaReposicion(true);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.IngredientesPriorizados.Should().HaveCount(2);
            
            // La papa debe tener mayor prioridad (rotación crítica y todo el año)
            result.Value.IngredientesPriorizados[0].Nombre.Should().Be("Papa");
            result.Value.IngredientesPriorizados[1].Nombre.Should().Be("Tomate");
        }

        [Fact]
        public async Task EjecutarPolicyParaIngrediente_ConIdVacio_DebeRetornarErrorValidacion()
        {
            // Arrange
            var ingredienteId = Guid.Empty;

            // Act
            var result = await _sut.EjecutarPolicyParaIngrediente(ingredienteId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "IngredienteId");
        }
    }
}



