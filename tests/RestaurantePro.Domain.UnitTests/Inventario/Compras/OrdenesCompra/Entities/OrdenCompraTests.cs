namespace RestaurantePro.Domain.UnitTests.Inventario.Compras.OrdenesCompra.Entities
{
    public class OrdenCompraTests
    {
        [Fact]
        public void ValidarInvariantes_ConDatosInvalidos_DebeLanzarExcepcion()
        {
            // Arrange - Crear orden y manipular directamente el estado para simular inconsistencia
            var fechaEmision = DateTime.Now;
            var fechaEntrega = fechaEmision.AddDays(7); // Fecha de entrega 7 días después
            
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                fechaEmision);
                
            // Establecer fecha de entrega estimada válida
            ordenCompra.EstablecerFechaEntrega(fechaEntrega);

            var item = ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Manipular directamente el Total para crear una inconsistencia
            typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetProperty("Total")
                .SetValue(ordenCompra, ordenCompra.Total + 100m); // Valor inconsistente
            
            // Act & Assert - Al intentar validar invariantes explícitamente, debería fallar
            Action action = () => typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetMethod("ValidarInvariantes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(ordenCompra, null);
                
            action.Should().Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*Inconsistencia en el total*");
        }

        [Fact]
        public void ValidarInvariantes_ConFechasIncoherentes_DebeLanzarExcepcion()
        {
            // Arrange
            var fechaEmision = DateTime.Now;
            var fechaEntrega = fechaEmision.AddDays(7); // Fecha de entrega 7 días después
            
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                fechaEmision);
                
            // Establecer fecha de entrega estimada válida
            ordenCompra.EstablecerFechaEntrega(fechaEntrega);

            ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m,
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            ordenCompra.Enviar();

            // Manipular directamente la fecha de envío para crear incoherencia
            typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetProperty("FechaEnvio")
                .SetValue(ordenCompra, fechaEmision.AddDays(-1)); // Fecha anterior a emisión
                
            // Act & Assert - Al intentar validar invariantes explícitamente, debería fallar
            Action action = () => typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetMethod("ValidarInvariantes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(ordenCompra, null);
                
            action.Should().Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*fecha de envío*");
        }
    }
} 