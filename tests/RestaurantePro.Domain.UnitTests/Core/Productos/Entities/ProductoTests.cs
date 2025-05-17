namespace RestaurantePro.Domain.UnitTests.Core.Productos.Entities
{
    public class ProductoTests
    {
        [Fact]
        public void CrearProducto_DeberiaCrearProductoConValoresCorrectos()
        {
            // Arrange
            var nombre = "Pizza Margarita";
            var descripcion = "Pizza tradicional italiana";
            var precio = new PrecioProducto(12.50m);
            var categoriaId = Guid.NewGuid();

            // Act
            var producto = Producto.Crear(nombre, descripcion, precio, categoriaId);

            // Assert
            producto.Should().NotBeNull();
            producto.Nombre.Should().Be(nombre);
            producto.Descripcion.Should().Be(descripcion);
            producto.Precio.Should().Be(precio);
            producto.CategoriaId.Should().Be(categoriaId);
            producto.EstaActivo.Should().BeTrue();
            producto.DomainEvents.Should().ContainSingle(e => e is ProductoCreado);
        }

        [Fact]
        public void ActualizarProducto_DeberiaActualizarPropiedadesYGenerarEvento()
        {
            // Arrange
            var producto = Producto.Crear(
                "Pizza Margarita",
                "Pizza tradicional italiana",
                new PrecioProducto(12.50m),
                Guid.NewGuid());

            var nuevoNombre = "Pizza Cuatro Quesos";
            var nuevaDescripcion = "Pizza con cuatro tipos de queso";
            var nuevoPrecio = new PrecioProducto(14.99m);

            producto.ClearDomainEvents(); // Limpiar evento de creación

            // Act
            producto.Actualizar(nuevoNombre, nuevaDescripcion, nuevoPrecio);

            // Assert
            producto.Nombre.Should().Be(nuevoNombre);
            producto.Descripcion.Should().Be(nuevaDescripcion);
            producto.Precio.Should().Be(nuevoPrecio);
            producto.DomainEvents.Should().ContainSingle(e => e is ProductoActualizado);
        }

        [Fact]
        public void DesactivarProducto_DeberiaMarcarProductoComoInactivoYGenerarEvento()
        {
            // Arrange
            var producto = Producto.Crear(
                "Pizza Margarita",
                "Pizza tradicional italiana",
                new PrecioProducto(12.50m),
                Guid.NewGuid());

            producto.ClearDomainEvents(); // Limpiar evento de creación

            // Act
            producto.Desactivar();

            // Assert
            producto.EstaActivo.Should().BeFalse();
            producto.DomainEvents.Should().ContainSingle(e => e is ProductoDesactivado);
        }

        [Fact]
        public void ActivarProducto_DeberiaMarcarProductoComoActivoYGenerarEvento()
        {
            // Arrange
            var producto = Producto.Crear(
                "Pizza Margarita",
                "Pizza tradicional italiana",
                new PrecioProducto(12.50m),
                Guid.NewGuid());

            producto.Desactivar();
            producto.ClearDomainEvents(); // Limpiar eventos previos

            // Act
            producto.Activar();

            // Assert
            producto.EstaActivo.Should().BeTrue();
            producto.DomainEvents.Should().ContainSingle(e => e is ProductoActivado);
        }
    }
}

