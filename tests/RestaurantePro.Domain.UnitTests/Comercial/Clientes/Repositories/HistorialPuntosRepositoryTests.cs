using RestaurantePro.Domain.Comercial.Clientes.Interfaces;

namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Repositories
{
    public class HistorialPuntosRepositoryTests
    {
        private readonly Mock<IHistorialPuntosRepository> _mockRepository;
        private readonly Guid _tarjetaId;
        private readonly List<HistorialPuntos> _historialItems;

        public HistorialPuntosRepositoryTests()
        {
            _mockRepository = new Mock<IHistorialPuntosRepository>();
            _tarjetaId = Guid.NewGuid();
            
            // Crear datos de prueba
            _historialItems = new List<HistorialPuntos>
            {
                HistorialPuntos.CrearRegistroAgregados(_tarjetaId, 100, "Compra inicial"),
                HistorialPuntos.CrearRegistroPorCompra(_tarjetaId, 500m, 10, "Compra restaurante"),
                HistorialPuntos.CrearRegistroCanjeados(_tarjetaId, 30, "Descuento aplicado"),
                HistorialPuntos.CrearRegistroVencidos(_tarjetaId, 20, "Puntos vencidos"),
                HistorialPuntos.CrearRegistroAjuste(_tarjetaId, 50, "Ajuste manual")
            };
        }
        
        [Fact]
        public async Task ObtenerPorIdAsync_IdExistente_DebeRetornarRegistro()
        {
            // Arrange
            var id = Guid.NewGuid();
            var historialEsperado = HistorialPuntos.CrearRegistroAgregados(_tarjetaId, 100, "Prueba");
            
            // Configurar mock
            _mockRepository.Setup(repo => repo.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(historialEsperado);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerPorIdAsync(id);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(historialEsperado);
            _mockRepository.Verify(repo => repo.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ObtenerPorTarjetaIdAsync_TarjetaValida_DebeRetornarTodosLosRegistros()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.ObtenerPorTarjetaIdAsync(_tarjetaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_historialItems);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerPorTarjetaIdAsync(_tarjetaId);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(5);
            resultado.Should().BeEquivalentTo(_historialItems);
        }
        
        [Fact]
        public async Task ObtenerPorTipoOperacionAsync_TipoAgregados_DebeRetornarSoloAgregados()
        {
            // Arrange
            var tipoOperacion = TipoOperacionPuntos.Agregados;
            var registrosAgregados = _historialItems.Where(h => h.TipoOperacion == tipoOperacion).ToList();
            
            _mockRepository.Setup(repo => repo.ObtenerPorTipoOperacionAsync(_tarjetaId, tipoOperacion, It.IsAny<CancellationToken>()))
                .ReturnsAsync(registrosAgregados);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerPorTipoOperacionAsync(_tarjetaId, tipoOperacion);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2); // Tenemos dos registros de tipo Agregados
            resultado.All(r => r.TipoOperacion == tipoOperacion).Should().BeTrue();
        }
        
        [Fact]
        public async Task ObtenerPorRangoFechasAsync_FechasValidas_DebeRetornarRegistrosEnRango()
        {
            // Arrange
            var fechaInicio = DateTime.Now.AddDays(-7);
            var fechaFin = DateTime.Now.AddDays(1);
            
            _mockRepository.Setup(repo => repo.ObtenerPorRangoFechasAsync(
                _tarjetaId, fechaInicio, fechaFin, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_historialItems);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerPorRangoFechasAsync(_tarjetaId, fechaInicio, fechaFin);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(5);
        }
        
        [Fact]
        public async Task ObtenerTotalPuntosPorTipoAsync_TipoCanjeados_DebeRetornarSumaDePuntos()
        {
            // Arrange
            var tipoOperacion = TipoOperacionPuntos.Canjeados;
            var puntosEsperados = 30; // solo tenemos un registro de 30 puntos canjeados
            
            _mockRepository.Setup(repo => repo.ObtenerTotalPuntosPorTipoAsync(
                _tarjetaId, tipoOperacion, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(puntosEsperados);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerTotalPuntosPorTipoAsync(
                _tarjetaId, tipoOperacion, DateTime.Now.AddDays(-30), DateTime.Now);
            
            // Assert
            resultado.Should().Be(puntosEsperados);
        }
        
        [Fact]
        public async Task AgregarAsync_RegistroValido_DebeAgregarCorrectamente()
        {
            // Arrange
            var nuevoHistorial = HistorialPuntos.CrearRegistroAgregados(_tarjetaId, 200, "Nuevo registro");
            
            _mockRepository.Setup(repo => repo.AgregarAsync(nuevoHistorial, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            _mockRepository.Setup(repo => repo.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
                
            // Act
            await _mockRepository.Object.AgregarAsync(nuevoHistorial);
            var resultadoGuardado = await _mockRepository.Object.GuardarCambiosAsync();
            
            // Assert
            _mockRepository.Verify(repo => repo.AgregarAsync(nuevoHistorial, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
            resultadoGuardado.Should().Be(1);
        }
    }
} 