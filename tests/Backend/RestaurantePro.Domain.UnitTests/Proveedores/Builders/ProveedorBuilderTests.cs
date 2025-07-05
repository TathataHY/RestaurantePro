#nullable disable
namespace RestaurantePro.Domain.UnitTests.Proveedores.Builders
{
    /// <summary>
    /// Pruebas unitarias para ProveedorBuilder - Validando patrón Builder con Result/Notification
    /// </summary>
    public class ProveedorBuilderTests
    {
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly Mock<ILogger<ProveedorBuilder>> _loggerMock;
        private readonly ProveedorBuilder _builder;

        public ProveedorBuilderTests()
        {
            _notificationManagerMock = new Mock<INotificationManager>();
            _loggerMock = new Mock<ILogger<ProveedorBuilder>>();
            _builder = new ProveedorBuilder(_notificationManagerMock.Object, _loggerMock.Object);
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
            var act = () => new ProveedorBuilder(null, _loggerMock.Object);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("notificationManager");
        }

        [Fact]
        public void Constructor_ConLoggerNulo_DebeLanzarExcepcion()
        {
            // Act & Assert
            var act = () => new ProveedorBuilder(_notificationManagerMock.Object, null);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        #endregion

        #region ConNombre Tests

        [Fact]
        public void ConNombre_ConNombreValido_DebeEstablecerNombre()
        {
            // Arrange
            var nombre = "Distribuidora La Central";

            // Act
            var resultado = _builder.ConNombre(nombre);

            // Assert
            resultado.Should().Be(_builder); // Fluent interface
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConNombre_ConNombreVacio_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConNombre("");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El nombre del proveedor no puede estar vacío", "Nombre", It.IsAny<string>()), Times.Once);
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
            _notificationManagerMock.Verify(n => n.AddError("El nombre del proveedor no puede exceder 100 caracteres", "Nombre", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConContactoPrincipal Tests

        [Fact]
        public void ConContactoPrincipal_ConNombreValido_DebeEstablecerContacto()
        {
            // Arrange
            var nombreContacto = "Juan Pérez";

            // Act
            var resultado = _builder.ConContactoPrincipal(nombreContacto);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConContactoPrincipal_ConNombreVacio_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConContactoPrincipal("");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El nombre del contacto principal no puede estar vacío", "NombreContacto", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConContactoPrincipal_ConNombreMuyLargo_DebeAgregarError()
        {
            // Arrange
            var nombreLargo = new string('A', 101);

            // Act
            var resultado = _builder.ConContactoPrincipal(nombreLargo);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El nombre del contacto no puede exceder 100 caracteres", "NombreContacto", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConEmail Tests

        [Fact]
        public void ConEmail_ConEmailValido_DebeEstablecerEmail()
        {
            // Arrange
            var email = "contacto@proveedor.cl";

            // Act
            var resultado = _builder.ConEmail(email);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConEmail_ConEmailVacio_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConEmail("");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El email del proveedor no puede estar vacío", "Email", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConEmail_ConFormatoInvalido_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConEmail("email-invalido");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.Is<string>(s => s.Contains("formato del email no es válido")), "Email", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConTelefono Tests

        [Fact]
        public void ConTelefono_ConTelefonoValido_DebeEstablecerTelefono()
        {
            // Arrange
            var telefono = "+52 55 1234 5678";

            // Act
            var resultado = _builder.ConTelefono(telefono);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConTelefono_ConTelefonoVacio_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConTelefono("");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El teléfono del proveedor no puede estar vacío", "Telefono", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConTelefono_ConFormatoInvalido_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConTelefono("123");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.Is<string>(s => s.Contains("formato del teléfono no es válido")), "Telefono", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConDireccion Tests

        [Fact]
        public void ConDireccion_ConDatosValidos_DebeEstablecerDireccion()
        {
            // Arrange
            var direccion = "Av. Providencia 123";
            var ciudad = "Santiago";
            var codigoPostal = "1234567";
            var pais = "Chile";

            // Act
            var resultado = _builder.ConDireccion(direccion, ciudad, codigoPostal, pais);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConDireccion_ConDireccionVacia_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConDireccion("", "Ciudad", "1234567");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("La dirección del proveedor no puede estar vacía", "Direccion", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConDireccion_ConCiudadVacia_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConDireccion("Dirección", "", "1234567");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("La ciudad del proveedor no puede estar vacía", "Ciudad", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConDireccion_ConCodigoPostalInvalidoChile_DebeAgregarError()
        {
            // Act - Usar código postal de 6 dígitos (inválido para Chile)
            var resultado = _builder.ConDireccion("Dirección", "Ciudad", "123456", "Chile");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El código postal debe tener 7 dígitos para Chile", "CodigoPostal", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConRUT Tests

        [Fact]
        public void ConRUT_ConRUTValido_DebeEstablecerRUT()
        {
            var rut = "12345678-9";

            var resultado = _builder.ConRUT(rut);

            resultado.Should().BeSameAs(_builder);
        }

        [Fact]
        public void ConRUT_ConRUTVacio_DebeAgregarError()
        {
            var resultado = _builder.ConRUT("");

            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConRUT_ConLongitudInvalida_DebeAgregarError()
        {
            var resultado = _builder.ConRUT("ABC123");

            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConRUT_ConFormatoInvalido_DebeAgregarError()
        {
            var resultado = _builder.ConRUT("1234567890");

            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConInformacionBancaria Tests

        [Fact]
        public void ConInformacionBancaria_ConInformacionValida_DebeEstablecerInformacion()
        {
            // Arrange
            var info = "BBVA Bancomer - Cuenta 123456789";

            // Act
            var resultado = _builder.ConInformacionBancaria(info);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConInformacionBancaria_ConInformacionVacia_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConInformacionBancaria("");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("La información bancaria no puede estar vacía", "InformacionBancaria", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConInformacionBancaria_ConInformacionMuyLarga_DebeAgregarError()
        {
            // Arrange
            var infoLarga = new string('A', 501);

            // Act
            var resultado = _builder.ConInformacionBancaria(infoLarga);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("La información bancaria no puede exceder 500 caracteres", "InformacionBancaria", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConDiasCredito Tests

        [Fact]
        public void ConDiasCredito_ConValorValido_DebeEstablecerDiasCredito()
        {
            // Arrange
            var diasCredito = 30;

            // Act
            var resultado = _builder.ConDiasCredito(diasCredito);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConDiasCredito_ConValorNegativo_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConDiasCredito(-1);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("Los días de crédito no pueden ser negativos", "DiasCredito", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ConDiasCredito_ConValorMuyAlto_DebeAgregarError()
        {
            // Act
            var resultado = _builder.ConDiasCredito(366);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("Los días de crédito no pueden exceder 365 días", "DiasCredito", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConObservaciones Tests

        [Fact]
        public void ConObservaciones_ConObservacionesValidas_DebeEstablecerObservaciones()
        {
            // Arrange
            var observaciones = "Proveedor confiable con entregas puntuales";

            // Act
            var resultado = _builder.ConObservaciones(observaciones);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConObservaciones_ConObservacionesVacias_DebePermitirlo()
        {
            // Act
            var resultado = _builder.ConObservaciones("");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ConObservaciones_ConObservacionesMuyLargas_DebeAgregarError()
        {
            // Arrange
            var observacionesLargas = new string('A', 1001);

            // Act
            var resultado = _builder.ConObservaciones(observacionesLargas);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("Las observaciones no pueden exceder 1000 caracteres", "Observaciones", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region AgregarContacto Tests

        [Fact]
        public void AgregarContacto_ConDatosValidos_DebeAgregarContacto()
        {
            // Arrange
            var nombre = "María González";
            var cargo = "Gerente de Ventas";
            var telefono = "+52 55 9876 5432";
            var email = "maria@proveedor.cl";

            // Act
            var resultado = _builder.AgregarContacto(nombre, cargo, telefono, email);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void AgregarContacto_ConNombreVacio_DebeAgregarError()
        {
            // Act
            var resultado = _builder.AgregarContacto("", "Cargo", "123456789", "email@test.cl");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El nombre del contacto no puede estar vacío", "ContactoNombre", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void AgregarContacto_ConCargoVacio_DebeAgregarError()
        {
            // Act
            var resultado = _builder.AgregarContacto("Nombre", "", "123456789", "email@test.cl");

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El cargo del contacto no puede estar vacío", "ContactoCargo", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void AgregarContacto_ConEmailDuplicado_DebeAgregarError()
        {
            // Arrange
            var email = "duplicado@test.cl";
            _builder.AgregarContacto("Contacto1", "Cargo1", "123456789", email);

            // Act
            var resultado = _builder.AgregarContacto("Contacto2", "Cargo2", "987654321", email);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.Is<string>(s => s.Contains("Ya existe un contacto con el email")), "ContactoDuplicado", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region EnCategoria Tests

        [Fact]
        public void EnCategoria_ConCategoriaValida_DebeAgregarCategoria()
        {
            // Arrange
            var categoria = CategoriaProveedor.BebidasNoAlcoholicas;
            var descuento = 5.0m;

            // Act
            var resultado = _builder.EnCategoria(categoria, descuento, true);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void EnCategoria_ConPorcentajeInvalido_DebeAgregarError()
        {
            // Act
            var resultado = _builder.EnCategoria(CategoriaProveedor.BebidasNoAlcoholicas, -1);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError("El porcentaje de descuento debe estar entre 0 y 100", "PorcentajeDescuento", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void EnCategoria_ConCategoriaDuplicada_DebeAgregarError()
        {
            // Arrange
            var categoria = CategoriaProveedor.BebidasNoAlcoholicas;
            _builder.EnCategoria(categoria, 5.0m);

            // Act
            var resultado = _builder.EnCategoria(categoria, 10.0m);

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.AddError(It.Is<string>(s => s.Contains("ya fue agregada al proveedor")), "CategoriaDuplicada", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Construir Tests

        [Fact]
        public void Construir_ConDatosCompletos_DebeCrearProveedorExitosamente()
        {
            // Arrange
            _notificationManagerMock.Setup(n => n.ToResult<Proveedor>(It.IsAny<Proveedor>()))
                .Returns<Proveedor>(p => Result.Success(p));

            // Act
            var resultado = _builder
                .ConNombre("Distribuidora La Central")
                .ConContactoPrincipal("Juan Pérez")
                .ConEmail("contacto@central.cl")
                .ConTelefono("+52 55 1234 5678")
                .ConDireccion("Av. Providencia 123", "Santiago", "1234567", "Chile")
                .ConRUT("12345678-9")
                .ConInformacionBancaria("Banco de Chile - Cuenta 987654321")
                .ConDiasCredito(30)
                .ConObservaciones("Proveedor confiable")
                .Construir();

            // Assert
            resultado.Should().NotBeNull();
            _notificationManagerMock.Verify(n => n.ClearErrors(), Times.Once);
        }

        [Fact]
        public void Construir_SinNombre_DebeRetornarError()
        {
            // Arrange
            _notificationManagerMock.Setup(n => n.ToResult<Proveedor>(null))
                .Returns(Result.Failure<Proveedor>("Error"));

            // Act
            var resultado = _builder
                .ConContactoPrincipal("Juan Pérez")
                .ConEmail("contacto@central.cl")
                .ConTelefono("+52 55 1234 5678")
                .ConDireccion("Av. Providencia 123", "Santiago", "1234567")
                .Construir();

            // Assert
            _notificationManagerMock.Verify(n => n.AddError("El nombre del proveedor es obligatorio", "Nombre", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Construir_SinContactoPrincipal_DebeRetornarError()
        {
            // Arrange
            _notificationManagerMock.Setup(n => n.ToResult<Proveedor>(null))
                .Returns(Result.Failure<Proveedor>("Error"));

            // Act
            var resultado = _builder
                .ConNombre("Distribuidora La Central")
                .ConEmail("contacto@central.cl")
                .ConTelefono("+52 55 1234 5678")
                .ConDireccion("Av. Providencia 123", "Santiago", "1234567")
                .Construir();

            // Assert
            _notificationManagerMock.Verify(n => n.AddError("El nombre del contacto principal es obligatorio", "NombreContacto", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Construir_SinDiasCredito_DebeAsignarValorPorDefecto()
        {
            // Arrange
            _notificationManagerMock.Setup(n => n.ToResult<Proveedor>(It.IsAny<Proveedor>()))
                .Returns<Proveedor>(p => Result.Success(p));

            // Act
            var resultado = _builder
                .ConNombre("Distribuidora La Central")
                .ConContactoPrincipal("Juan Pérez")
                .ConEmail("contacto@central.cl")
                .ConTelefono("+52 55 1234 5678")
                .ConDireccion("Av. Providencia 123", "Santiago", "1234567")
                .Construir();

            // Assert
            _notificationManagerMock.Verify(n => n.AddInformation("Se asignaron 30 días de crédito por defecto", "DiasCredito"), Times.Once);
        }

        #endregion

        #region Reset Tests

        [Fact]
        public void Reset_DebeReiniciarTodosLosCampos()
        {
            // Arrange
            _builder
                .ConNombre("Distribuidora La Central")
                .ConContactoPrincipal("Juan Pérez")
                .ConEmail("contacto@central.cl")
                .ConTelefono("+52 55 1234 5678")
                .ConDireccion("Av. Providencia 123", "Santiago", "1234567")
                .AgregarContacto("María", "Gerente", "123456789", "maria@test.cl")
                .EnCategoria(CategoriaProveedor.BebidasNoAlcoholicas, 5.0m);

            // Act
            var resultado = _builder.Reset();

            // Assert
            resultado.Should().Be(_builder);
            _notificationManagerMock.Verify(n => n.ClearErrors(), Times.Once);

            // Verificar que después del reset se requieren todos los campos nuevamente
            _notificationManagerMock.Setup(n => n.ToResult<Proveedor>(null))
                .Returns(Result.Failure<Proveedor>("Error"));

            _builder.Construir();

            _notificationManagerMock.Verify(n => n.AddError("El nombre del proveedor es obligatorio", "Nombre", It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Static Factory Tests

        [Fact]
        public void Nuevo_DebeCrearNuevaInstancia()
        {
            // Act
            var nuevoBuilder = ProveedorBuilder.Nuevo(_notificationManagerMock.Object, _loggerMock.Object);

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
            _notificationManagerMock.Setup(n => n.ToResult<Proveedor>(It.IsAny<Proveedor>()))
                .Returns<Proveedor>(p => Result.Success(p));

            // Act
            var resultado = ProveedorBuilder.Nuevo(_notificationManagerMock.Object, _loggerMock.Object)
                .ConNombre("Distribuidora Premium SA de CV")
                .ConContactoPrincipal("Carlos Mendoza")
                .ConEmail("contacto@premium.cl")
                .ConTelefono("+52 55 9876 5432")
                .ConDireccion("Av. Las Condes 456", "Las Condes", "7540000", "Chile")
                .ConRUT("12345678-9")
                .ConInformacionBancaria("Banco de Chile - Cuenta 987654321")
                .ConDiasCredito(45)
                .ConObservaciones("Proveedor especializado en productos premium")
                .AgregarContacto("Ana Ruiz", "Coordinadora Logística", "+56 9 1111 2222", "ana@premium.cl")
                .AgregarContacto("Luis Torres", "Ejecutivo de Ventas", "+56 9 3333 4444", "luis@premium.cl")
                .EnCategoria(CategoriaProveedor.BebidasNoAlcoholicas, 8.5m, true)
                .EnCategoria(CategoriaProveedor.Carnes, 5.0m, false)
                .Construir();

            // Assert
            resultado.Should().NotBeNull();
            _notificationManagerMock.Verify(n => n.ClearErrors(), Times.Once);
        }

        [Fact]
        public void Builder_DebePermitirReutilizacion()
        {
            // Arrange
            _notificationManagerMock.Setup(n => n.ToResult<Proveedor>(It.IsAny<Proveedor>()))
                .Returns<Proveedor>(p => Result.Success(p));

            // Act - Primera construcción
            _builder
                .ConNombre("Proveedor A")
                .ConContactoPrincipal("Contacto A")
                .ConEmail("a@test.cl")
                .ConTelefono("+52 55 1111 1111")
                .ConDireccion("Dirección A", "Ciudad A", "11111")
                .Construir();

            // Reset y segunda construcción
            var resultado2 = _builder
                .Reset()
                .ConNombre("Proveedor B")
                .ConContactoPrincipal("Contacto B")
                .ConEmail("b@test.cl")
                .ConTelefono("+52 55 2222 2222")
                .ConDireccion("Dirección B", "Ciudad B", "22222")
                .Construir();

            // Assert
            resultado2.Should().NotBeNull();
            _notificationManagerMock.Verify(n => n.ClearErrors(), Times.AtLeast(2));
        }

        #endregion
    }
} 