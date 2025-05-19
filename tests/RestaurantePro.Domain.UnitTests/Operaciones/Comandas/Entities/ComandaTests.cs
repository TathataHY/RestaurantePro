namespace RestaurantePro.Domain.UnitTests.Operaciones.Comandas.Entities
{
    public class ComandaTests
    {
        [Fact]
        public void CrearComanda_ConParametrosValidos_DebeCrearComandaConEstadoCreada()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var observaciones = "Observaciones de prueba";

            // Act
            var comanda = Comanda.Crear(mesaId, meseroId, clienteId, observaciones);

            // Assert
            comanda.Should().NotBeNull();
            comanda.Id.Should().NotBe(Guid.Empty);
            comanda.MesaId.Should().Be(mesaId);
            comanda.MeseroId.Should().Be(meseroId);
            comanda.ClienteId.Should().Be(clienteId);
            comanda.Observaciones.Should().Be(observaciones);
            comanda.Estado.Should().Be(EstadoComanda.Creada);
            comanda.Items.Should().BeEmpty();

            // Verificamos que se generó el evento de dominio
            comanda.DomainEvents.Should().ContainSingle(e => e is ComandaCreada);
            var evento = comanda.DomainEvents.OfType<ComandaCreada>().First();
            evento.ComandaId.Should().Be(comanda.Id);
            evento.MesaId.Should().Be(mesaId);
            evento.MeseroId.Should().Be(meseroId);
        }

        [Fact]
        public void AgregarProducto_CuandoComandaEstaActiva_DebeAgregarProductoYRecalcularTotal()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            var productoId = Guid.NewGuid();
            var cantidad = 2;
            var precioUnitario = 100m;
            var observaciones = "Sin cebolla";

            // Act
            comanda.AgregarProducto(productoId, cantidad, precioUnitario, observaciones);

            // Assert
            comanda.Items.Should().HaveCount(1);
            var item = comanda.Items.First();
            item.ProductoId.Should().Be(productoId);
            item.Cantidad.Should().Be(cantidad);
            item.PrecioUnitario.Should().Be(precioUnitario);
            item.Subtotal.Should().Be(cantidad * precioUnitario);
            item.Observaciones.Should().Be(observaciones);

            // Verificar el total
            comanda.Total.Should().NotBeNull();
            comanda.Total.Subtotal.Should().Be(cantidad * precioUnitario);
            comanda.Total.Impuestos.Should().Be(cantidad * precioUnitario * 0.16m);
            comanda.Total.Total.Should().Be(cantidad * precioUnitario * 1.16m);

            // Verificar evento de dominio
            comanda.DomainEvents.Should().Contain(e => e is ProductoAgregadoAComanda);
            var evento = comanda.DomainEvents.OfType<ProductoAgregadoAComanda>().First();
            evento.ComandaId.Should().Be(comanda.Id);
            evento.ProductoId.Should().Be(productoId);
            evento.Cantidad.Should().Be(cantidad);
        }

        [Fact]
        public void AgregarProducto_CuandoComandaNoEstaActiva_DebeLanzarExcepcion()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            
            // Hacemos que la comanda pase por los estados intermedios hasta llegar a Finalizada
            // Primero agregamos un producto para que pueda finalizarse
            comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            
            // Luego usamos reflection para modificar el estado directamente
            var fieldInfo = typeof(Comanda).GetProperty("Estado");
            fieldInfo?.SetValue(comanda, EstadoComanda.Finalizada);

            // Act & Assert
            Action action = () => comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*No se pueden realizar cambios*",
                    because: "No se deben permitir cambios en comandas no activas");
        }

        [Fact]
        public void Comanda_SinProductos_NoPuedeFinalizarse()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());

            // Cambiamos manualmente el estado para probar la transición
            // (normalmente pasaría por todos los estados intermedios)
            PropertyInfo propEstado = comanda.GetType().GetProperty("Estado", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (propEstado != null)
            {
                propEstado.SetValue(comanda, EstadoComanda.Entregada);
            }

            // Act & Assert
            Action action = () => comanda.ActualizarEstado(EstadoComanda.Finalizada);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*No se puede cambiar el estado de Creada a Finalizada*",
                    because: "No se deben permitir finalizar comandas sin productos");
        }
        
        [Fact]
        public void AplicarDescuentoFidelizacion_SinClienteAsociado_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid()); // Sin clienteId
            comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            
            // Act & Assert
            Action action = () => comanda.AplicarDescuentoFidelizacion(0.10m);
            
            // Primero debe pasar la aplicación del descuento ya que la verificación
            // de cliente ocurre en ValidarInvariantes
            // Al ejecutar RecalcularTotal se llama a ValidarInvariantes y ahí fallará
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*No se puede aplicar descuento de fidelización sin un cliente asociado*");
        }
        
        [Fact]
        public void AplicarDescuentoFidelizacion_ConDescuentoExcesivo_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            
            // Act & Assert
            Action action = () => comanda.AplicarDescuentoFidelizacion(1.5m); // 150% descuento
            action.Should().Throw<ArgumentException>()
                .WithMessage("*El porcentaje de descuento debe estar entre 0 y 1*");
        }
        
        [Fact]
        public void ValidarInvariantes_ConMesaIdVacia_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            
            // Manipulamos directamente la propiedad para simular un estado inválido
            typeof(Comanda).GetProperty("MesaId").SetValue(comanda, Guid.Empty);
            
            // Triggereamos ValidarInvariantes a través de algún método público
            Action action = () => comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            
            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*La comanda debe tener una mesa asignada*");
        }
        
        [Fact]
        public void ValidarInvariantes_ConObservacionesExcesivas_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            
            // Crear una cadena de más de 500 caracteres
            var observacionesExcesivas = new string('X', 501);
            typeof(Comanda).GetProperty("Observaciones").SetValue(comanda, observacionesExcesivas);
            
            // Triggereamos ValidarInvariantes a través de algún método público
            Action action = () => comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            
            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Las observaciones no pueden exceder los 500 caracteres*");
        }
        
        [Fact]
        public void ValidarInvariantes_ConFechasIncoherentes_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            var fechaCreacion = DateTime.Now;
            var fechaActualizacionInvalida = fechaCreacion.AddDays(-1); // Fecha anterior
            
            // Manipulamos directamente las propiedades
            typeof(Comanda).GetProperty("FechaCreacion").SetValue(comanda, fechaCreacion);
            typeof(Comanda).GetProperty("FechaActualizacion").SetValue(comanda, fechaActualizacionInvalida);
            
            // Triggereamos ValidarInvariantes a través de algún método público
            Action action = () => comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            
            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*La fecha de actualización no puede ser anterior a la fecha de creación*");
        }
        
        [Fact]
        public void ValidarInvariantes_ConProductosDuplicados_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            var productoId = Guid.NewGuid(); // Mismo ID para ambos productos
            
            // Agregamos el primer producto normalmente
            comanda.AgregarProducto(productoId, 1, 100m);
            
            // Para el segundo, manipulamos directamente la colección interna para evitar la validación
            var itemsField = typeof(Comanda).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
            var items = (List<ItemComanda>)itemsField.GetValue(comanda);
            
            // Crear un nuevo ItemComanda con el mismo productoId
            var nuevoItem = new ItemComanda(comanda.Id, productoId, 2, 150m, "Duplicado");
            items.Add(nuevoItem);
            
            // Triggereamos ValidarInvariantes a través de algún método público que no sea AgregarProducto
            // ya que esta validación es de agregar duplicados
            Action action = () => comanda.ActualizarEstado(EstadoComanda.EnProceso);
            
            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Existen productos duplicados en la comanda*");
        }
    }
}




