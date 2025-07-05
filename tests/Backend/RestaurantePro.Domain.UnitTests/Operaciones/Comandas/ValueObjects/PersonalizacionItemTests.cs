namespace RestaurantePro.Domain.UnitTests.Operaciones.Comandas.ValueObjects
{
    public class PersonalizacionItemTests
    {
        [Fact]
        public void CrearAgregar_ConParametrosValidos_CreaPropiedadesCorrectas()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Queso";
            decimal cantidad = 2.5m;
            decimal precioAdicional = 10m;

            // Act
            var personalizacion = PersonalizacionItem.CrearAgregar(
                ingredienteId,
                nombreIngrediente,
                cantidad,
                precioAdicional);

            // Assert
            personalizacion.IngredienteId.Should().Be(ingredienteId);
            personalizacion.NombreIngrediente.Should().Be(nombreIngrediente);
            personalizacion.Accion.Should().Be(AccionPersonalizacion.Agregar);
            personalizacion.Cantidad.Should().Be(cantidad);
            personalizacion.PrecioAdicional.Should().Be(precioAdicional);
            personalizacion.IngredienteSustitucionId.Should().BeNull();
            personalizacion.NombreIngredienteSustitucion.Should().BeNull();
        }

        [Fact]
        public void CrearQuitar_ConParametrosValidos_CreaPropiedadesCorrectas()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Cebolla";

            // Act
            var personalizacion = PersonalizacionItem.CrearQuitar(
                ingredienteId,
                nombreIngrediente);

            // Assert
            personalizacion.IngredienteId.Should().Be(ingredienteId);
            personalizacion.NombreIngrediente.Should().Be(nombreIngrediente);
            personalizacion.Accion.Should().Be(AccionPersonalizacion.Quitar);
            personalizacion.Cantidad.Should().Be(0);
            personalizacion.PrecioAdicional.Should().Be(0);
            personalizacion.IngredienteSustitucionId.Should().BeNull();
            personalizacion.NombreIngredienteSustitucion.Should().BeNull();
        }

        [Fact]
        public void CrearSustituir_ConParametrosValidos_CreaPropiedadesCorrectas()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Papas fritas";
            var ingredienteSustitucionId = Guid.NewGuid();
            var nombreIngredienteSustitucion = "Ensalada";
            decimal cantidad = 1m;
            decimal precioAdicional = 5m;

            // Act
            var personalizacion = PersonalizacionItem.CrearSustituir(
                ingredienteId,
                nombreIngrediente,
                ingredienteSustitucionId,
                nombreIngredienteSustitucion,
                cantidad,
                precioAdicional);

            // Assert
            personalizacion.IngredienteId.Should().Be(ingredienteId);
            personalizacion.NombreIngrediente.Should().Be(nombreIngrediente);
            personalizacion.Accion.Should().Be(AccionPersonalizacion.Sustituir);
            personalizacion.Cantidad.Should().Be(cantidad);
            personalizacion.PrecioAdicional.Should().Be(precioAdicional);
            personalizacion.IngredienteSustitucionId.Should().Be(ingredienteSustitucionId);
            personalizacion.NombreIngredienteSustitucion.Should().Be(nombreIngredienteSustitucion);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CrearAgregar_ConCantidadInvalida_DebeLanzarExcepcion(decimal cantidadInvalida)
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Queso";

            // Act & Assert
            Action act = () => PersonalizacionItem.CrearAgregar(
                ingredienteId,
                nombreIngrediente,
                cantidadInvalida);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*cantidad*");
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-10)]
        public void CrearAgregar_ConPrecioAdicionalNegativo_DebeLanzarExcepcion(decimal precioInvalido)
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Queso";
            decimal cantidad = 1m;

            // Act & Assert
            Action act = () => PersonalizacionItem.CrearAgregar(
                ingredienteId,
                nombreIngrediente,
                cantidad,
                precioInvalido);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*precio*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void CrearAgregar_ConNombreIngredienteInvalido_DebeLanzarExcepcion(string? nombreInvalido)
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            decimal cantidad = 1m;

            // Act & Assert
            Action act = () => PersonalizacionItem.CrearAgregar(
                ingredienteId,
                nombreInvalido!,
                cantidad);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*nombre*");
        }

        [Fact]
        public void CrearAgregar_ConIngredienteIdVacio_DebeLanzarExcepcion()
        {
            // Arrange
            var ingredienteId = Guid.Empty;
            var nombreIngrediente = "Queso";
            decimal cantidad = 1m;

            // Act & Assert
            Action act = () => PersonalizacionItem.CrearAgregar(
                ingredienteId,
                nombreIngrediente,
                cantidad);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*ID del ingrediente*");
        }

        [Fact]
        public void ObtenerDescripcion_ParaAccionAgregar_DevuelveTextoFormateadoCorrecto()
        {
            // Arrange
            var personalizacion = PersonalizacionItem.CrearAgregar(
                Guid.NewGuid(),
                "Queso",
                2.5m);

            // Act
            var descripcion = personalizacion.ObtenerDescripcion();

            // Assert
            descripcion.Should().Be("Extra Queso (2.5)");
        }

        [Fact]
        public void ObtenerDescripcion_ParaAccionQuitar_DevuelveTextoFormateadoCorrecto()
        {
            // Arrange
            var personalizacion = PersonalizacionItem.CrearQuitar(
                Guid.NewGuid(),
                "Cebolla");

            // Act
            var descripcion = personalizacion.ObtenerDescripcion();

            // Assert
            descripcion.Should().Be("Sin Cebolla");
        }

        [Fact]
        public void ObtenerDescripcion_ParaAccionSustituir_DevuelveTextoFormateadoCorrecto()
        {
            // Arrange
            var personalizacion = PersonalizacionItem.CrearSustituir(
                Guid.NewGuid(),
                "Papas fritas",
                Guid.NewGuid(),
                "Ensalada");

            // Act
            var descripcion = personalizacion.ObtenerDescripcion();

            // Assert
            descripcion.Should().Be("Sustituir Papas fritas por Ensalada (1)");
        }

        [Fact]
        public void AfectaPrecio_ConPrecioAdicionalMayorACero_DevuelveTrue()
        {
            // Arrange
            var personalizacion = PersonalizacionItem.CrearAgregar(
                Guid.NewGuid(),
                "Queso",
                2m,
                10m);

            // Act
            var afectaPrecio = personalizacion.AfectaPrecio();

            // Assert
            afectaPrecio.Should().BeTrue();
        }

        [Fact]
        public void AfectaPrecio_ConPrecioAdicionalCero_DevuelveFalse()
        {
            // Arrange
            var personalizacion = PersonalizacionItem.CrearAgregar(
                Guid.NewGuid(),
                "Queso",
                2m,
                0m);

            // Act
            var afectaPrecio = personalizacion.AfectaPrecio();

            // Assert
            afectaPrecio.Should().BeFalse();
        }

        [Fact]
        public void Equals_ConMismosValores_DevuelveTrue()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Queso";
            decimal cantidad = 2.5m;
            decimal precioAdicional = 10m;

            var personalizacion1 = PersonalizacionItem.CrearAgregar(
                ingredienteId,
                nombreIngrediente,
                cantidad,
                precioAdicional);

            var personalizacion2 = PersonalizacionItem.CrearAgregar(
                ingredienteId,
                nombreIngrediente,
                cantidad,
                precioAdicional);

            // Act & Assert
            personalizacion1.Should().Be(personalizacion2);
            (personalizacion1 == personalizacion2).Should().BeTrue();
            personalizacion1.GetHashCode().Should().Be(personalizacion2.GetHashCode());
        }

        [Fact]
        public void Equals_ConValoresDiferentes_DevuelveFalse()
        {
            // Arrange
            var personalizacion1 = PersonalizacionItem.CrearAgregar(
                Guid.NewGuid(),
                "Queso",
                2.5m,
                10m);

            var personalizacion2 = PersonalizacionItem.CrearAgregar(
                Guid.NewGuid(),
                "Tocino",
                1.5m,
                5m);

            // Act & Assert
            personalizacion1.Should().NotBe(personalizacion2);
            (personalizacion1 == personalizacion2).Should().BeFalse();
        }
    }
} 