namespace RestaurantePro.Domain.UnitTests.Inventario.Policies
{
    public class StockBajoPolicyTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IServicioNotificacionesInventario> _servicioNotificacionesMock;
        private readonly Mock<IVerificadorStock> _verificadorStockMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly StockBajoPolicy _policy;

        public StockBajoPolicyTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _servicioNotificacionesMock = new Mock<IServicioNotificacionesInventario>();
            _verificadorStockMock = new Mock<IVerificadorStock>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();

            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));

            _policy = new StockBajoPolicy(
                _ingredienteRepositoryMock.Object,
                _servicioNotificacionesMock.Object,
                _verificadorStockMock.Object,
                _dateTimeServiceMock.Object);
        }

        [Fact]
        public async Task EjecutarPolicy_ConIngredientesBajoStock_DebeNotificarYGenerarOrdenes()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngrediente("Tomate", 5, 2),
                CrearIngrediente("Cebolla", 8, 3)
            };

            // Configuración de repository sin usar It.IsAny
            ConfigurarObtenerConStockBajo(ingredientes);

            // Configuración del resultado del verificador
            var resultadoVerificacion = new ResultadoVerificacionStock();
            var proveedor = Guid.NewGuid();
            var orden = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                proveedor,
                "Orden de prueba",
                DateTime.Now);
            resultadoVerificacion.OrdenesGeneradas.Add(orden);

            // Configuración de mock sin usar It.IsAny
            ConfigurarVerificadorStock(resultadoVerificacion);
            ConfigurarNotificacionStockBajo(Guid.NewGuid());

            // Act
            var resultado = await _policy.EjecutarPolicy();

            // Assert
            resultado.Notificaciones.Should().HaveCount(2);
            resultado.OrdenesCompraGeneradas.Should().HaveCount(1);

            // Verificación usando métodos sin problemas de árboles de expresión
            VerificarNotificacionesEnviadas(2);
            VerificarVerificadorInvocado(1);
        }

        [Fact]
        public async Task EjecutarPolicy_SinIngredientesBajoStock_NoDebeNotificarNiGenerarOrdenes()
        {
            // Arrange
            ConfigurarObtenerConStockBajo(new List<Ingrediente>());

            // Act
            var resultado = await _policy.EjecutarPolicy();

            // Assert
            resultado.Notificaciones.Should().BeEmpty();
            resultado.OrdenesCompraGeneradas.Should().BeEmpty();

            VerificarNotificacionesEnviadas(0);
            VerificarVerificadorInvocado(0);
        }

        [Fact]
        public async Task EjecutarPolicy_ConIngredientesBajoStockPeroSinGenerarOrdenes_DebeNotificarPeroNoGenerarOrdenes()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngrediente("Tomate", 5, 2),
                CrearIngrediente("Cebolla", 8, 3)
            };

            ConfigurarObtenerConStockBajo(ingredientes);
            ConfigurarVerificadorStock(new ResultadoVerificacionStock());
            ConfigurarNotificacionStockBajo(Guid.NewGuid());

            // Act
            var resultado = await _policy.EjecutarPolicy();

            // Assert
            resultado.Notificaciones.Should().HaveCount(2);
            resultado.OrdenesCompraGeneradas.Should().BeEmpty();

            VerificarNotificacionesEnviadas(2);
            VerificarVerificadorInvocado(1);
        }

        // Métodos auxiliares para configurar mocks evitando problemas de árboles de expresión
        private void ConfigurarObtenerConStockBajo(List<Ingrediente> ingredientes)
        {
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.Is<CancellationToken>(t => true)))
                .ReturnsAsync(ingredientes);
        }

        private void ConfigurarVerificadorStock(ResultadoVerificacionStock resultado) 
        { 
            _verificadorStockMock
                .Setup(v => v.VerificarYGenerarOrdenesCompraAsync(It.Is<CancellationToken>(t => true)))
                .ReturnsAsync(resultado); 
        }

        private void ConfigurarNotificacionStockBajo(Guid notificacionId)
        {
            // Usamos genéricamente para cualquier parámetro para evitar árboles de expresión
            _servicioNotificacionesMock
                .Setup(s => s.NotificarStockBajo(
                    It.Is<Guid>(g => true),
                    It.Is<string>(s => true),
                    It.Is<decimal>(d => true),
                    It.Is<decimal>(d => true)))
                .ReturnsAsync(notificacionId);
        }

        private void VerificarNotificacionesEnviadas(int veces)
        {
            _servicioNotificacionesMock.Verify(
                s => s.NotificarStockBajo(
                    It.Is<Guid>(g => true),
                    It.Is<string>(s => true),
                    It.Is<decimal>(d => true),
                    It.Is<decimal>(d => true)),
                Times.Exactly(veces));
        }

        private void VerificarVerificadorInvocado(int veces) 
        { 
            _verificadorStockMock.Verify(
                v => v.VerificarYGenerarOrdenesCompraAsync(It.Is<CancellationToken>(t => true)), 
                Times.Exactly(veces)); 
        }

        private Ingrediente CrearIngrediente(string nombre, decimal stockMinimo, decimal stockActual)
        {
            var ingrediente = Ingrediente.Crear(
                nombre,
                "ING-" + Guid.NewGuid().ToString().Substring(0, 5),
                $"Descripción de {nombre}",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                stockMinimo,
                stockActual);

            // Asignar un proveedor ficticio
            ingrediente.AsociarProveedorPrincipal(Guid.NewGuid());

            return ingrediente;
        }
    }
}



