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
        private readonly Mock<ICurrentUserService> _currentUserMock;
        
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
            _currentUserMock = new Mock<ICurrentUserService>();
            
            _handler = new AcumularPuntosHandler(
                _clienteRepositoryMock.Object,
                _tarjetaRepositoryMock.Object,
                _transaccionRepositoryMock.Object,
                _promocionRepositoryMock.Object,
                _calculadoraPuntosMock.Object,
                _servicioFidelizacionMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _currentUserMock.Object);
        }
        
        [Fact]
        public async Task Handle_AcumulacionVentaBasica_DeberiaCalcularPuntosCorrectamente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();
            var facturaId = Guid.NewGuid();
            
            // Utilizar los métodos factory de ayuda existentes
            var cliente = CreateMockCliente(clienteId);
            var tarjeta = CreateMockTarjeta(clienteId);
            
            // Configuramos el ID de la tarjeta para que coincida con el esperado
            // Usando reflexión, solo para pruebas
            typeof(TarjetaFidelizacion).GetProperty("Id")?.SetValue(tarjeta, tarjetaId);
                
            // Configura mocks
            _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
            
            // Setup correcto solo para la firma con token
            _clienteRepositoryMock
                .Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) =>
                    id == clienteId ? cliente : null);
                
            _tarjetaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>(), false))
                .ReturnsAsync(tarjeta);
                
            _tarjetaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);
                
            // Configura el cálculo de puntos con el constructor correcto
            var calculoResultado = new CalculoResultadoPuntos(100, 0.1m, 0, "Test calculation");
            
            _calculadoraPuntosMock.Setup(x => x.CalcularPuntosPorCompraAsync(
                    It.IsAny<Guid>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(calculoResultado));
                
            _transaccionRepositoryMock.Setup(x => x.AgregarAsync(It.IsAny<RestaurantePro.Domain.Comercial.Clientes.Entities.TransaccionPuntos>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            _tarjetaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            var command = new AcumularPuntosCommand
            {
                ClienteId = clienteId,
                TarjetaFidelizacionId = tarjetaId,
                MontoCompra = 1000,
                TipoTransaccion = RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.Compra,
                FacturaId = facturaId,
                Canal = "Web",
                Comentarios = "Compra test"
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            // Imprimir el mensaje de error para diagnóstico
            if (!result.Succeeded)
            {
                Console.WriteLine($"ERROR DETALLADO: {result.Error}");
                
                // Imprimir los mocks configurados
                Console.WriteLine($"Mock calculadora configurado para: TarjetaId={tarjetaId}, Monto={command.MontoCompra}");
                Console.WriteLine($"Comando solicitado: ClienteId={command.ClienteId}, TarjetaId={command.TarjetaFidelizacionId}, MontoCompra={command.MontoCompra}");
                Console.WriteLine($"Tipo transacción: {command.TipoTransaccion}");
            }
            
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.PuntosAcumulados.Should().Be(100);
            result.Value.TarjetaFidelizacionId.Should().Be(tarjetaId);
            result.Value.ClienteId.Should().Be(clienteId);
            result.Value.FacturaId.Should().Be(facturaId);
            
            // Verificar que se llamaron los métodos esperados
            _tarjetaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()), Times.Once);
            _transaccionRepositoryMock.Verify(x => x.AgregarAsync(It.IsAny<RestaurantePro.Domain.Comercial.Clientes.Entities.TransaccionPuntos>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ClienteInexistente_DeberiaRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid(); // Este Guid no coincide con el del cliente mockeado
            var command = new AcumularPuntosCommand
            {
                ClienteId = clienteId,
                MontoCompra = 100.00m,
                TipoTransaccion = RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos.TipoTransaccionPuntos.Compra,
                Canal = "Web"
            };

            // No configurar setup específico, el global ya devuelve null si el Guid no coincide

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
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, $"TF{clienteId.ToString()[..8]}");
            tarjeta.Activar(); // Activar la tarjeta para permitir acumulación de puntos
            return tarjeta;
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
