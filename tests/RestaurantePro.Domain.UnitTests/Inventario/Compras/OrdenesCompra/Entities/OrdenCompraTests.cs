namespace RestaurantePro.Domain.UnitTests.Inventario.Compras.OrdenesCompra.Entities
{
    public class OrdenCompraTests
    {
        [Fact]
        public void ValidarInvariantes_ConDatosInvalidos_DebeLanzarExcepcion()
        {
            // Arrange - Crear orden y manipular directamente el estado para simular inconsistencia
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            var item = ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Manipular directamente el Total para crear una inconsistencia
            typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetProperty("Total")
                .SetValue(ordenCompra, ordenCompra.Total + 100m); // Valor inconsistente
            
            // Act & Assert - Al intentar enviar, se llamará a ValidarInvariantes y debería fallar
            Action action = () => ordenCompra.Enviar();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Inconsistencia en el total*");
        }

        [Fact]
        public void ValidarInvariantes_ConFechasIncoherentes_DebeLanzarExcepcion()
        {
            // Arrange
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                DateTime.Now);

            ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m,
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            ordenCompra.Enviar();

            // Manipular directamente la fecha de envío para crear incoherencia
            typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetProperty("FechaEnvio")
                .SetValue(ordenCompra, ordenCompra.FechaEmision.AddDays(-1)); // Fecha anterior a emisión
                
            // Act & Assert - Al intentar recibir, se llamará a ValidarInvariantes y debería fallar
            Action action = () => ordenCompra.Recibir(DateTime.Now);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*fecha de envío*");
        }
    }
} 