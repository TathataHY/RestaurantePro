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
            
            // Accedemos al campo privado usando reflexión para simular un estado inconsistente
            var subtotalField = typeof(ItemOrdenCompra).GetProperty("Subtotal", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            
            var originalSubtotal = (decimal)subtotalField.GetValue(item);
            var invalidSubtotal = originalSubtotal + 100m; // Valor inconsistente
            
            // Simulamos una operación que no mantiene la consistencia interna
            // Esto es sólo para propósitos de prueba, en código real utilizaríamos sólo API pública
            typeof(ItemOrdenCompra).GetProperty("Subtotal")
                .SetValue(item, invalidSubtotal);
            
            // Act & Assert
            Action action = () => item.Actualizar(cantidad, 10m);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Inconsistencia en el subtotal*");
        }
    }
} 