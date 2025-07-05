namespace RestaurantePro.Domain.UnitTests.Inventario.Ingredientes.Entities
{
    public class IngredienteTests
    {
        [Fact]
        public void CrearIngrediente_ConDatosValidos_DebeCrearCorrectamente()
        {
            // Arrange
            var nombre = "Tomate";
            var codigo = "TOM-001";
            var descripcion = "Tomate fresco";
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var stockMinimo = 5.0m;
            var stockActual = 0.0m;
            
            // Act
            var ingrediente = Ingrediente.Crear(nombre, codigo, descripcion, unidadMedida, stockMinimo, stockActual);
            
            // Assert
            ingrediente.Should().NotBeNull();
            ingrediente.Nombre.Should().Be(nombre);
            ingrediente.UnidadMedida.Should().Be(unidadMedida);
            ingrediente.StockMinimo.Should().Be(stockMinimo);
            ingrediente.Stock.Should().Be(stockActual);
            ingrediente.EstaActivo.Should().BeTrue();
            ingrediente.Movimientos.Should().BeEmpty();
        }
        
        [Fact]
        public void CrearIngrediente_ConNombreVacio_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = "";
            var codigo = "TOM-001";
            var descripcion = "Tomate fresco";
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var stockMinimo = 5.0m;
            var stockActual = 0.0m;
            
            // Act & Assert
            var action = () => Ingrediente.Crear(nombre, codigo, descripcion, unidadMedida, stockMinimo, stockActual);
            action.Should().Throw<ArgumentException>().WithMessage("*nombre*");
        }
        
        [Fact]
        public void CrearIngrediente_ConStockMinimoNegativo_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = "Tomate";
            var codigo = "TOM-001";
            var descripcion = "Tomate fresco";
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var stockMinimo = -1.0m;
            var stockActual = 0.0m;
            
            // Act & Assert
            var action = () => Ingrediente.Crear(nombre, codigo, descripcion, unidadMedida, stockMinimo, stockActual);
            action.Should().Throw<ArgumentException>().WithMessage("*stock*");
        }
        
        [Fact]
        public void IncrementarStock_DebeCrearMovimientoYActualizarStock()
        {
            // Arrange
            var ingrediente = Ingrediente.Crear("Tomate", "TOM-001", "Tomate fresco", RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 5.0m, 0.0m);
            var cantidadIncremento = 10.0m;
            var motivo = "Compra inicial";
            
            // Act
            var movimiento = ingrediente.IncrementarStock(cantidadIncremento, motivo);
            
            // Assert
            ingrediente.Stock.Should().Be(cantidadIncremento);
            ingrediente.Movimientos.Should().HaveCount(1);
            ingrediente.Movimientos.Should().Contain(movimiento);
            
            movimiento.TipoMovimiento.Should().Be(TipoMovimientoInventario.Ingreso);
            movimiento.Cantidad.Should().Be(cantidadIncremento);
            movimiento.Motivo.Should().Be(motivo);
            movimiento.IngredienteId.Should().Be(ingrediente.Id);
            movimiento.EstaAplicado.Should().BeTrue();
            movimiento.CantidadFinal.Should().Be(cantidadIncremento);
        }
        
        [Fact]
        public void DecrementarStock_DebeCrearMovimientoYActualizarStock()
        {
            // Arrange
            var ingrediente = Ingrediente.Crear("Tomate", "TOM-001", "Tomate fresco", RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 5.0m, 0.0m);
            var stockInicial = 20.0m;
            var cantidadDecremento = 8.0m;
            var stockEsperado = stockInicial - cantidadDecremento;
            var motivoIngreso = "Compra inicial";
            var motivoEgreso = "Consumo en cocina";
            
            // Establecer stock inicial
            ingrediente.IncrementarStock(stockInicial, motivoIngreso);
            
            // Act
            var movimiento = ingrediente.DecrementarStock(cantidadDecremento, motivoEgreso);
            
            // Assert
            ingrediente.Stock.Should().Be(stockEsperado);
            ingrediente.Movimientos.Should().HaveCount(2);
            ingrediente.Movimientos.Should().Contain(movimiento);
            
            movimiento.TipoMovimiento.Should().Be(TipoMovimientoInventario.Egreso);
            movimiento.Cantidad.Should().Be(cantidadDecremento);
            movimiento.Motivo.Should().Be(motivoEgreso);
            movimiento.IngredienteId.Should().Be(ingrediente.Id);
            movimiento.EstaAplicado.Should().BeTrue();
            movimiento.CantidadFinal.Should().Be(stockEsperado);
        }
        
        [Fact]
        public void DecrementarStock_StockInsuficiente_DebeLanzarExcepcion()
        {
            // Arrange
            var ingrediente = Ingrediente.Crear("Tomate", "TOM-001", "Tomate fresco", RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 5.0m, 0.0m);
            var stockInicial = 10.0m;
            var cantidadDecremento = 15.0m;
            
            // Establecer stock inicial
            ingrediente.IncrementarStock(stockInicial, "Compra inicial");
            
            // Act & Assert
            var action = () => ingrediente.DecrementarStock(cantidadDecremento, "Consumo");
            action.Should().Throw<StockInsuficienteException>().WithMessage("*Stock insuficiente*");
            
            // El stock no debe cambiar
            ingrediente.Stock.Should().Be(stockInicial);
            // Solo debe existir un movimiento (el ingreso inicial)
            ingrediente.Movimientos.Should().HaveCount(1);
        }
    }
}


