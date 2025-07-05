namespace RestaurantePro.Domain.UnitTests.Inventario.Ingredientes.Movimientos.Entities
{
    public class MovimientoInventarioTests
    {
        [Fact]
        public void CrearMovimientoIngreso_ConDatosValidos_DebeCrearCorrectamente()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var cantidad = 10.5m;
            var fecha = DateTime.Now;
            var motivo = "Compra de ingredientes";
            var tipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso;
            
            // Act
            var movimiento = MovimientoInventario.CrearIngreso(ingredienteId, cantidad, motivo, fecha);
            
            // Assert
            movimiento.Should().NotBeNull();
            movimiento.IngredienteId.Should().Be(ingredienteId);
            movimiento.Cantidad.Should().Be(cantidad);
            movimiento.Fecha.Should().BeCloseTo(fecha, TimeSpan.FromSeconds(1));
            movimiento.Motivo.Should().Be(motivo);
            movimiento.TipoMovimiento.Should().Be(tipoMovimiento);
            movimiento.CantidadFinal.Should().BeNull(); // No se ha aplicado todavía
        }
        
        [Fact]
        public void CrearMovimientoEgreso_ConDatosValidos_DebeCrearCorrectamente()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var cantidad = 5.25m;
            var fecha = DateTime.Now;
            var motivo = "Consumo en cocina";
            var tipoMovimiento = Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Egreso;
            
            // Act
            var movimiento = MovimientoInventario.CrearEgreso(ingredienteId, cantidad, motivo, fecha);
            
            // Assert
            movimiento.Should().NotBeNull();
            movimiento.IngredienteId.Should().Be(ingredienteId);
            movimiento.Cantidad.Should().Be(cantidad);
            movimiento.Fecha.Should().BeCloseTo(fecha, TimeSpan.FromSeconds(1));
            movimiento.Motivo.Should().Be(motivo);
            movimiento.TipoMovimiento.Should().Be(tipoMovimiento);
            movimiento.CantidadFinal.Should().BeNull(); // No se ha aplicado todavía
        }
        
        [Fact]
        public void CrearMovimientoIngreso_ConCantidadNegativa_DebeLanzarExcepcion()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var cantidad = -5.0m;
            var motivo = "Compra de ingredientes";
            
            // Act & Assert
            var action = () => MovimientoInventario.CrearIngreso(ingredienteId, cantidad, motivo);
            action.Should().Throw<ArgumentException>().WithMessage("*cantidad*");
        }
        
        [Fact]
        public void CrearMovimientoEgreso_ConCantidadNegativa_DebeLanzarExcepcion()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var cantidad = -2.0m;
            var motivo = "Consumo en cocina";
            
            // Act & Assert
            var action = () => MovimientoInventario.CrearEgreso(ingredienteId, cantidad, motivo);
            action.Should().Throw<ArgumentException>().WithMessage("*cantidad*");
        }
        
        [Fact]
        public void CrearMovimiento_ConMotivoVacio_DebeLanzarExcepcion()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var cantidad = 10.0m;
            var motivo = "";
            
            // Act & Assert
            var action = () => MovimientoInventario.CrearIngreso(ingredienteId, cantidad, motivo);
            action.Should().Throw<ArgumentException>().WithMessage("*motivo*");
        }
        
        [Fact]
        public void AplicarMovimientoIngreso_ActualizaCantidadFinal()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var cantidad = 10.0m;
            var stockActual = 5.0m;
            var cantidadFinalEsperada = 15.0m;
            var movimiento = MovimientoInventario.CrearIngreso(ingredienteId, cantidad, "Compra");
            
            // Act
            movimiento.Aplicar(stockActual);
            
            // Assert
            movimiento.CantidadFinal.Should().Be(cantidadFinalEsperada);
            movimiento.EstaAplicado.Should().BeTrue();
        }
        
        [Fact]
        public void AplicarMovimientoEgreso_ActualizaCantidadFinal()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var cantidad = 3.0m;
            var stockActual = 10.0m;
            var cantidadFinalEsperada = 7.0m;
            var movimiento = MovimientoInventario.CrearEgreso(ingredienteId, cantidad, "Consumo");
            
            // Act
            movimiento.Aplicar(stockActual);
            
            // Assert
            movimiento.CantidadFinal.Should().Be(cantidadFinalEsperada);
            movimiento.EstaAplicado.Should().BeTrue();
        }
        
        [Fact]
        public void AplicarMovimientoEgreso_CuandoStockInsuficiente_DebeLanzarExcepcion()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var cantidad = 10.0m;
            var stockActual = 5.0m;
            var movimiento = MovimientoInventario.CrearEgreso(ingredienteId, cantidad, "Consumo");
            
            // Act & Assert
            var action = () => movimiento.Aplicar(stockActual);
            action.Should().Throw<InvalidOperationException>().WithMessage("No hay stock suficiente para completar el movimiento");
        }
        
        [Fact]
        public void AplicarMovimiento_CuandoYaFueAplicado_DebeLanzarExcepcion()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var cantidad = 5.0m;
            var stockActual = 10.0m;
            var movimiento = MovimientoInventario.CrearIngreso(ingredienteId, cantidad, "Compra");
            
            // Aplicar por primera vez
            movimiento.Aplicar(stockActual);
            
            // Act & Assert
            var action = () => movimiento.Aplicar(15.0m);
            action.Should().Throw<InvalidOperationException>().WithMessage("El movimiento ya fue aplicado");
        }
    }
}

