namespace RestaurantePro.Domain.UnitTests.Operaciones.Comandas.Entities
{
    public class ItemComandaPersonalizacionTests
    {
        private readonly Guid _comandaId = Guid.NewGuid();
        private readonly Guid _productoId = Guid.NewGuid();

        [Fact]
        public void AgregarPersonalizacionExtra_ConParametrosValidos_DebeAgregarPersonalizacion()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Queso";
            decimal cantidad = 2m;
            decimal precioAdicional = 10m;

            // Act
            item.AgregarPersonalizacionExtra(
                ingredienteId,
                nombreIngrediente,
                cantidad,
                precioAdicional);

            // Assert
            item.Personalizaciones.Should().HaveCount(1);
            var personalizacion = item.Personalizaciones.First();
            personalizacion.Accion.Should().Be(AccionPersonalizacion.Agregar);
            personalizacion.IngredienteId.Should().Be(ingredienteId);
            personalizacion.NombreIngrediente.Should().Be(nombreIngrediente);
            personalizacion.Cantidad.Should().Be(cantidad);
            personalizacion.PrecioAdicional.Should().Be(precioAdicional);
        }

        [Fact]
        public void AgregarPersonalizacionExtra_ConPrecioAdicional_DebeActualizarPrecioYSubtotal()
        {
            // Arrange
            var cantidad = 2;
            var precioUnitario = 100m;
            var item = new ItemComanda(_comandaId, _productoId, cantidad, precioUnitario);
            var subtotalOriginal = item.Subtotal;
            var precioAdicional = 10m;

            // Act
            item.AgregarPersonalizacionExtra(
                Guid.NewGuid(),
                "Queso",
                1m,
                precioAdicional);

            // Assert
            item.PrecioUnitario.Should().Be(precioUnitario + precioAdicional);
            item.Subtotal.Should().Be((precioUnitario + precioAdicional) * cantidad);
            item.Subtotal.Should().BeGreaterThan(subtotalOriginal);
        }

        [Fact]
        public void AgregarPersonalizacionQuitar_ConParametrosValidos_DebeAgregarPersonalizacion()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Cebolla";

            // Act
            item.AgregarPersonalizacionQuitar(
                ingredienteId,
                nombreIngrediente);

            // Assert
            item.Personalizaciones.Should().HaveCount(1);
            var personalizacion = item.Personalizaciones.First();
            personalizacion.Accion.Should().Be(AccionPersonalizacion.Quitar);
            personalizacion.IngredienteId.Should().Be(ingredienteId);
            personalizacion.NombreIngrediente.Should().Be(nombreIngrediente);
            personalizacion.Cantidad.Should().Be(0);
            personalizacion.PrecioAdicional.Should().Be(0);
        }

        [Fact]
        public void AgregarPersonalizacionQuitar_NoCambiaPrecio()
        {
            // Arrange
            var cantidad = 2;
            var precioUnitario = 100m;
            var item = new ItemComanda(_comandaId, _productoId, cantidad, precioUnitario);
            var subtotalOriginal = item.Subtotal;

            // Act
            item.AgregarPersonalizacionQuitar(
                Guid.NewGuid(),
                "Cebolla");

            // Assert
            item.PrecioUnitario.Should().Be(precioUnitario);
            item.Subtotal.Should().Be(subtotalOriginal);
        }

        [Fact]
        public void AgregarPersonalizacionSustituir_ConParametrosValidos_DebeAgregarPersonalizacion()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Papas fritas";
            var ingredienteSustitucionId = Guid.NewGuid();
            var nombreIngredienteSustitucion = "Ensalada";
            decimal cantidad = 1m;
            decimal precioAdicional = 5m;

            // Act
            item.AgregarPersonalizacionSustituir(
                ingredienteId,
                nombreIngrediente,
                ingredienteSustitucionId,
                nombreIngredienteSustitucion,
                cantidad,
                precioAdicional);

            // Assert
            item.Personalizaciones.Should().HaveCount(1);
            var personalizacion = item.Personalizaciones.First();
            personalizacion.Accion.Should().Be(AccionPersonalizacion.Sustituir);
            personalizacion.IngredienteId.Should().Be(ingredienteId);
            personalizacion.NombreIngrediente.Should().Be(nombreIngrediente);
            personalizacion.IngredienteSustitucionId.Should().Be(ingredienteSustitucionId);
            personalizacion.NombreIngredienteSustitucion.Should().Be(nombreIngredienteSustitucion);
            personalizacion.Cantidad.Should().Be(cantidad);
            personalizacion.PrecioAdicional.Should().Be(precioAdicional);
        }

        [Fact]
        public void AgregarPersonalizacion_ItemNoEnEstadoPendiente_DebeLanzarExcepcion()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);
            item.MarcarEnPreparacion(); // Cambiar estado a EnPreparacion

            // Act & Assert
            Action act = () => item.AgregarPersonalizacionExtra(
                Guid.NewGuid(),
                "Queso",
                1m);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*No se pueden agregar personalizaciones*");
        }

        [Fact]
        public void EliminarPersonalizacion_ExistenteConPrecioAdicional_DebeEliminarYActualizarPrecio()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 2, 100m);
            var precioAdicional = 10m;
            
            item.AgregarPersonalizacionExtra(
                Guid.NewGuid(),
                "Queso",
                1m,
                precioAdicional);
                
            var personalizacion = item.Personalizaciones.First();
            var precioUnitarioConExtra = item.PrecioUnitario;
            var subtotalConExtra = item.Subtotal;

            // Act
            item.EliminarPersonalizacion(personalizacion);

            // Assert
            item.Personalizaciones.Should().BeEmpty();
            item.PrecioUnitario.Should().Be(precioUnitarioConExtra - precioAdicional);
            item.Subtotal.Should().Be(subtotalConExtra - (precioAdicional * item.Cantidad));
        }

        [Fact]
        public void EliminarPersonalizacion_NoExistente_NoDebeCambiarNada()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);
            item.AgregarPersonalizacionExtra(
                Guid.NewGuid(),
                "Queso",
                1m);
                
            var personalizacionNoExistente = PersonalizacionItem.CrearAgregar(
                Guid.NewGuid(),
                "Tocino",
                1m);
                
            var personalizacionesCountAntes = item.Personalizaciones.Count;

            // Act
            item.EliminarPersonalizacion(personalizacionNoExistente);

            // Assert
            item.Personalizaciones.Should().HaveCount(personalizacionesCountAntes);
        }

        [Fact]
        public void EliminarPersonalizacion_ItemNoEnEstadoPendiente_DebeLanzarExcepcion()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);
            var personalizacion = PersonalizacionItem.CrearAgregar(
                Guid.NewGuid(),
                "Queso",
                1m);
                
            item.AgregarPersonalizacionExtra(
                personalizacion.IngredienteId,
                personalizacion.NombreIngrediente,
                personalizacion.Cantidad);
                
            item.MarcarEnPreparacion(); // Cambiar estado a EnPreparacion

            // Act & Assert
            Action act = () => item.EliminarPersonalizacion(personalizacion);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*No se pueden eliminar personalizaciones*");
        }

        [Fact]
        public void TienePersonalizaciones_ConPersonalizaciones_DevuelveTrue()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);
            item.AgregarPersonalizacionExtra(
                Guid.NewGuid(),
                "Queso",
                1m);

            // Act
            var result = item.TienePersonalizaciones();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void TienePersonalizaciones_SinPersonalizaciones_DevuelveFalse()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);

            // Act
            var result = item.TienePersonalizaciones();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CalcularPrecioAdicionalPersonalizaciones_ConVariasPersonalizaciones_SumaCorrectamente()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);
            var precio1 = 10m;
            var precio2 = 5m;
            
            item.AgregarPersonalizacionExtra(
                Guid.NewGuid(),
                "Queso",
                1m,
                precio1);
                
            item.AgregarPersonalizacionSustituir(
                Guid.NewGuid(),
                "Papas",
                Guid.NewGuid(),
                "Ensalada",
                1m,
                precio2);

            // Act
            var precioAdicionalTotal = item.CalcularPrecioAdicionalPersonalizaciones();

            // Assert
            precioAdicionalTotal.Should().Be(precio1 + precio2);
        }

        [Fact]
        public void AgregarMultiplesPersonalizaciones_TodasLasPersonalizacionesSeAgreganCorrectamente()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);

            // Act
            item.AgregarPersonalizacionExtra(Guid.NewGuid(), "Queso", 1m, 10m);
            item.AgregarPersonalizacionQuitar(Guid.NewGuid(), "Cebolla");
            item.AgregarPersonalizacionSustituir(
                Guid.NewGuid(), 
                "Papas", 
                Guid.NewGuid(), 
                "Ensalada", 
                1m, 
                5m);

            // Assert
            item.Personalizaciones.Should().HaveCount(3);
            item.Personalizaciones.Count(p => p.Accion == AccionPersonalizacion.Agregar).Should().Be(1);
            item.Personalizaciones.Count(p => p.Accion == AccionPersonalizacion.Quitar).Should().Be(1);
            item.Personalizaciones.Count(p => p.Accion == AccionPersonalizacion.Sustituir).Should().Be(1);
        }

        [Fact]
        public void AgregarPersonalizacion_GeneraEventoDominio()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Queso";

            // Act
            item.AgregarPersonalizacionExtra(ingredienteId, nombreIngrediente, 1m);

            // Assert
            item.DomainEvents.Should().ContainSingle(e => e is PersonalizacionAgregadaAItem);
            var evento = item.DomainEvents.OfType<PersonalizacionAgregadaAItem>().First();
            evento.ItemId.Should().Be(item.Id);
            evento.ComandaId.Should().Be(_comandaId);
            evento.IngredienteId.Should().Be(ingredienteId);
            evento.NombreIngrediente.Should().Be(nombreIngrediente);
            evento.Accion.Should().Be(AccionPersonalizacion.Agregar);
            evento.Cantidad.Should().Be(1m);
            evento.PrecioAdicional.Should().Be(0m);
        }

        [Fact]
        public void EliminarPersonalizacion_GeneraEventoDominio()
        {
            // Arrange
            var item = new ItemComanda(_comandaId, _productoId, 1, 100m);
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Queso";
            
            item.AgregarPersonalizacionExtra(ingredienteId, nombreIngrediente, 1m);
            var personalizacion = item.Personalizaciones.First();
            item.ClearDomainEvents(); // Limpiar eventos existentes

            // Act
            item.EliminarPersonalizacion(personalizacion);

            // Assert
            item.DomainEvents.Should().ContainSingle(e => e is PersonalizacionEliminadaDeItem);
            var evento = item.DomainEvents.OfType<PersonalizacionEliminadaDeItem>().First();
            evento.ItemId.Should().Be(item.Id);
            evento.ComandaId.Should().Be(_comandaId);
            evento.IngredienteId.Should().Be(ingredienteId);
            evento.NombreIngrediente.Should().Be(nombreIngrediente);
            evento.Accion.Should().Be(AccionPersonalizacion.Agregar);
            evento.Cantidad.Should().Be(1m);
            evento.PrecioAdicional.Should().Be(0m);
        }
    }
} 