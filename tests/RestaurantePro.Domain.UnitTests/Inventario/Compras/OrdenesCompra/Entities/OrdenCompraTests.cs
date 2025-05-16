namespace RestaurantePro.Domain.UnitTests.Inventario.Compras.OrdenesCompra.Entities
{
    public class OrdenCompraTests
    {
        [Fact]
        public void CrearOrdenCompra_ConDatosValidos_DebeCrearCorrectamente()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var fechaEmision = DateTime.Now;
            var fechaEntregaEstimada = DateTime.Now.AddDays(3);
            var observaciones = "Observaciones de prueba";
            
            // Act
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                proveedorId, 
                fechaEmision, 
                fechaEntregaEstimada,
                observaciones);
            
            // Assert
            ordenCompra.Should().NotBeNull();
            ordenCompra.ProveedorId.Should().Be(proveedorId);
            ordenCompra.FechaEmision.Should().Be(fechaEmision);
            ordenCompra.FechaEntregaEstimada.Should().Be(fechaEntregaEstimada);
            ordenCompra.Observaciones.Should().Be(observaciones);
            ordenCompra.Estado.Should().Be(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Pendiente);
            ordenCompra.Items.Should().BeEmpty();
            ordenCompra.Total.Should().Be(0);
            
            // Verificar que se generó el evento de dominio
            ordenCompra.DomainEvents.Should().ContainSingle(e => e is OrdenCompraCreada);
            var evento = ordenCompra.DomainEvents.OfType<OrdenCompraCreada>().First();
            evento.OrdenCompraId.Should().Be(ordenCompra.Id);
            evento.ProveedorId.Should().Be(proveedorId);
        }
        
        [Fact]
        public void CrearOrdenCompra_ConFechaEntregaAnteriorAEmision_DebeLanzarExcepcion()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var fechaEmision = DateTime.Now;
            var fechaEntregaInvalida = fechaEmision.AddDays(-1); // Fecha anterior a emisión
            
            // Act & Assert
            var action = () => Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                proveedorId, 
                fechaEmision, 
                fechaEntregaInvalida,
                "Observaciones");
                
            action.Should().Throw<ArgumentException>()
                .WithMessage("*fecha de entrega*");
        }
        
        [Fact]
        public void AgregarItem_ConCantidadYPrecioValidos_DebeAgregarYCalcularTotal()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now.AddDays(3),
                "Observaciones");
                
            var ingredienteId = Guid.NewGuid();
            var cantidad = 10.0m;
            var precioUnitario = 5.5m;
            var totalEsperado = cantidad * precioUnitario;
            
            // Act
            var item = ordenCompra.AgregarItem(ingredienteId, cantidad, precioUnitario);
            
            // Assert
            ordenCompra.Items.Should().HaveCount(1);
            ordenCompra.Items.Should().Contain(item);
            ordenCompra.Total.Should().Be(totalEsperado);
            
            item.OrdenCompraId.Should().Be(ordenCompra.Id);
            item.IngredienteId.Should().Be(ingredienteId);
            item.Cantidad.Should().Be(cantidad);
            item.PrecioUnitario.Should().Be(precioUnitario);
            item.Subtotal.Should().Be(totalEsperado);
            
            // Verificar que se generó el evento de dominio
            ordenCompra.DomainEvents.Should().Contain(e => e is ItemOrdenCompraAgregado);
        }
        
        [Fact]
        public void AgregarItem_ConCantidadNegativa_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now.AddDays(3),
                "Observaciones");
                
            var ingredienteId = Guid.NewGuid();
            var cantidadInvalida = -5.0m;
            var precioUnitario = 10.0m;
            
            // Act & Assert
            var action = () => ordenCompra.AgregarItem(ingredienteId, cantidadInvalida, precioUnitario);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*cantidad*");
        }
        
        [Fact]
        public void AgregarItem_ConPrecioNegativo_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now.AddDays(3),
                "Observaciones");
                
            var ingredienteId = Guid.NewGuid();
            var cantidad = 5.0m;
            var precioInvalido = -10.0m;
            
            // Act & Assert
            var action = () => ordenCompra.AgregarItem(ingredienteId, cantidad, precioInvalido);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*precio*");
        }
        
        [Fact]
        public void EliminarItem_ItemExistente_DebeEliminarYRecalcularTotal()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now.AddDays(3),
                "Observaciones");
                
            var item1 = ordenCompra.AgregarItem(Guid.NewGuid(), 10.0m, 5.0m); // Subtotal: 50
            var item2 = ordenCompra.AgregarItem(Guid.NewGuid(), 20.0m, 2.0m); // Subtotal: 40
            
            // Act
            ordenCompra.EliminarItem(item1.Id);
            
            // Assert
            ordenCompra.Items.Should().HaveCount(1);
            ordenCompra.Items.Should().NotContain(item1);
            ordenCompra.Items.Should().Contain(item2);
            ordenCompra.Total.Should().Be(40.0m);
            
            // Verificar que se generó el evento de dominio
            ordenCompra.DomainEvents.Should().Contain(e => e is ItemOrdenCompraEliminado);
        }
        
        [Fact]
        public void EnviarOrdenCompra_CuandoEstaPendiente_DebeCambiarEstadoAEnviada()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now.AddDays(3),
                "Observaciones");
                
            ordenCompra.AgregarItem(Guid.NewGuid(), 10.0m, 5.0m);
            
            // Act
            ordenCompra.Enviar();
            
            // Assert
            ordenCompra.Estado.Should().Be(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Enviada);
            ordenCompra.FechaEnvio.Should().NotBeNull();
            ordenCompra.FechaEnvio.Value.Date.Should().Be(DateTime.Now.Date);
            
            // Verificar que se generó el evento de dominio
            ordenCompra.DomainEvents.Should().Contain(e => e is OrdenCompraEnviada);
        }
        
        [Fact]
        public void EnviarOrdenCompra_SinItems_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now.AddDays(3),
                "Observaciones");
                
            // Act & Assert
            var action = () => ordenCompra.Enviar();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*sin items*");
        }
        
        [Fact]
        public void RecibirOrdenCompra_CuandoEstaEnviada_DebeCambiarEstadoARecibida()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now.AddDays(3),
                "Observaciones");
                
            ordenCompra.AgregarItem(Guid.NewGuid(), 10.0m, 5.0m);
            ordenCompra.Enviar();
            
            var fechaRecepcion = DateTime.Now.AddDays(2);
            var observacionesRecepcion = "Todo correcto";
            
            // Act
            ordenCompra.Recibir(fechaRecepcion, observacionesRecepcion);
            
            // Assert
            ordenCompra.Estado.Should().Be(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Recibida);
            ordenCompra.FechaRecepcion.Should().Be(fechaRecepcion);
            ordenCompra.ObservacionesRecepcion.Should().Be(observacionesRecepcion);
            
            // Verificar que se generó el evento de dominio
            ordenCompra.DomainEvents.Should().Contain(e => e is OrdenCompraRecibida);
        }
        
        [Fact]
        public void CancelarOrdenCompra_CuandoEstaPendiente_DebeCambiarEstadoACancelada()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now.AddDays(3),
                "Observaciones");
                
            var motivoCancelacion = "Proveedor no disponible";
            
            // Act
            ordenCompra.Cancelar(motivoCancelacion);
            
            // Assert
            ordenCompra.Estado.Should().Be(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Cancelada);
            ordenCompra.FechaCancelacion.Should().NotBeNull();
            ordenCompra.FechaCancelacion.Value.Date.Should().Be(DateTime.Now.Date);
            ordenCompra.MotivoCancelacion.Should().Be(motivoCancelacion);
            
            // Verificar que se generó el evento de dominio
            ordenCompra.DomainEvents.Should().Contain(e => e is OrdenCompraCancelada);
        }
        
        [Fact]
        public void CancelarOrdenCompra_CuandoEstaRecibida_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now.AddDays(3),
                "Observaciones");
                
            ordenCompra.AgregarItem(Guid.NewGuid(), 10.0m, 5.0m);
            ordenCompra.Enviar();
            ordenCompra.Recibir(DateTime.Now.AddDays(2), "Recibido correctamente");
            
            // Act & Assert
            var action = () => ordenCompra.Cancelar("Motivo");
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*no puede cancelarse*");
        }
    }
} 
