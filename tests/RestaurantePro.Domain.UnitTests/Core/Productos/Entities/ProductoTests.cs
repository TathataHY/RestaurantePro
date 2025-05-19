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

        [Fact]
        public void ActualizarCategoria_ConDatosValidos_DebeActualizarPropiedades()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoriaNombre = "Bebidas";
            var producto = Producto.Crear("Café", "Café negro", new PrecioProducto(50), categoriaId, categoriaNombre);
            
            var nuevaCategoriaId = Guid.NewGuid();
            var nuevoCategoriaNombre = "Cafetería";
            
            // Act
            producto.ActualizarCategoria(nuevaCategoriaId, nuevoCategoriaNombre);
            
            // Assert
            producto.CategoriaId.Should().Be(nuevaCategoriaId);
            producto.CategoriaNombre.Should().Be(nuevoCategoriaNombre);
            
            // Verificar evento de dominio
            var eventos = producto.DomainEvents.Where(e => e is ProductoCambioCategoria);
            eventos.Should().ContainSingle();
            var evento = (ProductoCambioCategoria)eventos.First();
            evento.ProductoId.Should().Be(producto.Id);
            evento.CategoriaId.Should().Be(nuevaCategoriaId);
            evento.CategoriaNombre.Should().Be(nuevoCategoriaNombre);
        }

        [Fact]
        public void ActualizarCategoria_ConCategoriaSinCambios_NoDebeActualizarNiGenerarEvento()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoriaNombre = "Bebidas";
            var producto = Producto.Crear("Café", "Café negro", new PrecioProducto(50), categoriaId, categoriaNombre);
            
            // Limpiar eventos
            var fieldInfo = typeof(EntityBase).GetField("_domainEvents", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo?.SetValue(producto, new List<DomainEvent>());
            
            // Act
            producto.ActualizarCategoria(categoriaId, categoriaNombre);
            
            // Assert
            producto.CategoriaId.Should().Be(categoriaId);
            producto.CategoriaNombre.Should().Be(categoriaNombre);
            
            // No debe haber generado eventos
            producto.DomainEvents.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void ActualizarCategoria_ConNombreInvalido_DebeLanzarExcepcion(string nombreInvalido)
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoriaNombre = "Bebidas";
            var producto = Producto.Crear("Café", "Café negro", new PrecioProducto(50), categoriaId, categoriaNombre);
            
            // Act & Assert
            Action action = () => producto.ActualizarCategoria(Guid.NewGuid(), nombreInvalido);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*categoría*");
        }
    }
}

