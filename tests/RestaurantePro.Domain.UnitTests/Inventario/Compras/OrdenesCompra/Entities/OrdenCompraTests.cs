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
        
        [Fact]
        public void ValidarInvariantes_ConCantidadNegativa_DebeLanzarExcepcion()
        {
            // Arrange
            var fechaEmision = DateTime.Now;
            var fechaEntrega = fechaEmision.AddDays(7);
            
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                fechaEmision);
                
            ordenCompra.EstablecerFechaEntrega(fechaEntrega);

            var item = ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m,
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            // Manipular directamente la cantidad para hacerla negativa
            var itemField = typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetField("_items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            var items = (List<Domain.Inventario.Compras.OrdenesCompra.Entities.ItemOrdenCompra>)itemField.GetValue(ordenCompra);
            typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.ItemOrdenCompra)
                .GetProperty("Cantidad")
                .SetValue(items[0], -5.0m); // Cantidad negativa
                
            // Act & Assert
            Action action = () => typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetMethod("ValidarInvariantes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(ordenCompra, null);
                
            action.Should().Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*cantidad inválida*");
        }
        
        [Fact]
        public void ValidarInvariantes_ConCantidadExcesiva_DebeLanzarExcepcion()
        {
            // Arrange
            var fechaEmision = DateTime.Now;
            var fechaEntrega = fechaEmision.AddDays(7);
            
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones",
                fechaEmision);
                
            ordenCompra.EstablecerFechaEntrega(fechaEntrega);

            var item = ordenCompra.AgregarItem(Guid.NewGuid(), "Tomate", 10.0m,
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            // Manipular directamente la cantidad para hacerla excesiva
            var itemField = typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetField("_items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            var items = (List<Domain.Inventario.Compras.OrdenesCompra.Entities.ItemOrdenCompra>)itemField.GetValue(ordenCompra);
            typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.ItemOrdenCompra)
                .GetProperty("Cantidad")
                .SetValue(items[0], 2000.0m); // Cantidad excesiva
                
            // Act & Assert
            Action action = () => typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetMethod("ValidarInvariantes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(ordenCompra, null);
                
            action.Should().Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*excede el límite máximo permitido*");
        }
        
        [Fact]
        public void ValidarInvariantes_ConObservacionesExcesivas_DebeLanzarExcepcion()
        {
            // Arrange
            var fechaEmision = DateTime.Now;
            var fechaEntrega = fechaEmision.AddDays(7); // Fecha de entrega 7 días después
            
            var ordenCompra = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                Guid.NewGuid(),
                "Observaciones cortas",
                fechaEmision);
                
            // Establecer fecha de entrega estimada válida
            ordenCompra.EstablecerFechaEntrega(fechaEntrega);
                
            // Manipular directamente las observaciones para hacerlas excesivas
            var observacionesExcesivas = new string('X', 501); // 501 caracteres
            typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetProperty("Observaciones")
                .SetValue(ordenCompra, observacionesExcesivas);
                
            // Act & Assert
            Action action = () => typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetMethod("ValidarInvariantes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(ordenCompra, null);
                
            action.Should().Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*observaciones no pueden exceder*");
        }
        
        [Fact]
        public void ValidarInvariantes_ConEstadoYFechaIncoherentes_DebeLanzarExcepcion()
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
            
            // Manipular directamente para crear incoherencia: 
            // Estado Pendiente pero con FechaCancelacion
            typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetProperty("FechaCancelacion")
                .SetValue(ordenCompra, DateTime.Now);
                
            // Act & Assert
            Action action = () => typeof(Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra)
                .GetMethod("ValidarInvariantes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(ordenCompra, null);
                
            action.Should().Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*fecha de cancelación debe estar en estado Cancelada*");
        }
    }
} 