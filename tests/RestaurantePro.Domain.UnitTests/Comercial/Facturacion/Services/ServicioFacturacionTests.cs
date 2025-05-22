using Moq;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Services;
using RestaurantePro.Domain.Core.Services;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;

namespace RestaurantePro.Domain.UnitTests.Comercial.Facturacion.Services
{
    public class ServicioFacturacionTests
    {
        private readonly Mock<IFacturaRepository> _facturaRepositoryMock;
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly ServicioFacturacion _servicioFacturacion;
        private readonly DateTime _fechaActual = new DateTime(2024, 1, 1, 12, 0, 0);

        public ServicioFacturacionTests()
        {
            _facturaRepositoryMock = new Mock<IFacturaRepository>();
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            _servicioFacturacion = new ServicioFacturacion(
                _facturaRepositoryMock.Object,
                _comandaRepositoryMock.Object,
                _dateTimeServiceMock.Object);
        }

        [Fact]
        public async Task GenerarFacturaParaComandaAsync_ComandaExistente_DebeGenerarFactura()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var numeroFactura = "F-2024-001";
            
            // Crear una comanda simulada con un ítem
            var comanda = new Comanda
            {
                Id = comandaId,
                EstaAnulada = false,
                Items = new List<ItemComanda>
                {
                    new ItemComanda
                    {
                        ProductoId = productoId,
                        Descripcion = "Producto de prueba",
                        Cantidad = 2,
                        PrecioUnitario = 100.0m,
                        PorcentajeDescuento = 0
                    }
                }
            };
            
            // Configurar el mock del repositorio de comandas
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            // Configurar el mock del repositorio de facturas para devolver número de factura
            _facturaRepositoryMock
                .Setup(r => r.ObtenerSiguienteNumeroFacturaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(numeroFactura);
                
            // Act
            var factura = await _servicioFacturacion.GenerarFacturaParaComandaAsync(
                comandaId,
                TipoFactura.Normal,
                "Cliente de Prueba",
                null,
                null,
                null,
                "Observaciones de prueba");
                
            // Assert
            factura.Should().NotBeNull();
            factura.NumeroFactura.Should().Be(numeroFactura);
            factura.TipoFactura.Should().Be(TipoFactura.Normal);
            factura.NombreCliente.Should().Be("Cliente de Prueba");
            factura.Observaciones.Should().Be("Observaciones de prueba");
            factura.Estado.Should().Be(EstadoFactura.Borrador);
            factura.FechaEmision.Should().Be(_fechaActual);
            factura.ComandasIds.Should().ContainSingle().Which.Should().Be(comandaId);
            
            // Verificar detalles de la factura
            factura.Detalles.Should().HaveCount(1);
            var detalle = factura.Detalles.First();
            detalle.ProductoId.Should().Be(productoId);
            detalle.Cantidad.Should().Be(2);
            detalle.PrecioUnitario.Should().Be(100.0m);
            
            // Verificar llamadas a repositorios
            _facturaRepositoryMock.Verify(r => r.AgregarAsync(factura, It.IsAny<CancellationToken>()), Times.Once);
            _facturaRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task GenerarFacturaParaComandaAsync_ComandaNoExistente_DebeLanzarExcepcion()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            
            // Configurar el mock del repositorio de comandas para devolver null
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Comanda)null);
                
            // Act & Assert
            Func<Task> action = async () => await _servicioFacturacion.GenerarFacturaParaComandaAsync(
                comandaId,
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*No se encontró la comanda con ID {comandaId}*");
        }
        
        [Fact]
        public async Task GenerarFacturaParaComandaAsync_ComandaAnulada_DebeLanzarExcepcion()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            
            // Crear una comanda simulada anulada
            var comanda = new Comanda
            {
                Id = comandaId,
                EstaAnulada = true,
                Items = new List<ItemComanda>()
            };
            
            // Configurar el mock del repositorio de comandas
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            // Act & Assert
            Func<Task> action = async () => await _servicioFacturacion.GenerarFacturaParaComandaAsync(
                comandaId,
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*No se puede generar factura para una comanda anulada*");
        }
        
        [Fact]
        public async Task EmitirFacturaAsync_FacturaExistente_DebeEmitirFactura()
        {
            // Arrange
            var facturaId = Guid.NewGuid();
            var numeroFactura = "F-2024-002";
            
            // Crear una factura simulada
            var factura = Factura.Crear(
                numeroFactura,
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            // Agregar un detalle para que se pueda emitir
            factura.AgregarDetalle(
                Guid.NewGuid(),
                "Producto",
                1,
                100.0m,
                16.0m);
                
            // Configurar el mock del repositorio de facturas
            _facturaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(factura);
                
            // Act
            var facturaEmitida = await _servicioFacturacion.EmitirFacturaAsync(facturaId, 30);
                
            // Assert
            facturaEmitida.Should().NotBeNull();
            facturaEmitida.Estado.Should().Be(EstadoFactura.Emitida);
            facturaEmitida.FechaVencimiento.Should().NotBeNull();
            facturaEmitida.FechaVencimiento.Value.Date.Should().Be(_fechaActual.AddDays(30).Date);
            
            // Verificar llamadas a repositorios
            _facturaRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task AnularFacturaAsync_FacturaExistente_DebeAnularFactura()
        {
            // Arrange
            var facturaId = Guid.NewGuid();
            var numeroFactura = "F-2024-003";
            var motivo = "Datos incorrectos";
            
            // Crear una factura simulada
            var factura = Factura.Crear(
                numeroFactura,
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            // Agregar un detalle para que se pueda emitir
            factura.AgregarDetalle(
                Guid.NewGuid(),
                "Producto",
                1,
                100.0m,
                16.0m);
                
            factura.Emitir();
                
            // Configurar el mock del repositorio de facturas
            _facturaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(factura);
                
            // Act
            var facturaAnulada = await _servicioFacturacion.AnularFacturaAsync(facturaId, motivo);
                
            // Assert
            facturaAnulada.Should().NotBeNull();
            facturaAnulada.Estado.Should().Be(EstadoFactura.Anulada);
            facturaAnulada.MotivoAnulacion.Should().Be(motivo);
            
            // Verificar llamadas a repositorios
            _facturaRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task RegistrarPagoFacturaAsync_FacturaExistente_DebeRegistrarPago()
        {
            // Arrange
            var facturaId = Guid.NewGuid();
            var pagoId = Guid.NewGuid();
            var numeroFactura = "F-2024-004";
            var monto = 50.0m; // Pago parcial
            
            // Crear una factura simulada
            var factura = Factura.Crear(
                numeroFactura,
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            // Agregar un detalle para que se pueda emitir
            factura.AgregarDetalle(
                Guid.NewGuid(),
                "Producto",
                1,
                100.0m,
                16.0m); // Total: 116.0m
                
            factura.Emitir();
                
            // Configurar el mock del repositorio de facturas
            _facturaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(factura);
                
            // Act
            var facturaConPago = await _servicioFacturacion.RegistrarPagoFacturaAsync(facturaId, pagoId, monto);
                
            // Assert
            facturaConPago.Should().NotBeNull();
            facturaConPago.Estado.Should().Be(EstadoFactura.PagadaParcialmente);
            facturaConPago.TotalPagado.Should().Be(monto);
            
            // Verificar llamadas a repositorios
            _facturaRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task GenerarSiguienteNumeroFacturaAsync_DebeRetornarNumeroDelRepositorio()
        {
            // Arrange
            var numeroFactura = "F-2024-005";
            var prefijo = "F-2024-";
            
            // Configurar el mock del repositorio de facturas
            _facturaRepositoryMock
                .Setup(r => r.ObtenerSiguienteNumeroFacturaAsync(prefijo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(numeroFactura);
                
            // Act
            var numero = await _servicioFacturacion.GenerarSiguienteNumeroFacturaAsync(prefijo);
                
            // Assert
            numero.Should().Be(numeroFactura);
            
            // Verificar llamadas a repositorios
            _facturaRepositoryMock.Verify(
                r => r.ObtenerSiguienteNumeroFacturaAsync(prefijo, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 