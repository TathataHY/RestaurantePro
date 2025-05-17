namespace RestaurantePro.Domain.UnitTests.Comercial.Services
{
    public class ServicioFidelizacionTests
    {
        private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IHistorialPuntosRepository> _historialPuntosRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        
        public ServicioFidelizacionTests()
        {
            _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _historialPuntosRepositoryMock = new Mock<IHistorialPuntosRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));
        }

        [Fact]
        public async Task CalcularDescuento_ClienteSinTarjeta_DebeRetornarCero()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var totalComanda = 1000m;
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(
                    It.IsAny<Guid>(), 
                    _cancellationToken))
                .ReturnsAsync((TarjetaFidelizacion)null);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var resultado = await servicio.CalcularDescuentoAsync(clienteId, totalComanda);

            // Assert
            resultado.PorcentajeDescuento.Should().Be(0);
            resultado.MontoDescuento.Should().Be(0);
        }
        
        [Fact]
        public async Task CalcularDescuento_TarjetaInactiva_DebeRetornarCero()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var totalComanda = 1000m;
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(
                    It.IsAny<Guid>(), 
                    _cancellationToken))
                .ReturnsAsync((TarjetaFidelizacion)null);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var resultado = await servicio.CalcularDescuentoAsync(clienteId, totalComanda);

            // Assert
            resultado.PorcentajeDescuento.Should().Be(0);
            resultado.MontoDescuento.Should().Be(0);
        }
        
        [Fact]
        public async Task CalcularDescuento_ClienteNivel1_DebeAplicarDescuentoNivel1()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var totalComanda = 1000m;
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Plata);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(
                    It.IsAny<Guid>(), 
                    _cancellationToken))
                .ReturnsAsync(tarjeta);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var resultado = await servicio.CalcularDescuentoAsync(clienteId, totalComanda);

            // Assert
            resultado.PorcentajeDescuento.Should().Be(5); // 5% para nivel Plata
            resultado.MontoDescuento.Should().Be(50); // 1000 * 0.05 = 50
        }
        
        [Fact]
        public async Task CalcularDescuento_ClienteNivel2_DebeAplicarDescuentoNivel2()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var totalComanda = 1000m;
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(
                    It.IsAny<Guid>(), 
                    _cancellationToken))
                .ReturnsAsync(tarjeta);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var resultado = await servicio.CalcularDescuentoAsync(clienteId, totalComanda);

            // Assert
            resultado.PorcentajeDescuento.Should().Be(10); // 10% para nivel Oro
            resultado.MontoDescuento.Should().Be(100); // 1000 * 0.10 = 100
        }
        
        [Fact]
        public async Task CalcularDescuento_ClienteNivel3_DebeAplicarDescuentoNivel3()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var totalComanda = 1000m;
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Platino);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(
                    It.IsAny<Guid>(), 
                    _cancellationToken))
                .ReturnsAsync(tarjeta);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            var resultado = await servicio.CalcularDescuentoAsync(clienteId, totalComanda);

            // Assert
            resultado.PorcentajeDescuento.Should().Be(15); // 15% para nivel Platino
            resultado.MontoDescuento.Should().Be(150); // 1000 * 0.15 = 150
        }
        
        [Fact]
        public async Task AcumularPuntos_DebeCrearRegistroHistorialYActualizarTarjeta()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var totalComanda = 1000m;
            var comandaId = Guid.NewGuid();
            var puntosPrevios = 500;
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Plata, puntosPrevios);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(
                    It.IsAny<Guid>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);
            
            _tarjetaRepositoryMock.Setup(r => r.ActualizarAsync(
                    It.IsAny<TarjetaFidelizacion>(), 
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            _historialPuntosRepositoryMock.Setup(r => r.AgregarAsync(
                    It.IsAny<HistorialPuntos>(), 
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            await servicio.AcumularPuntosAsync(clienteId, comandaId, totalComanda);

            // Assert
            // Verificamos que se llamaron los métodos correctos
            _tarjetaRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()), Times.Once());
            _historialPuntosRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<HistorialPuntos>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async Task CanjearPuntos_ClienteSinSuficientesPuntos_DebeLanzarExcepcion()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntosCanjear = 500;
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro, 300); // Solo tiene 300 puntos
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(
                    It.IsAny<Guid>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                servicio.CanjearPuntosAsync(clienteId, puntosCanjear, "Descuento comanda"));
        }
        
        [Fact]
        public async Task CanjearPuntos_ClienteConSuficientesPuntos_DebeActualizarTarjetaYCrearHistorial()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntosCanjear = 300;
            var puntosPrevios = 500;
            var motivo = "Descuento comanda";
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro, puntosPrevios);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(
                    It.IsAny<Guid>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);
            
            _tarjetaRepositoryMock.Setup(r => r.ActualizarAsync(
                    It.IsAny<TarjetaFidelizacion>(), 
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            _historialPuntosRepositoryMock.Setup(r => r.AgregarAsync(
                    It.IsAny<HistorialPuntos>(), 
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            await servicio.CanjearPuntosAsync(clienteId, puntosCanjear, motivo);

            // Assert
            _tarjetaRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()), Times.Once());
            _historialPuntosRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<HistorialPuntos>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task AcumularPuntos_ClienteNoExiste_DebeLanzarExcepcion()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var totalComanda = 1000m;
            var comandaId = Guid.NewGuid();
            
            // Configurar que el cliente no existe
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, _cancellationToken))
                .ReturnsAsync((Cliente)null);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                servicio.AcumularPuntosAsync(clienteId, comandaId, totalComanda));
        }
        
        [Fact]
        public async Task CanjearPuntos_TarjetaSuspendida_DebeLanzarExcepcion()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntosCanjear = 300;
            var motivo = "Descuento comanda";
            
            // Crear una tarjeta suspendida
            var tarjeta = TarjetaFidelizacion.Crear(
                clienteId, 
                $"TF-{Guid.NewGuid().ToString().Substring(0, 8)}");
                
            tarjeta.Activar(); // Primero activamos
            tarjeta.AgregarPuntos(500); // Añadimos puntos
            tarjeta.Suspender("Motivo de prueba"); // Luego suspendemos
            
            // Configurar que se devuelve la tarjeta suspendida (que no está activa)
            _tarjetaRepositoryMock.Setup(r => r.ObtenerPorClienteIdAsync(clienteId, _cancellationToken))
                .ReturnsAsync(tarjeta);
                
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, _cancellationToken))
                .ReturnsAsync((TarjetaFidelizacion)null);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                servicio.CanjearPuntosAsync(clienteId, puntosCanjear, motivo));
        }
        
        // Método auxiliar para crear una tarjeta de fidelización activa
        private TarjetaFidelizacion CrearTarjetaActiva(Guid clienteId, NivelFidelizacion nivel, int puntosAcumulados = 0)
        {
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "CARD-" + Guid.NewGuid().ToString().Substring(0, 8));
            tarjeta.Activar();
                
            // Agregar puntos si es necesario
            if (puntosAcumulados > 0)
            {
                for (int i = 0; i < puntosAcumulados; i += 10)
            {
                    tarjeta.AgregarPuntos(10, "Acumulación para prueba");
                }
            }
            
            // Actualizar nivel si es diferente del básico
            if (nivel != NivelFidelizacion.Basico)
            {
                tarjeta.ActualizarNivel(nivel);
            }
            
            return tarjeta;
        }
    }
} 





