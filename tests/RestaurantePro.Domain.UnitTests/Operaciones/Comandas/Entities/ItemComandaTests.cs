namespace RestaurantePro.Domain.UnitTests.Operaciones.Comandas.Entities
{
    public class ItemComandaTests
    {
        [Fact]
        public void Constructor_DatosValidos_DebeCrearItemComandaCorrecta()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var cantidad = 2;
            var precioUnitario = 150.5m;
            var observaciones = "Sin picante";

            // Act
            var itemComanda = new ItemComanda(comandaId, productoId, cantidad, precioUnitario, observaciones);

            // Assert
            itemComanda.Should().NotBeNull();
            itemComanda.Id.Should().NotBe(Guid.Empty);
            itemComanda.ComandaId.Should().Be(comandaId);
            itemComanda.ProductoId.Should().Be(productoId);
            itemComanda.Cantidad.Should().Be(cantidad);
            itemComanda.PrecioUnitario.Should().Be(precioUnitario);
            itemComanda.Subtotal.Should().Be(cantidad * precioUnitario);
            itemComanda.Observaciones.Should().Be(observaciones);
        }

        [Fact]
        public void Constructor_CantidadCero_DebeLanzarArgumentException()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var cantidadInvalida = 0; // Cantidad inválida
            var precioUnitario = 150.5m;

            // Act & Assert
            Action action = () => new ItemComanda(comandaId, productoId, cantidadInvalida, precioUnitario);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*cantidad debe ser mayor que cero*");
        }

        [Fact]
        public void Constructor_CantidadNegativa_DebeLanzarArgumentException()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var cantidadInvalida = -1; // Cantidad inválida
            var precioUnitario = 150.5m;

            // Act & Assert
            Action action = () => new ItemComanda(comandaId, productoId, cantidadInvalida, precioUnitario);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*cantidad debe ser mayor que cero*");
        }

        [Fact]
        public void Constructor_PrecioNegativo_DebeLanzarArgumentException()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var cantidad = 2;
            var precioInvalido = -10m; // Precio inválido

            // Act & Assert
            Action action = () => new ItemComanda(comandaId, productoId, cantidad, precioInvalido);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*precio unitario no puede ser negativo*");
        }

        [Fact]
        public void ActualizarCantidad_CantidadValida_DebeActualizarCantidadYRecalcularSubtotal()
        {
            // Arrange
            var itemComanda = new ItemComanda(Guid.NewGuid(), Guid.NewGuid(), 2, 100m);
            var nuevaCantidad = 5;
            var subtotalEsperado = nuevaCantidad * itemComanda.PrecioUnitario;

            // Act
            itemComanda.ActualizarCantidad(nuevaCantidad);

            // Assert
            itemComanda.Cantidad.Should().Be(nuevaCantidad);
            itemComanda.Subtotal.Should().Be(subtotalEsperado);
        }

        [Fact]
        public void ActualizarCantidad_CantidadInvalida_DebeLanzarArgumentException()
        {
            // Arrange
            var itemComanda = new ItemComanda(Guid.NewGuid(), Guid.NewGuid(), 2, 100m);
            var cantidadInvalida = 0;

            // Act & Assert
            Action action = () => itemComanda.ActualizarCantidad(cantidadInvalida);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*cantidad debe ser mayor que cero*");
        }

        [Fact]
        public void ActualizarPrecioUnitario_PrecioValido_DebeActualizarPrecioYRecalcularSubtotal()
        {
            // Arrange
            var itemComanda = new ItemComanda(Guid.NewGuid(), Guid.NewGuid(), 2, 100m);
            var nuevoPrecio = 120m;
            var subtotalEsperado = itemComanda.Cantidad * nuevoPrecio;

            // Act
            itemComanda.ActualizarPrecioUnitario(nuevoPrecio);

            // Assert
            itemComanda.PrecioUnitario.Should().Be(nuevoPrecio);
            itemComanda.Subtotal.Should().Be(subtotalEsperado);
        }

        [Fact]
        public void ActualizarPrecioUnitario_PrecioInvalido_DebeLanzarArgumentException()
        {
            // Arrange
            var itemComanda = new ItemComanda(Guid.NewGuid(), Guid.NewGuid(), 2, 100m);
            var precioInvalido = -50m;

            // Act & Assert
            Action action = () => itemComanda.ActualizarPrecioUnitario(precioInvalido);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*precio unitario no puede ser negativo*");
        }

        [Fact]
        public void ActualizarObservaciones_DebeActualizarObservacionesCorrectamente()
        {
            // Arrange
            var itemComanda = new ItemComanda(Guid.NewGuid(), Guid.NewGuid(), 2, 100m, "Observación inicial");
            var nuevasObservaciones = "Observación actualizada";

            // Act
            itemComanda.ActualizarObservaciones(nuevasObservaciones);

            // Assert
            itemComanda.Observaciones.Should().Be(nuevasObservaciones);
        }

        [Fact]
        public void ActualizarObservaciones_ObservacionesNulas_DebeAceptarValorNulo()
        {
            // Arrange
            var itemComanda = new ItemComanda(Guid.NewGuid(), Guid.NewGuid(), 2, 100m, "Observación inicial");
            string nuevasObservaciones = null;

            // Act
            itemComanda.ActualizarObservaciones(nuevasObservaciones);

            // Assert
            itemComanda.Observaciones.Should().BeNull();
        }
    }
}
