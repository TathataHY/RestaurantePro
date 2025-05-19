namespace RestaurantePro.Domain.UnitTests.Inventario.Compras.OrdenesCompra.Entities
{
    public class ItemOrdenCompraTests
    {
        [Fact]
        public void RegistrarRecepcionCompleta_DebeEmitirEventoCompletado()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Tomate";
            var cantidad = 10.0m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;

            var item = ItemOrdenCompra.Crear(ordenCompraId, ingredienteId, nombreIngrediente, cantidad, unidadMedida);
            var estadoOrden = EstadoOrdenCompra.Recibida;

            // Act
            item.RegistrarRecepcion(cantidad, estadoOrden); // Recepción completa

            // Assert
            item.EstaCompletoEnRecepcion.Should().BeTrue();
            
            // Verificar que se emitió el evento ItemOrdenCompraCompletado
            item.DomainEvents.Should().Contain(e => e is ItemOrdenCompraCompletado);
            
            // Verificar propiedades del evento
            var evento = item.DomainEvents.OfType<ItemOrdenCompraCompletado>().First();
            evento.ItemId.Should().Be(item.Id);
            evento.OrdenCompraId.Should().Be(ordenCompraId);
            evento.IngredienteId.Should().Be(ingredienteId);
            evento.CantidadRecibida.Should().Be(cantidad);
        }

        [Fact]
        public void RegistrarRecepcionParcial_NoDebeEmitirEventoCompletado()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Tomate";
            var cantidad = 10.0m;
            var cantidadRecibida = 5.0m; // Recepción parcial
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;

            var item = ItemOrdenCompra.Crear(ordenCompraId, ingredienteId, nombreIngrediente, cantidad, unidadMedida);
            var estadoOrden = EstadoOrdenCompra.Recibida;

            // Act
            item.RegistrarRecepcion(cantidadRecibida, estadoOrden);

            // Assert
            item.EstaCompletoEnRecepcion.Should().BeFalse();
            
            // Verificar que NO se emitió el evento ItemOrdenCompraCompletado
            item.DomainEvents.Should().NotContain(e => e is ItemOrdenCompraCompletado);
        }
        
        [Fact]
        public void ValidarInvariantes_ConDatosInvalidos_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Tomate";
            var cantidad = 10.0m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;

            var item = ItemOrdenCompra.Crear(ordenCompraId, ingredienteId, nombreIngrediente, cantidad, unidadMedida);
            var precioUnitario = 5.0m; // Establecemos un precio unitario para el cálculo del subtotal
            
            // Establecemos un precio unitario inicial con la función Actualizar
            item.Actualizar(cantidad, precioUnitario);
            
            // Modificamos el subtotal directamente para crear una inconsistencia
            typeof(ItemOrdenCompra).GetProperty("Subtotal")
                .SetValue(item, item.Subtotal + 100m); // Valor inconsistente con cantidad * precioUnitario
            
            // Act & Assert - Llamamos al método ValidarInvariantes de forma reflectiva
            Action action = () => typeof(ItemOrdenCompra)
                .GetMethod("ValidarInvariantes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(item, null);
                
            action.Should().Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*Inconsistencia en el subtotal*");
        }
    }
} 