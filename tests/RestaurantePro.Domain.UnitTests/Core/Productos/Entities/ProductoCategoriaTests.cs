namespace RestaurantePro.Domain.UnitTests.Core.Productos.Entities
{
    public class ProductoCategoriaTests
    {
        [Fact]
        public void Crear_ConParametrosValidos_DebeCrearCategoriaActiva()
        {
            // Arrange
            var nombre = "Bebidas";
            var descripcion = "Categoría para bebidas";
            var orden = 1;

            // Act
            var categoria = ProductoCategoria.Crear(nombre, descripcion, orden);

            // Assert
            categoria.Should().NotBeNull();
            categoria.Id.Should().NotBe(Guid.Empty);
            categoria.Nombre.Should().Be(nombre);
            categoria.Descripcion.Should().Be(descripcion);
            categoria.Orden.Should().Be(orden);
            categoria.EstaActivo.Should().BeTrue();
            
            // Verificar evento de dominio
            categoria.DomainEvents.Should().ContainSingle();
            var evento = categoria.DomainEvents.First();
            evento.Should().BeOfType<ProductoCategoriaCreada>();
            var categoriaCreada = (ProductoCategoriaCreada)evento;
            categoriaCreada.Id.Should().Be(categoria.Id);
            categoriaCreada.Nombre.Should().Be(nombre);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Crear_ConNombreInvalido_DebeLanzarExcepcion(string nombreInvalido)
        {
            // Arrange
            var descripcion = "Descripción válida";
            var orden = 1;

            // Act & Assert
            Action action = () => ProductoCategoria.Crear(nombreInvalido, descripcion, orden);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*nombre*");
        }
        
        [Fact]
        public void Actualizar_ConDatosValidos_DebeActualizarPropiedades()
        {
            // Arrange
            var categoria = ProductoCategoria.Crear("Bebidas", "Categoría para bebidas", 1);
            var nuevoNombre = "Bebidas Alcohólicas";
            var nuevaDescripcion = "Categoría para bebidas con alcohol";
            var nuevoOrden = 2;
            
            // Act
            categoria.Actualizar(nuevoNombre, nuevaDescripcion, nuevoOrden);
            
            // Assert
            categoria.Nombre.Should().Be(nuevoNombre);
            categoria.Descripcion.Should().Be(nuevaDescripcion);
            categoria.Orden.Should().Be(nuevoOrden);
            
            // Verificar evento de dominio
            var eventos = categoria.DomainEvents.Where(e => e is ProductoCategoriaActualizada);
            eventos.Should().ContainSingle();
            var evento = (ProductoCategoriaActualizada)eventos.First();
            evento.Id.Should().Be(categoria.Id);
            evento.Nombre.Should().Be(nuevoNombre);
            evento.Descripcion.Should().Be(nuevaDescripcion);
            evento.Orden.Should().Be(nuevoOrden);
        }
        
        [Fact]
        public void Desactivar_CategoriaActiva_DebeDesactivarYGenerarEvento()
        {
            // Arrange
            var categoria = ProductoCategoria.Crear("Bebidas", "Categoría para bebidas", 1);
            
            // Act
            categoria.Desactivar();
            
            // Assert
            categoria.EstaActivo.Should().BeFalse();
            
            // Verificar evento de dominio
            var eventos = categoria.DomainEvents.Where(e => e is ProductoCategoriaDesactivada);
            eventos.Should().ContainSingle();
            var evento = (ProductoCategoriaDesactivada)eventos.First();
            evento.Id.Should().Be(categoria.Id);
        }
        
        [Fact]
        public void Activar_CategoriaInactiva_DebeActivarYGenerarEvento()
        {
            // Arrange
            var categoria = ProductoCategoria.Crear("Bebidas", "Categoría para bebidas", 1);
            categoria.Desactivar();
            
            // Act
            categoria.Activar();
            
            // Assert
            categoria.EstaActivo.Should().BeTrue();
            
            // Verificar evento de dominio
            var eventos = categoria.DomainEvents.Where(e => e is ProductoCategoriaActivada);
            eventos.Should().ContainSingle();
            var evento = (ProductoCategoriaActivada)eventos.First();
            evento.Id.Should().Be(categoria.Id);
        }
    }
} 