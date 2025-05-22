using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Events;

namespace RestaurantePro.Domain.UnitTests.Comercial.Facturacion.Entities
{
    public class FacturaTests
    {
        [Fact]
        public void CrearFactura_ConDatosValidos_DebeCrearFacturaEnEstadoBorrador()
        {
            // Arrange
            var numeroFactura = "F-2024-001";
            var tipoFactura = TipoFactura.Normal;
            var nombreCliente = "Cliente de Prueba";
            var comandaId = Guid.NewGuid();
            var fechaEmision = DateTime.Now;

            // Act
            var factura = Factura.Crear(
                numeroFactura,
                tipoFactura,
                nombreCliente,
                null,
                null,
                null,
                new List<Guid> { comandaId },
                "Observaciones de prueba",
                fechaEmision);

            // Assert
            factura.Should().NotBeNull();
            factura.NumeroFactura.Should().Be(numeroFactura);
            factura.TipoFactura.Should().Be(tipoFactura);
            factura.NombreCliente.Should().Be(nombreCliente);
            factura.Estado.Should().Be(EstadoFactura.Borrador);
            factura.FechaEmision.Should().Be(fechaEmision);
            factura.ComandasIds.Should().ContainSingle().Which.Should().Be(comandaId);
            factura.Observaciones.Should().Be("Observaciones de prueba");
            factura.Total.Should().Be(0); // Sin detalles aún
            
            // Verificar evento de dominio
            factura.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<FacturaCreada>()
                .Which.NumeroFactura.Should().Be(numeroFactura);
        }

        [Fact]
        public void CrearFactura_TipoFacturaFiscalSinRFC_DebeLanzarExcepcion()
        {
            // Arrange
            var numeroFactura = "F-2024-001";
            var tipoFactura = TipoFactura.Fiscal;
            var nombreCliente = "Cliente Fiscal de Prueba";
            
            // Act & Assert
            Action action = () => Factura.Crear(
                numeroFactura,
                tipoFactura,
                nombreCliente,
                null,
                null, // Sin RFC
                null);
                
            action.Should().Throw<ArgumentException>()
                .WithMessage("*identificador fiscal es obligatorio*")
                .WithParameterName("identificacionFiscal");
        }

        [Fact]
        public void AgregarDetalle_EnFacturaBorrador_DebeAgregarDetalleYCalcularTotales()
        {
            // Arrange
            var factura = Factura.Crear(
                "F-2024-002",
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            var productoId = Guid.NewGuid();
            var descripcion = "Producto de prueba";
            var cantidad = 2.0m;
            var precioUnitario = 100.0m;
            var porcentajeImpuesto = 16.0m;
            var porcentajeDescuento = 10.0m;
            
            // Subtotal esperado: 2 * 100 = 200
            // Impuesto esperado: 200 * 0.16 = 32
            // Descuento esperado: 200 * 0.10 = 20
            // Total esperado: 200 + 32 - 20 = 212
            
            // Act
            var detalle = factura.AgregarDetalle(
                productoId,
                descripcion,
                cantidad,
                precioUnitario,
                porcentajeImpuesto,
                porcentajeDescuento);
                
            // Assert
            factura.Detalles.Should().ContainSingle();
            factura.Subtotal.Should().Be(200.0m);
            factura.TotalImpuestos.Should().Be(32.0m);
            factura.TotalDescuentos.Should().Be(20.0m);
            factura.Total.Should().Be(212.0m);
            
            // Verificar detalle
            detalle.FacturaId.Should().Be(factura.Id);
            detalle.ProductoId.Should().Be(productoId);
            detalle.Descripcion.Should().Be(descripcion);
            detalle.Cantidad.Should().Be(cantidad);
            detalle.PrecioUnitario.Should().Be(precioUnitario);
            detalle.PorcentajeImpuesto.Should().Be(porcentajeImpuesto);
            detalle.PorcentajeDescuento.Should().Be(porcentajeDescuento);
            detalle.Subtotal.Should().Be(200.0m);
            detalle.ImporteImpuesto.Should().Be(32.0m);
            detalle.ImporteDescuento.Should().Be(20.0m);
            detalle.Total.Should().Be(212.0m);
        }

        [Fact]
        public void AgregarDetalle_EnFacturaEmitida_DebeLanzarExcepcion()
        {
            // Arrange
            var factura = Factura.Crear(
                "F-2024-003",
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            // Agregar un detalle para poder emitir la factura
            factura.AgregarDetalle(
                Guid.NewGuid(),
                "Producto inicial",
                1,
                100.0m,
                16.0m);
                
            // Emitir la factura
            factura.Emitir();
            
            // Act & Assert
            Action action = () => factura.AgregarDetalle(
                Guid.NewGuid(),
                "Producto adicional",
                1,
                100.0m,
                16.0m);
                
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Solo se pueden agregar detalles a facturas en estado Borrador*");
        }

        [Fact]
        public void Emitir_FacturaSinDetalles_DebeLanzarExcepcion()
        {
            // Arrange
            var factura = Factura.Crear(
                "F-2024-004",
                TipoFactura.Normal,
                "Cliente de Prueba");
            
            // Act & Assert
            Action action = () => factura.Emitir();
                
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*No se puede emitir una factura sin detalles*");
        }

        [Fact]
        public void Emitir_FacturaConDetalles_DebeEmitirYGenerarEvento()
        {
            // Arrange
            var factura = Factura.Crear(
                "F-2024-005",
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            factura.AgregarDetalle(
                Guid.NewGuid(),
                "Producto",
                1,
                100.0m,
                16.0m);
                
            // Limpiar eventos previos
            factura.ClearDomainEvents();
            
            // Act
            factura.Emitir(null, 30); // 30 días para vencimiento
            
            // Assert
            factura.Estado.Should().Be(EstadoFactura.Emitida);
            factura.FechaVencimiento.Should().NotBeNull();
            factura.FechaVencimiento.Value.Date.Should().Be(DateTime.Now.AddDays(30).Date);
            
            // Verificar evento
            factura.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<FacturaEmitida>();
        }

        [Fact]
        public void Anular_FacturaEmitida_DebeAnularYGenerarEvento()
        {
            // Arrange
            var factura = Factura.Crear(
                "F-2024-006",
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            factura.AgregarDetalle(
                Guid.NewGuid(),
                "Producto",
                1,
                100.0m,
                16.0m);
                
            factura.Emitir();
            
            // Limpiar eventos previos
            factura.ClearDomainEvents();
            
            var motivo = "Datos incorrectos";
            
            // Act
            factura.Anular(motivo);
            
            // Assert
            factura.Estado.Should().Be(EstadoFactura.Anulada);
            factura.MotivoAnulacion.Should().Be(motivo);
            
            // Verificar evento
            factura.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<FacturaAnulada>()
                .Which.Motivo.Should().Be(motivo);
        }

        [Fact]
        public void RegistrarPago_PagoParcial_DebeActualizarEstadoYGenerarEvento()
        {
            // Arrange
            var factura = Factura.Crear(
                "F-2024-007",
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            factura.AgregarDetalle(
                Guid.NewGuid(),
                "Producto",
                2,
                100.0m,
                16.0m); // Total: 232.0m
                
            factura.Emitir();
            
            // Limpiar eventos previos
            factura.ClearDomainEvents();
            
            var pagoId = Guid.NewGuid();
            var montoPago = 100.0m; // Pago parcial
            
            // Act
            factura.RegistrarPago(montoPago, pagoId);
            
            // Assert
            factura.Estado.Should().Be(EstadoFactura.PagadaParcialmente);
            factura.TotalPagado.Should().Be(montoPago);
            factura.FechaPago.Should().BeNull(); // No tiene fecha de pago completo
            
            // Verificar evento
            factura.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<PagoFacturaRegistrado>()
                .Which.Monto.Should().Be(montoPago);
        }

        [Fact]
        public void RegistrarPago_PagoCompleto_DebeActualizarEstadoYGenerarEventos()
        {
            // Arrange
            var factura = Factura.Crear(
                "F-2024-008",
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            factura.AgregarDetalle(
                Guid.NewGuid(),
                "Producto",
                1,
                100.0m,
                16.0m); // Total: 116.0m
                
            factura.Emitir();
            
            // Limpiar eventos previos
            factura.ClearDomainEvents();
            
            var pagoId = Guid.NewGuid();
            var montoPago = 116.0m; // Pago completo
            
            // Act
            factura.RegistrarPago(montoPago, pagoId);
            
            // Assert
            factura.Estado.Should().Be(EstadoFactura.Pagada);
            factura.TotalPagado.Should().Be(montoPago);
            factura.FechaPago.Should().NotBeNull();
            
            // Verificar eventos (debería haber dos: PagoFacturaRegistrado y FacturaPagada)
            factura.DomainEvents.Should().HaveCount(2);
            factura.DomainEvents.Should().ContainItemsAssignableTo<PagoFacturaRegistrado>();
            factura.DomainEvents.Should().ContainItemsAssignableTo<FacturaPagada>();
        }

        [Fact]
        public void RegistrarPago_MontoExcesivo_DebeLanzarExcepcion()
        {
            // Arrange
            var factura = Factura.Crear(
                "F-2024-009",
                TipoFactura.Normal,
                "Cliente de Prueba");
                
            factura.AgregarDetalle(
                Guid.NewGuid(),
                "Producto",
                1,
                100.0m,
                16.0m); // Total: 116.0m
                
            factura.Emitir();
            
            var pagoId = Guid.NewGuid();
            var montoPago = 200.0m; // Más que el total
            
            // Act & Assert
            Action action = () => factura.RegistrarPago(montoPago, pagoId);
                
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*El pago excede el total pendiente*");
        }
    }
} 