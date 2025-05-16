namespace RestaurantePro.Domain.UnitTests.Comercial.Services
{
    public class ServicioFidelizacionTests
    {
        private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IHistorialPuntosRepository> _historialPuntosRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        
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
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
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
            
            // En este caso, como ahora usamos ObtenerTarjetaActivaPorClienteIdAsync,
            // que ya filtra por tarjetas activas, esta prueba se modifica para simular
            // que el repositorio no devuelve ninguna tarjeta (que es el comportamiento esperado)
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
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
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
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
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
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
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
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
            var puntosAcumulados = 100; // 10% del total de la comanda
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Plata, puntosPrevios);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
                .ReturnsAsync(tarjeta);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            await servicio.AcumularPuntosAsync(clienteId, comandaId, totalComanda);

            // Assert
            // Verifica que se actualizó la tarjeta con puntos adicionales
            _tarjetaRepositoryMock.Verify(r => r.ActualizarAsync(It.Is<TarjetaFidelizacion>(
                t => t.Id == tarjeta.Id && t.PuntosAcumulados == puntosPrevios + puntosAcumulados)), 
                Times.Once);
                
            // Verifica que se creó registro en el historial
            _historialPuntosRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<HistorialPuntos>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task CanjearPuntos_ClienteSinSuficientesPuntos_DebeLanzarExcepcion()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntosCanjear = 500;
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro, 300); // Solo tiene 300 puntos
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
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
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
                .ReturnsAsync(tarjeta);
                
            var servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object);

            // Act
            await servicio.CanjearPuntosAsync(clienteId, puntosCanjear, motivo);

            // Assert
            // Verifica que se actualizó la tarjeta con menos puntos
            _tarjetaRepositoryMock.Verify(r => r.ActualizarAsync(It.Is<TarjetaFidelizacion>(
                t => t.Id == tarjeta.Id && t.PuntosAcumulados == puntosPrevios - puntosCanjear)), 
                Times.Once);
                
            // Verifica que se creó registro en el historial
            _historialPuntosRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<HistorialPuntos>()), 
                Times.Once);
        }
        
        // Métodos auxiliares para crear objetos de prueba
        private TarjetaFidelizacion CrearTarjetaActiva(Guid clienteId, NivelFidelizacion nivel, int puntosAcumulados = 0)
        {
            return TarjetaFidelizacion.Crear(
                clienteId, 
                $"TF-{Guid.NewGuid().ToString().Substring(0, 8)}",
                nivel);
        }
        
        private TarjetaFidelizacion CrearTarjetaInactiva(Guid clienteId)
        {
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Plata);
            tarjeta.Desactivar();
            return tarjeta;
        }
    }
} 