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
            var observaciones = "Observaciones de prueba";

            // Act
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                proveedorId,
                observaciones,
                fechaEmision);

            // Assert
            ordenCompra.Should().NotBeNull();
            ordenCompra.ProveedorId.Should().Be(proveedorId);
            ordenCompra.FechaEmision.Should().Be(fechaEmision);
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

            // Act
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                proveedorId,
                "Observaciones",
                fechaEmision);

            // Assert
            Action action = () => ordenCompra.EstablecerFechaEntrega(fechaEntregaInvalida);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*fecha de entrega*");
        }

        [Fact]
        public void AgregarItem_ConCantidadYPrecioValidos_DebeAgregarYCalcularTotal()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Tomate";
            var cantidad = 10.0m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;

            // Act
            var item = ordenCompra.AgregarItem(ingredienteId, nombreIngrediente, cantidad, unidadMedida);

            // Assert
            ordenCompra.Items.Should().HaveCount(1);
            ordenCompra.Items.Should().Contain(item);

            item.OrdenCompraId.Should().Be(ordenCompra.Id);
            item.IngredienteId.Should().Be(ingredienteId);
            item.Cantidad.Should().Be(cantidad);
            item.NombreIngrediente.Should().Be(nombreIngrediente);

            // Verificar que se generó el evento de dominio
            ordenCompra.DomainEvents.Should().ContainSingle(e => e is ItemOrdenCompraAgregado);
        }

        [Fact]
        public void AgregarItem_ConCantidadNegativa_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var ingredienteId = Guid.NewGuid();
            var cantidadInvalida = -5.0m;

            // Act & Assert
            var action = () => ordenCompra.AgregarItem(ingredienteId, "Tomate", cantidadInvalida, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*cantidad*");
        }

        [Fact]
        public void EliminarItem_ItemExistente_DebeEliminarYRecalcularTotal()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var item1 = ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            var item2 = ordenCompra.AgregarItem(Guid.NewGuid(), "Cebolla", 20.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Act
            ordenCompra.EliminarItem(item1.Id);

            // Assert
            ordenCompra.Items.Should().HaveCount(1);
            ordenCompra.Items.Should().NotContain(item1);
            ordenCompra.Items.Should().Contain(item2);

            // Verificar que se generó el evento de dominio
            ordenCompra.DomainEvents.Should().Contain(e => e is ItemOrdenCompraEliminado);
        }

        [Fact]
        public void EnviarOrdenCompra_CuandoEstaPendiente_DebeCambiarEstadoAEnviada()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Act
            ordenCompra.Enviar();

            // Assert
            ordenCompra.Estado.Should().Be(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Enviada);
            ordenCompra.FechaEnvio.Should().NotBeNull();
            if (ordenCompra.FechaEnvio.HasValue)
            {
                ordenCompra.FechaEnvio.Value.Date.Should().Be(DateTime.Now.Date);
            }

            // Verificar que se generó el evento de dominio
            ordenCompra.DomainEvents.Should().Contain(e => e is OrdenCompraEnviada);
        }

        [Fact]
        public void EnviarOrdenCompra_SinItems_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

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
                "Observaciones",
                DateTime.Now);

            ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
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
                "Observaciones",
                DateTime.Now);

            var motivoCancelacion = "Proveedor no disponible";

            // Act
            ordenCompra.Cancelar(motivoCancelacion);

            // Assert
            ordenCompra.Estado.Should().Be(Domain.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.Cancelada);
            ordenCompra.FechaCancelacion.Should().NotBeNull();
            if (ordenCompra.FechaCancelacion.HasValue)
            {
                ordenCompra.FechaCancelacion.Value.Date.Should().Be(DateTime.Now.Date);
            }
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
                "Observaciones",
                DateTime.Now);

            ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            ordenCompra.Enviar();
            ordenCompra.Recibir(DateTime.Now.AddDays(1));

            // Act & Assert
            var action = () => ordenCompra.Cancelar("Motivo");
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*estado*");
        }

        [Fact]
        public void AgregarItemDuplicado_DebeAumentarCantidadNoCrearNuevoItem()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var ingredienteId = Guid.NewGuid();
            var cantidadInicial = 5.0m;
            var cantidadAdicional = 3.0m;
            var cantidadEsperada = cantidadInicial + cantidadAdicional;

            // Act
            var item1 = ordenCompra.AgregarItem(ingredienteId, "Tomate", cantidadInicial, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            var item2 = ordenCompra.AgregarItem(ingredienteId, "Tomate", cantidadAdicional, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Assert
            ordenCompra.Items.Should().HaveCount(1);
            item1.Should().BeSameAs(item2); // Debe ser el mismo objeto
            item1.Cantidad.Should().Be(cantidadEsperada);
        }

        [Fact]
        public void AgregarYEliminarItems_DebeCalcularTotalCorrectamente()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            // Act
            var item1 = ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            var item2 = ordenCompra.AgregarItem(Guid.NewGuid(), "Cebolla", 5.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            var item3 = ordenCompra.AgregarItem(Guid.NewGuid(), "Ajo", 2.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            ordenCompra.EliminarItem(item2.Id);

            // Assert
            ordenCompra.Items.Should().HaveCount(2);
            ordenCompra.Items.Should().Contain(item1);
            ordenCompra.Items.Should().Contain(item3);
            ordenCompra.Items.Should().NotContain(item2);
        }

        [Fact]
        public void ActualizarItemOrdenCompra_DebeActualizarCantidadYPrecio()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Tomate";
            var cantidadInicial = 10.0m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;

            var item = ordenCompra.AgregarItem(ingredienteId, nombreIngrediente, cantidadInicial, unidadMedida);

            var nuevaCantidad = 15.0m;
            var nuevoPrecioUnitario = 7.5m;
            var nuevoSubtotalEsperado = nuevaCantidad * nuevoPrecioUnitario;

            // Act
            item.Actualizar(nuevaCantidad, nuevoPrecioUnitario);

            // Assert
            item.Cantidad.Should().Be(nuevaCantidad);
            item.PrecioUnitario.Should().Be(nuevoPrecioUnitario);
            item.Subtotal.Should().Be(nuevoSubtotalEsperado);
        }

        [Fact]
        public void ActualizarItemOrdenCompra_ConCantidadInvalida_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var item = ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            var cantidadInvalida = -5.0m;

            // Act & Assert
            var action = () => item.Actualizar(cantidadInvalida, 10.0m);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*cantidad*");
        }

        [Fact]
        public void ActualizarItemOrdenCompra_ConPrecioInvalido_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var item = ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            var precioInvalido = -5.0m;

            // Act & Assert
            var action = () => item.Actualizar(10.0m, precioInvalido);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*precio*");
        }

        [Fact]
        public void RegistrarRecepcionItem_CantidadValida_DebeActualizarCantidadRecibida()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var ingredienteId = Guid.NewGuid();
            var item = ordenCompra.AgregarItem(ingredienteId, "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            ordenCompra.Enviar();
            ordenCompra.Recibir(DateTime.Now.AddDays(1));

            var cantidadRecibida = 8.5m;

            // Act
            item.RegistrarRecepcion(cantidadRecibida, ordenCompra.Estado);

            // Assert
            item.CantidadRecibida.Should().Be(cantidadRecibida);
        }

        [Fact]
        public void RegistrarRecepcionItem_CantidadNegativa_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var ingredienteId = Guid.NewGuid();
            var item = ordenCompra.AgregarItem(ingredienteId, "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            ordenCompra.Enviar();
            ordenCompra.Recibir(DateTime.Now.AddDays(1));

            var cantidadInvalida = -5.0m;

            // Act & Assert
            var action = () => item.RegistrarRecepcion(cantidadInvalida, ordenCompra.Estado);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*cantidad*");
        }

        [Fact]
        public void RegistrarRecepcionItem_CantidadMayorALaSolicitada_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var ingredienteId = Guid.NewGuid();
            var cantidadSolicitada = 10.0m;
            var item = ordenCompra.AgregarItem(ingredienteId, "Tomate", cantidadSolicitada, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            ordenCompra.Enviar();
            ordenCompra.Recibir(DateTime.Now.AddDays(1));

            var cantidadInvalida = cantidadSolicitada + 1.0m;

            // Act & Assert
            var action = () => item.RegistrarRecepcion(cantidadInvalida, ordenCompra.Estado);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*mayor*solicitada*");
        }

        [Fact]
        public void RegistrarRecepcionItem_OrdenNoRecibida_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var ingredienteId = Guid.NewGuid();
            var item = ordenCompra.AgregarItem(ingredienteId, "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            // No marcamos la orden como recibida

            // Act & Assert
            var action = () => item.RegistrarRecepcion(5.0m, ordenCompra.Estado);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*recibida*");
        }

        [Fact]
        public void RegistrarRecepcionCompleta_DebeIndicarItemCompleto()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var ingredienteId = Guid.NewGuid();
            var cantidadSolicitada = 10.0m;
            var item = ordenCompra.AgregarItem(ingredienteId, "Tomate", cantidadSolicitada, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            ordenCompra.Enviar();
            ordenCompra.Recibir(DateTime.Now.AddDays(1));

            // Act
            item.RegistrarRecepcion(cantidadSolicitada, ordenCompra.Estado); // Recepción completa

            // Assert
            item.EstaCompletoEnRecepcion.Should().BeTrue();
        }

        [Fact]
        public void RegistrarRecepcionParcial_NoDebeIndicarItemCompleto()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var ingredienteId = Guid.NewGuid();
            var cantidadSolicitada = 10.0m;
            var cantidadRecibida = 8.0m; // Recepción parcial
            var item = ordenCompra.AgregarItem(ingredienteId, "Tomate", cantidadSolicitada, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            ordenCompra.Enviar();
            ordenCompra.Recibir(DateTime.Now.AddDays(1));

            // Act
            item.RegistrarRecepcion(cantidadRecibida, ordenCompra.Estado);

            // Assert
            item.EstaCompletoEnRecepcion.Should().BeFalse();
        }

        [Fact]
        public void TodosLosItemsRecibidos_CuandoTodosLosItemsEstanCompletos_DebeRetornarTrue()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var item1 = ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            var item2 = ordenCompra.AgregarItem(Guid.NewGuid(), "Cebolla", 5.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            ordenCompra.Enviar();
            ordenCompra.Recibir(DateTime.Now.AddDays(1));

            // Act
            item1.RegistrarRecepcion(10.0m, ordenCompra.Estado); // Recepción completa
            item2.RegistrarRecepcion(5.0m, ordenCompra.Estado); // Recepción completa

            // Assert
            ordenCompra.TodosLosItemsRecibidos.Should().BeTrue();
        }

        [Fact]
        public void TodosLosItemsRecibidos_CuandoAlgunosItemsNoEstanCompletos_DebeRetornarFalse()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var item1 = ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            var item2 = ordenCompra.AgregarItem(Guid.NewGuid(), "Cebolla", 5.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            ordenCompra.Enviar();
            ordenCompra.Recibir(DateTime.Now.AddDays(1));

            // Act
            item1.RegistrarRecepcion(10.0m, ordenCompra.Estado); // Recepción completa
            item2.RegistrarRecepcion(3.0m, ordenCompra.Estado); // Recepción parcial

            // Assert
            ordenCompra.TodosLosItemsRecibidos.Should().BeFalse();
        }

        [Fact]
        public void TodosLosItemsRecibidos_OrdenNoRecibida_DebeRetornarFalse()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            ordenCompra.AgregarItem(Guid.NewGuid(), "Cebolla", 5.0m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            ordenCompra.Enviar();
            // No marcamos la orden como recibida

            // Assert
            ordenCompra.TodosLosItemsRecibidos.Should().BeFalse();
        }
    }
}

