#nullable disable
namespace RestaurantePro.Domain.UnitTests.Core.Productos.Builders
{
    /// <summary>
    /// Pruebas unitarias para ProductoBuilder - Validando patrón Builder con Result/Notification
    /// </summary>
    public class ProductoBuilderTests
    {
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly Mock<ILogger<ProductoBuilder>> _loggerMock;
        private readonly ProductoBuilder _builder;

        public ProductoBuilderTests()
        {
            _notificationManagerMock = new Mock<INotificationManager>();
            _loggerMock = new Mock<ILogger<ProductoBuilder>>();
            _builder = new ProductoBuilder(_notificationManagerMock.Object, _loggerMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ConParametrosValidos_DebeCrearBuilder()
        {
            // Act & Assert
            _builder.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ConNotificationManagerNulo_DebeLanzarExcepcion()
        {
            // Act & Assert
            var act = () => new ProductoBuilder(null, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("notificationManager");
        }

        [Fact]
        public void Constructor_ConLoggerNulo_DebeLanzarExcepcion()
        {
            // Act & Assert
            var act = () => new ProductoBuilder(_notificationManagerMock.Object, null);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        #endregion

        #region ConNombre Tests

        [Fact]
        public void ConNombre_ConNombreValido_DebeEstablecerNombre()
        {
            // Arrange
            var nombre = "Pizza Margarita";

            // Act
            var resultado = _builder.ConNombre(nombre);

            // Assert
            resultado.Should().Be(_builder); // Fluent interface
            // No debe agregar errores
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConNombre_ConNombreVacio_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConNombre("");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El nombre del producto no puede estar vacío", "Nombre", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConNombre_ConNombreMuyLargo_DebeAgregarError()
        {
            // Arrange
            var nombreLargo = new string('A', 101); // 101 caracteres

            // Act
            var resultado = _builder.ConNombre(nombreLargo);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El nombre del producto no puede exceder 100 caracteres", "Nombre", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConNombre_ConEspaciosEnBlanco_DebeRecortarEspacios()
        {
            // Arrange
            var nombreConEspacios = "  Pizza Margarita  ";

            // Act
            _builder.ConNombre(nombreConEspacios);

            // Assert
            // Verificar que se logueó el nombre sin espacios
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Pizza Margarita")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region ConDescripcion Tests

        [Fact]
        public void ConDescripcion_ConDescripcionValida_DebeEstablecerDescripcion()
        {
            // Arrange
            var descripcion = "Pizza tradicional italiana con tomate y mozzarella";

            // Act
            var resultado = _builder.ConDescripcion(descripcion);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConDescripcion_ConDescripcionVacia_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConDescripcion("");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("La descripción del producto no puede estar vacía", "Descripcion", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConDescripcion_ConDescripcionMuyLarga_DebeAgregarError()
        {
            // Arrange
            var descripcionLarga = new string('A', 501); // 501 caracteres

            // Act
            var resultado = _builder.ConDescripcion(descripcionLarga);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("La descripción del producto no puede exceder 500 caracteres", "Descripcion", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConPrecio Tests

        [Fact]
        public void ConPrecio_ConPrecioValido_DebeEstablecerPrecio()
        {
            // Arrange
            var precio = 15.99m;

            // Act
            var resultado = _builder.ConPrecio(precio);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConPrecio_ConPrecioCero_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConPrecio(0);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El precio del producto debe ser mayor que cero", "Precio", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConPrecio_ConPrecioNegativo_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConPrecio(-5.99m);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El precio del producto debe ser mayor que cero", "Precio", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConPrecio_ConPrecioMuyAlto_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConPrecio(1000001m);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El precio del producto no puede exceder $1,000,000", "Precio", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region EnCategoria Tests

        [Fact]
        public void EnCategoria_ConIdValido_DebeEstablecerCategoria()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoriaNombre = "Pizzas";

            // Act
            var resultado = _builder.EnCategoria(categoriaId, categoriaNombre);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void EnCategoria_ConIdVacio_DebeAgregarError()
        {
            // Act
            var resultado = _builder.EnCategoria(Guid.Empty);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El ID de categoría no puede estar vacío", "CategoriaId", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void EnCategoria_ConNombreValido_DebeEstablecerCategoria()
        {
            // Arrange
            var nombreCategoria = "Pizzas";

            // Act
            var resultado = _builder.EnCategoria(nombreCategoria);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _notificationManagerMock.Verify(n => n.AddInformation(It.Is<string>(s => s.Contains("Pizzas")), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void EnCategoria_ConNombreVacio_DebeAgregarError()
        {
            // Act
            var resultado = _builder.EnCategoria("");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El nombre de la categoría no puede estar vacío", "CategoriaNombre", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConPopularidadInicial Tests

        [Fact]
        public void ConPopularidadInicial_ConValorValido_DebeEstablecerPopularidad()
        {
            // Arrange
            var popularidad = 5;

            // Act
            var resultado = _builder.ConPopularidadInicial(popularidad);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConPopularidadInicial_ConValorNegativo_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConPopularidadInicial(-1);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("La popularidad inicial debe estar entre 0 y 10", "PopularidadInicial", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConPopularidadInicial_ConValorMuyAlto_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConPopularidadInicial(11);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("La popularidad inicial debe estar entre 0 y 10", "PopularidadInicial", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Construir Tests

        [Fact]
        public void Construir_ConDatosCompletos_DebeCrearProductoExitosamente()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            _notificationManagerMock.Setup(n => n.ToResult<Producto>(It.IsAny<Producto>()))
                .Returns<Producto>(p => Result.Success(p));

            // Act
            var resultado = _builder
                .ConNombre("Pizza Margarita")
                .ConDescripcion("Pizza tradicional italiana")
                .ConPrecio(15.99m)
                .EnCategoria(categoriaId, "Pizzas")
                .ConPopularidadInicial(5)
                .Construir();

            // Assert
            resultado.Should().NotBeNull();
            _notificationManagerMock.Verify(n => n.ClearErrors(), Times.Once);
        }

        [Fact]
        public void Construir_SinNombre_DebeRetornarError()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            _notificationManagerMock.Setup(n => n.ToResult<Producto>(null))
                .Returns(Result.Failure<Producto>("Error"));

            // Act
            var resultado = _builder
                .ConDescripcion("Pizza tradicional italiana")
                .ConPrecio(15.99m)
                .EnCategoria(categoriaId, "Pizzas")
                .Construir();

            // Assert
            _notificationManagerMock.Verify(n => n.AddError("El nombre del producto es obligatorio", "Nombre", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Construir_SinDescripcion_DebeRetornarError()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            _notificationManagerMock.Setup(n => n.ToResult<Producto>(null))
                .Returns(Result.Failure<Producto>("Error"));

            // Act
            var resultado = _builder
                .ConNombre("Pizza Margarita")
                .ConPrecio(15.99m)
                .EnCategoria(categoriaId, "Pizzas")
                .Construir();

            // Assert
            _notificationManagerMock.Verify(n => n.AddError("La descripción del producto es obligatoria", "Descripcion", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Construir_SinPrecio_DebeRetornarError()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            _notificationManagerMock.Setup(n => n.ToResult<Producto>(null))
                .Returns(Result.Failure<Producto>("Error"));

            // Act
            var resultado = _builder
                .ConNombre("Pizza Margarita")
                .ConDescripcion("Pizza tradicional italiana")
                .EnCategoria(categoriaId, "Pizzas")
                .Construir();

            // Assert
            _notificationManagerMock.Verify(n => n.AddError("El precio del producto es obligatorio", "Precio", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Construir_SinCategoria_DebeRetornarError()
        {
            // Arrange
            _notificationManagerMock.Setup(n => n.ToResult<Producto>(null))
                .Returns(Result.Failure<Producto>("Error"));

            // Act
            var resultado = _builder
                .ConNombre("Pizza Margarita")
                .ConDescripcion("Pizza tradicional italiana")
                .ConPrecio(15.99m)
                .Construir();

            // Assert
            _notificationManagerMock.Verify(n => n.AddError("La categoría del producto es obligatoria", "CategoriaId", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Construir_ConExcepcion_DebeCapturarYRetornarError()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            _notificationManagerMock.Setup(n => n.ToResult<Producto>(null))
                .Returns(Result.Failure<Producto>("Error"));

            // Simular excepción en creación del PrecioProducto
            // (esto es difícil de simular directamente, pero podemos verificar el manejo)

            // Act
            var resultado = _builder
                .ConNombre("Pizza Margarita")
                .ConDescripcion("Pizza tradicional italiana")
                .ConPrecio(0) // Esto causará un error en PrecioProducto
                .EnCategoria(categoriaId, "Pizzas")
                .Construir();

            // Assert - Verificar que se manejó el error
            _notificationManagerMock.Verify(n => n.AddError("El precio del producto debe ser mayor que cero", "Precio", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Reset Tests

        [Fact]
        public void Reset_DebeReiniciarTodosLosCampos()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            _builder
                .ConNombre("Pizza Margarita")
                .ConDescripcion("Pizza tradicional italiana")
                .ConPrecio(15.99m)
                .EnCategoria(categoriaId, "Pizzas")
                .ConPopularidadInicial(5);

            // Act
            var resultado = _builder.Reset();

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.ClearErrors(), Times.Once);

            // Verificar que después del reset se requieren todos los campos nuevamente
            _notificationManagerMock.Setup(n => n.ToResult<Producto>(null))
                .Returns(Result.Failure<Producto>("Error"));

            _builder.Construir();

            _notificationManagerMock.Verify(n => n.AddError("El nombre del producto es obligatorio", "Nombre", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Static Factory Tests

        [Fact]
        public void Nuevo_DebeCrearNuevaInstancia()
        {
            // Act
            var nuevoBuilder = ProductoBuilder.Nuevo(_notificationManagerMock.Object, _loggerMock.Object);

            // Assert
            nuevoBuilder.Should().NotBeNull();
            nuevoBuilder.Should().NotBe(_builder); // Debe ser una nueva instancia
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Builder_DebePermitirConstruccionFluidaCompleta()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            _notificationManagerMock.Setup(n => n.ToResult<Producto>(It.IsAny<Producto>()))
                .Returns<Producto>(p => Result.Success(p));

            // Act
            var resultado = ProductoBuilder.Nuevo(_notificationManagerMock.Object, _loggerMock.Object)
                .ConNombre("Pizza Quattro Stagioni")
                .ConDescripcion("Pizza con cuatro sabores representando las estaciones")
                .ConPrecio(18.50m)
                .EnCategoria("Pizzas Especiales")
                .ConPopularidadInicial(7)
                .Construir();

            // Assert
            resultado.Should().NotBeNull();
            _notificationManagerMock.Verify(n => n.AddInformation(It.Is<string>(s => s.Contains("Pizzas Especiales")), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Builder_DebePermitirReutilizacion()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            _notificationManagerMock.Setup(n => n.ToResult<Producto>(It.IsAny<Producto>()))
                .Returns<Producto>(p => Result.Success(p));

            // Act - Primera construcción
            _builder
                .ConNombre("Pizza Margarita")
                .ConDescripcion("Pizza tradicional italiana")
                .ConPrecio(15.99m)
                .EnCategoria(categoriaId, "Pizzas")
                .Construir();

            // Reset y segunda construcción
            var resultado2 = _builder
                .Reset()
                .ConNombre("Pizza Pepperoni")
                .ConDescripcion("Pizza con pepperoni")
                .ConPrecio(17.99m)
                .EnCategoria(categoriaId, "Pizzas")
                .Construir();

            // Assert
            resultado2.Should().NotBeNull();
            _notificationManagerMock.Verify(n => n.ClearErrors(), Times.AtLeast(2));
        }

        #endregion
    }
} 