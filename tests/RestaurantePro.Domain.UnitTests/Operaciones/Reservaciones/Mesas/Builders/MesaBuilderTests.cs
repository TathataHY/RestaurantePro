namespace RestaurantePro.Domain.UnitTests.Operaciones.Reservaciones.Mesas.Builders
{
    public class MesaBuilderTests
    {
        private readonly Mock<ILogger<MesaBuilder>> _mockLogger;
        private readonly NotificationManager _notificationManager;
        private readonly MesaBuilder _builder;

        public MesaBuilderTests()
        {
            _mockLogger = new Mock<ILogger<MesaBuilder>>();
            _notificationManager = new NotificationManager();
            _builder = new MesaBuilder(_notificationManager, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ConParametrosNulos_DeberiaLanzarExcepcion()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MesaBuilder(null!, _mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new MesaBuilder(_notificationManager, null!));
        }

        [Fact]
        public void ConNumero_ConNumeroValido_DeberiaEstablecerNumero()
        {
            // Arrange
            var numero = 1;

            // Act
            var resultado = _builder.ConNumero(numero);

            // Assert
            Assert.Equal(_builder, resultado);
            _notificationManager.HasErrors.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void ConNumero_ConNumeroInvalido_DeberiaAgregarError(int numeroInvalido)
        {
            // Act
            _builder.ConNumero(numeroInvalido);

            // Assert
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("número de mesa debe ser mayor que 0"));
        }

        [Fact]
        public void ConCapacidad_ConCapacidadValida_DeberiaEstablecerCapacidad()
        {
            // Arrange
            var capacidad = 4;

            // Act
            var resultado = _builder.ConCapacidad(capacidad);

            // Assert
            Assert.Equal(_builder, resultado);
            _notificationManager.HasErrors.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void ConCapacidad_ConCapacidadInvalida_DeberiaAgregarError(int capacidadInvalida)
        {
            // Act
            _builder.ConCapacidad(capacidadInvalida);

            // Assert
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("capacidad debe ser mayor que 0"));
        }

        [Fact]
        public void ConCapacidad_ConCapacidadMayorA50_DeberiaAgregarError()
        {
            // Act
            _builder.ConCapacidad(51);

            // Assert
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("capacidad no puede ser mayor a 50"));
        }

        [Fact]
        public void EnUbicacion_ConUbicacionValida_DeberiaEstablecerUbicacion()
        {
            // Arrange
            var ubicacion = "Terraza";

            // Act
            var resultado = _builder.EnUbicacion(ubicacion);

            // Assert
            Assert.Equal(_builder, resultado);
            _notificationManager.HasErrors.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void EnUbicacion_ConUbicacionVacia_DeberiaAgregarError(string ubicacionVacia)
        {
            // Act
            _builder.EnUbicacion(ubicacionVacia);

            // Assert
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("ubicación no puede estar vacía"));
        }

        [Fact]
        public void EnUbicacion_ConUbicacionNula_DeberiaAgregarError()
        {
            // Act
            _builder.EnUbicacion(null!);

            // Assert
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("ubicación no puede estar vacía"));
        }

        [Fact]
        public void EnUbicacion_ConUbicacionMuyLarga_DeberiaAgregarError()
        {
            // Arrange
            var ubicacionLarga = new string('A', 101); // 101 caracteres

            // Act
            _builder.EnUbicacion(ubicacionLarga);

            // Assert
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("ubicación no puede tener más de 100 caracteres"));
        }

        [Fact]
        public void EnUbicacion_ConEspaciosAlRededor_DeberiaTrimearUbicacion()
        {
            // Arrange
            var ubicacion = "  Terraza  ";

            // Act
            _builder.EnUbicacion(ubicacion);

            // Assert
            _notificationManager.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void Construir_ConTodosLosDatosValidos_DeberiaRetornarExito()
        {
            // Act
            var resultado = _builder
                .ConNumero(1)
                .ConCapacidad(4)
                .EnUbicacion("Terraza")
                .Construir();

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value!.Numero.Should().Be(1);
            resultado.Value.Capacidad.Should().Be(4);
            resultado.Value.Ubicacion.Should().Be("Terraza");
            resultado.Value.Estado.Should().Be(EstadoMesa.Disponible);
        }

        [Fact]
        public void Construir_SinNumero_DeberiaAgregarErrorYRetornarFallo()
        {
            // Act
            var resultado = _builder
                .ConCapacidad(4)
                .EnUbicacion("Terraza")
                .Construir();

            // Assert
            resultado.Succeeded.Should().BeFalse();
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("número de mesa es obligatorio"));
        }

        [Fact]
        public void Construir_SinCapacidad_DeberiaAgregarErrorYRetornarFallo()
        {
            // Act
            var resultado = _builder
                .ConNumero(1)
                .EnUbicacion("Terraza")
                .Construir();

            // Assert
            resultado.Succeeded.Should().BeFalse();
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("capacidad es obligatoria"));
        }

        [Fact]
        public void Construir_SinUbicacion_DeberiaAgregarErrorYRetornarFallo()
        {
            // Act
            var resultado = _builder
                .ConNumero(1)
                .ConCapacidad(4)
                .Construir();

            // Assert
            resultado.Succeeded.Should().BeFalse();
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("ubicación es obligatoria"));
        }

        [Fact]
        public void Construir_ConVariosErrores_DeberiaAcumularErrores()
        {
            // Act
            var resultado = _builder.Construir();

            // Assert
            resultado.Succeeded.Should().BeFalse();
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("número de mesa es obligatorio"));
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("capacidad es obligatoria"));
            _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("ubicación es obligatoria"));
        }

        [Fact]
        public void Reset_DespuesDeConstruir_DeberiaLimpiarPropiedades()
        {
            // Arrange
            _builder.ConNumero(1).ConCapacidad(4).EnUbicacion("Terraza");

            // Act
            var resultado = _builder.Reset();

            // Assert
            Assert.Equal(_builder, resultado);
            _notificationManager.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void Reset_Y_ConstruirNuevamente_DeberiaPermitirReutilizacion()
        {
            // Primera construcción
            var resultado1 = _builder
                .ConNumero(1)
                .ConCapacidad(4)
                .EnUbicacion("Terraza")
                .Construir();

            // Act - Reset y segunda construcción
            var resultado2 = _builder
                .Reset()
                .ConNumero(2)
                .ConCapacidad(6)
                .EnUbicacion("Interior")
                .Construir();

            // Assert
            resultado1.Succeeded.Should().BeTrue();
            resultado2.Succeeded.Should().BeTrue();
            resultado1.Value!.Numero.Should().Be(1);
            resultado2.Value!.Numero.Should().Be(2);
            resultado1.Value.Ubicacion.Should().Be("Terraza");
            resultado2.Value.Ubicacion.Should().Be("Interior");
        }

        [Fact]
        public void Builder_InterfazFluida_DeberiaPermitirEncadenamientoCompleto()
        {
            // Act
            var resultado = _builder
                .ConNumero(5)
                .ConCapacidad(8)
                .EnUbicacion("VIP")
                .Construir();

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value!.Numero.Should().Be(5);
            resultado.Value.Capacidad.Should().Be(8);
            resultado.Value.Ubicacion.Should().Be("VIP");
        }

        [Theory]
        [InlineData(1, 1, "Ventana")]
        [InlineData(10, 2, "Terraza")]
        [InlineData(50, 50, "Salón Principal")]
        public void Builder_ConDiferentesCombinacionesValidas_DeberiaFuncionar(int numero, int capacidad, string ubicacion)
        {
            // Act
            var resultado = _builder
                .ConNumero(numero)
                .ConCapacidad(capacidad)
                .EnUbicacion(ubicacion)
                .Construir();

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value!.Numero.Should().Be(numero);
            resultado.Value.Capacidad.Should().Be(capacidad);
            resultado.Value.Ubicacion.Should().Be(ubicacion);
        }
    }
} 