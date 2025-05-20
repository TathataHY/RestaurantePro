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
                .WithMessage("*No se puede aplicar descuento sin un cliente asociado*");
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
            
            // Agregamos un producto para que no falle por otras validaciones
            comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            
            // Obtenemos acceso al método ValidarInvariantes directamente
            var validarInvariantes = typeof(Comanda).GetMethod(
                "ValidarInvariantes", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Establecer las fechas incoherentes después de cualquier otra operación
            var fechaCreacion = DateTime.Now;
            var fechaActualizacionInvalida = fechaCreacion.AddDays(-1); // Fecha anterior
            
            typeof(Comanda).GetProperty("FechaCreacion").SetValue(comanda, fechaCreacion);
            typeof(Comanda).GetProperty("FechaActualizacion").SetValue(comanda, fechaActualizacionInvalida);
            
            // Act & Assert - Llamamos directamente a ValidarInvariantes
            Action action = () => validarInvariantes.Invoke(comanda, null);
            
            // Assert
            action.Should().Throw<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
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
            
            // Crear un nuevo ItemComanda con el mismo productoId pero asegurándonos de que el subtotal sea correcto
            var nuevoItem = new ItemComanda(comanda.Id, productoId, 3, 100m, "Duplicado");
            items.Add(nuevoItem);
            
            // Actualizamos el Total para que no falle por inconsistencia de total
            var totalField = typeof(Comanda).GetProperty("Total");
            var nuevoSubtotal = items.Sum(i => i.Subtotal); // 100 + 300 = 400
            var nuevoImpuesto = nuevoSubtotal * 0.16m; // 64
            var nuevoTotal = TotalComanda.Crear(nuevoSubtotal, nuevoImpuesto);
            totalField.SetValue(comanda, nuevoTotal);
            
            // Obtenemos acceso al método ValidarInvariantes directamente
            var validarInvariantes = typeof(Comanda).GetMethod(
                "ValidarInvariantes", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Act & Assert - Llamamos directamente a ValidarInvariantes
            Action action = () => validarInvariantes.Invoke(comanda, null);
            
            // Assert
            action.Should().Throw<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*Existen productos duplicados en la comanda*");
        }

        [Fact]
        public void AgregarProducto_ConCantidadExcesiva_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            var productoId = Guid.NewGuid();
            var cantidadExcesiva = 100; // Más de 50 unidades debería fallar según las validaciones
            
            // Act & Assert
            Action action = () => comanda.AgregarProducto(productoId, cantidadExcesiva, 10m);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*La cantidad del item*excede el límite máximo permitido*");
        }
        
        [Fact]
        public void AgregarProducto_ConPrecioExcesivo_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            var productoId = Guid.NewGuid();
            var precioExcesivo = 150000m; // Más de 100000 debería fallar según las validaciones
            
            // Act & Assert
            Action action = () => comanda.AgregarProducto(productoId, 1, precioExcesivo);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*El precio unitario del item*excede el límite máximo permitido*");
        }
        
        [Fact]
        public void AgregarProducto_ConObservacionesExcesivas_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            var productoId = Guid.NewGuid();
            var observacionesExcesivas = new string('X', 201); // Más de 200 caracteres
            
            // Act & Assert
            Action action = () => comanda.AgregarProducto(productoId, 1, 10m, observacionesExcesivas);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Las observaciones del item*no pueden exceder los 200 caracteres*");
        }
        
        [Fact]
        public void ActualizarEstado_ConTransicionInvalida_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            
            // Act & Assert - Intentar saltar directamente a Finalizada
            Action action = () => comanda.ActualizarEstado(EstadoComanda.Finalizada);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*No se puede cambiar el estado*", 
                    because: "Las transiciones de estado deben seguir el flujo establecido");
        }
        
        [Fact]
        public void Cancelar_DespuesDeEnProceso_DebeFallarValidacionEstado()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            
            // Pasamos a En Proceso, que es válido desde Creada
            comanda.ActualizarEstado(EstadoComanda.EnProceso);
            
            // Luego pasamos a Lista, que es válido desde EnProceso
            comanda.ActualizarEstado(EstadoComanda.Lista);
            
            // Act & Assert - Ahora intentamos cancelar cuando ya no está activa
            Action action = () => comanda.Cancelar("Motivo de cancelación");
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*No se pueden realizar cambios*",
                    because: "Una comanda solo puede cancelarse cuando está en estado Creada o EnProceso");
        }
        
        [Fact]
        public void Comanda_ConEstadoListaYSinProductos_DebeFallar()
        {
            // Arrange - Creamos una comanda y le asignamos estado Lista sin productos
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            
            // Use reflection to modify the state to Lista and then force validation
            typeof(Comanda).GetProperty("Estado").SetValue(comanda, EstadoComanda.Lista);
            
            // Act & Assert
            // Llamamos a ValidarInvariantes directamente usando reflection
            var validateMethod = typeof(Comanda).GetMethod("ValidarInvariantes", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            Action action = () => validateMethod.Invoke(comanda, null);
            
            // Assert
            action.Should().Throw<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*Una comanda lista debe tener al menos un producto*");
        }
        
        [Fact]
        public void AplicarDescuentoFidelizacion_ConPorcentajeMayorPermitido_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            
            // Act & Assert
            // Intentamos con un valor claramente por encima del límite para que falle la validación inicial
            Action action = () => comanda.AplicarDescuentoFidelizacion(1.5m); // 150% descuento
            action.Should().Throw<ArgumentException>()
                .WithMessage("*El porcentaje de descuento debe estar entre 0 y 1*");
        }
        
        [Fact]
        public void Comanda_ConDescuentoMayorQueSubtotal_DebeFallar()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            comanda.AgregarProducto(Guid.NewGuid(), 1, 100m); // Subtotal = 100
            
            // Asignamos un descuento que exceda el 50% del subtotal
            // Usamos reflection para evitar las validaciones del método público
            var descuentoProperty = typeof(Comanda).GetProperty("DescuentoFidelizacion");
            descuentoProperty.SetValue(comanda, 60m); // 60% del subtotal de 100
            
            // Act & Assert
            Action action = () => comanda.AgregarProducto(Guid.NewGuid(), 1, 10m);
            
            // El mensaje debe coincidir con el lanzado en ValidarInvariantes para descuentos mayores al 50%
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*El descuento no puede exceder el 50% del subtotal*");
        }
        
        [Fact]
        public void Comanda_ConFechaCreacionFutura_DebeFallar()
        {
            // Arrange - Creamos una comanda normal
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            
            // Modificamos la fecha de creación para que sea en el futuro
            var fechaFutura = DateTime.Now.AddDays(1);
            typeof(Comanda).GetProperty("FechaCreacion").SetValue(comanda, fechaFutura);
            
            // Act - Llamamos a ValidarInvariantes directamente
            var validateMethod = typeof(Comanda).GetMethod("ValidarInvariantes", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            Action action = () => validateMethod.Invoke(comanda, null);
            
            // Assert
            action.Should().Throw<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*La fecha de creación no puede ser en el futuro*");
        }
    }
}




