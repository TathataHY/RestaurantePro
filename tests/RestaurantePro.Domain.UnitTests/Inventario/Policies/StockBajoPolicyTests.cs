#pragma warning disable CS0854 // Un árbol de expresión no puede contener una llamada o invocación que use argumentos opcionales

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
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
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

            // Crear ingrediente usando factory method
            var ingrediente = Ingrediente.Crear(
                "Tomate", 
                "TOM-001", 
                "Tomates frescos", 
                UnidadMedida.Kilogramo, 
                10, // Stock mínimo
                5,  // Stock actual
                RotacionIngrediente.Alta, 
                TemporadaIngrediente.Verano);
                
            // Corregir línea 70 - Inicialización de ID para ingrediente
            var idProperty = typeof(EntityBase).GetProperty("Id");
            if (idProperty != null)
            {
                idProperty.SetValue(ingrediente, ingredienteId);
            }
            
            var proveedorId = Guid.NewGuid();
            ingrediente.AsociarProveedorPrincipal(proveedorId);

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == ingredienteId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);

            var notificacionId = Guid.NewGuid();
            _servicioNotificacionesMock
                .Setup(s => s.NotificarStockBajo(
                    It.Is<Guid>(id => id == ingredienteId),
                    It.Is<string>(n => n == "Tomate"),
                    It.Is<decimal>(s => s == 5),
                    It.Is<decimal>(s => s == 10)))
                .ReturnsAsync(notificacionId);

            var ordenCompraId = Guid.NewGuid();
            
            // Crear OrdenCompra usando factory method con Result
            var items = new List<ItemOrdenCompra>();
            items.Add(ItemOrdenCompra.Crear(ordenCompraId, ingredienteId, "Tomate", 10, UnidadMedida.Kilogramo));
            
            var ordenCompraResult = OrdenCompra.Crear(
                proveedorId,
                items,
                fechaActual,
                fechaActual.AddDays(7),
                "Orden automática",
                _notificationManager);
                
            var ordenCompra = ordenCompraResult.Value;
            
            // Corregir línea 96 - Inicialización de ID para ordenCompra
            if (idProperty != null)
            {
                idProperty.SetValue(ordenCompra, ordenCompraId);
            }

            var resultadoVerificacion = new ResultadoVerificacionStock();
            resultadoVerificacion.OrdenesGeneradas.Add(ordenCompra);

            _verificadorStockMock
                .Setup(v => v.VerificarYGenerarOrdenesCompraAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(resultadoVerificacion));

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
                .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == ingredienteId), It.IsAny<CancellationToken>()))
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
                Ingrediente.Crear(
                    "Tomate", 
                    "TOM-001", 
                    "Tomates frescos", 
                    UnidadMedida.Kilogramo, 
                    10, // Stock mínimo
                    5,  // Stock actual
                    RotacionIngrediente.Alta, 
                    TemporadaIngrediente.Verano),
                    
                Ingrediente.Crear(
                    "Papa", 
                    "PAP-001", 
                    "Papas", 
                    UnidadMedida.Kilogramo, 
                    20, // Stock mínimo
                    2,  // Stock actual
                    RotacionIngrediente.Critica, 
                    TemporadaIngrediente.TodoElAño)
            };

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
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
            result.Errors.Should().ContainSingle(e => e.ToString().Contains("IngredienteId"));
        }
    }
}

#pragma warning restore CS0854



