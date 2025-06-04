namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Commands
{
    public class AcumularPuntosHandlerTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
        private readonly Mock<ITransaccionPuntosRepository> _transaccionRepositoryMock;
        private readonly Mock<IPromocionRepository> _promocionRepositoryMock;
        private readonly Mock<ICalculadoraPuntosService> _calculadoraPuntosMock;
        private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<AcumularPuntosHandler>> _loggerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly AcumularPuntosHandler _handler;

        public AcumularPuntosHandlerTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
            _transaccionRepositoryMock = new Mock<ITransaccionPuntosRepository>();
            _promocionRepositoryMock = new Mock<IPromocionRepository>();
            _calculadoraPuntosMock = new Mock<ICalculadoraPuntosService>();
            _servicioFidelizacionMock = new Mock<IServicioFidelizacion>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<AcumularPuntosHandler>>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            _handler = new AcumularPuntosHandler(
                _clienteRepositoryMock.Object,
                _tarjetaRepositoryMock.Object,
                _transaccionRepositoryMock.Object,
                _promocionRepositoryMock.Object,
                _calculadoraPuntosMock.Object,
                _servicioFidelizacionMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_AcumulacionVentaBasica_DeberiaCalcularPuntosCorrectamente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var command = new AcumularPuntosCommand
            {
                ClienteId = clienteId,
                MontoCompra = 120.50m,
                TipoAcumulacion = TipoAcumulacion.PorCompra
            };

            // Setup mocks
            var clienteMock = CreateMockCliente(clienteId);
            var tarjetaMock = CreateMockTarjeta(clienteId);
            var calculoResultado = CreateMockCalculoResultado(120);
            var resultadoAcumulacion = CreateMockResultadoVentaBasica(clienteId);

            _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId))
                .ReturnsAsync(clienteMock);

            _tarjetaRepositoryMock.Setup(x => x.ObtenerTarjetaActivaPorClienteIdAsync(clienteId))
                .ReturnsAsync(tarjetaMock);

            _calculadoraPuntosMock.Setup(x => x.CalcularPuntosPorCompraAsync(
                    It.IsAny<Guid>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(calculoResultado));

            _transaccionRepositoryMock.Setup(x => x.AgregarAsync(It.IsAny<TransaccionPuntos>()))
                .Returns(Task.CompletedTask);

            _tarjetaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<TarjetaFidelizacion>()))
                .Returns(Task.CompletedTask);

            _currentUserServiceMock.Setup(x => x.UserId)
                .Returns(Guid.NewGuid().ToString());

            _mapperMock.Setup(x => x.Map<AcumulacionPuntosDto>(It.IsAny<object>()))
                .Returns(resultadoAcumulacion);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Value);
            Assert.Equal(clienteId, result.Value.ClienteId);
        }

        [Fact]
        public async Task Handle_ClienteInexistente_DeberiaRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var command = new AcumularPuntosCommand
            {
                ClienteId = clienteId,
                MontoCompra = 100.00m
            };

            _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId))
                .ReturnsAsync((Cliente)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("no existe", result.Error);
        }

        // Métodos de ayuda
        private static Cliente CreateMockCliente(Guid clienteId, bool activo = true)
        {
            var nombre = ClienteNombre.Crear("Juan", "Perez");
            var email = Email.Create("juan.perez@test.com");
            var telefono = PhoneNumber.Create("123456789");

            return Cliente.Crear(
                clienteId,
                nombre,
                email,
                telefono,
                DateTime.Now.AddYears(-30),
                activo);
        }

        private static TarjetaFidelizacion CreateMockTarjeta(Guid clienteId)
        {
            return TarjetaFidelizacion.Crear(clienteId, $"TF{clienteId.ToString()[..8]}");
        }

        private static CalculoResultadoPuntos CreateMockCalculoResultado(decimal puntos)
        {
            return new CalculoResultadoPuntos((int)puntos, 1.0m, 0, "Mock calculation");
        }

        private static Promocion CreateMockPromocion()
        {
            return Promocion.Crear(
                "DOUBLE2025",
                "Doble Puntos 2025", 
                "Promocion de doble puntos para el año 2025",
                TipoPromocion.CanjePuntos,
                2.0m,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow.AddDays(30),
                0m,
                0,
                null,
                false);
        }

        private static AcumulacionPuntosDto CreateMockResultadoVentaBasica(Guid clienteId)
        {
            return new AcumulacionPuntosDto
            {
                ClienteId = clienteId,
                TarjetaFidelizacionId = Guid.NewGuid(),
                PuntosAcumulados = 121,
                TotalPuntos = 2421,
                MontoTransaccion = 120.50m,
                FactorMultiplicacion = 1.0m,
                FechaAcumulacion = DateTime.UtcNow,
                Concepto = "Acumulacion por Compra"
            };
        }
    }
}
