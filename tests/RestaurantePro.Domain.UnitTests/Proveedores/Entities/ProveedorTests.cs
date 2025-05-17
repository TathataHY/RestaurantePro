namespace RestaurantePro.Domain.UnitTests.Proveedores.Entities
{
    public class ProveedorTests
    {
        [Fact]
        public void CrearProveedor_ConParametrosValidos_DebeCrearProveedorActivo()
        {
            // Arrange
            var nombre = "Distribuidora XYZ";
            var nombreContacto = "Juan Pérez";
            var rfc = "XYZ123456ABC";
            var telefono = "555-123-4567";
            var email = "contacto@xyz.com";
            var direccion = "Calle Principal 123";
            var ciudad = "Ciudad de México";
            var codigoPostal = "12345";
            var pais = "México";
            var informacionBancaria = "Cuenta 12345-67890";
            var diasCredito = 30;
            var observaciones = "Proveedor principal de lácteos";

            // Act
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                nombre,
                nombreContacto,
                email,
                telefono,
                direccion,
                ciudad,
                codigoPostal,
                pais,
                rfc,
                informacionBancaria,
                diasCredito);

            // Assert
            proveedor.Should().NotBeNull();
            proveedor.Id.Should().NotBe(Guid.Empty);
            proveedor.Nombre.Should().Be(nombre);
            proveedor.RFC.Should().Be(rfc);
            proveedor.Telefono.Should().Be(telefono);
            proveedor.Email.Should().Be(email);
            proveedor.Direccion.Should().Be(direccion);
            proveedor.Ciudad.Should().Be(ciudad);
            proveedor.CodigoPostal.Should().Be(codigoPostal);
            proveedor.Pais.Should().Be(pais);
            proveedor.Activo.Should().BeTrue();
            
            // Verificamos que se generó el evento de dominio
            proveedor.DomainEvents.Should().ContainSingle(e => e is ProveedorRegistrado);
            var evento = proveedor.DomainEvents.OfType<ProveedorRegistrado>().First();
            evento.ProveedorId.Should().Be(proveedor.Id);
            evento.Nombre.Should().Be(nombre);
        }

        [Fact]
        public void CrearProveedor_ConNombreVacio_DebeLanzarExcepcion()
        {
            // Arrange
            var nombreVacio = string.Empty;
            var nombreContacto = "Juan Pérez";
            var rfc = "XYZ123456ABC";
            var telefono = "555-123-4567";
            var email = "contacto@xyz.com";
            var direccion = "Calle Principal 123";
            var ciudad = "Ciudad de México";
            var codigoPostal = "12345";
            var pais = "México";
            var informacionBancaria = "Cuenta 12345-67890";
            var diasCredito = 30;

            // Act & Assert
            Action action = () => Domain.Proveedores.Entities.Proveedor.Crear(
                nombreVacio, 
                nombreContacto,
                email, 
                telefono, 
                direccion,
                ciudad,
                codigoPostal,
                pais,
                rfc,
                informacionBancaria,
                diasCredito);
                
            action.Should().Throw<ArgumentException>()
                .WithMessage("*nombre*")
                .WithParameterName("nombre");
        }
        
        [Fact]
        public void CrearProveedor_ConRFCInvalido_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = "Distribuidora XYZ";
            var nombreContacto = "Juan Pérez";
            var rfcInvalido = "123"; // RFC demasiado corto
            var telefono = "555-123-4567";
            var email = "contacto@xyz.com";
            var direccion = "Calle Principal 123";
            var ciudad = "Ciudad de México";
            var codigoPostal = "12345";
            var pais = "México";
            var informacionBancaria = "Cuenta 12345-67890";
            var diasCredito = 30;

            // Act & Assert
            Action action = () => Domain.Proveedores.Entities.Proveedor.Crear(
                nombre,
                nombreContacto,
                email,
                telefono,
                direccion,
                ciudad,
                codigoPostal,
                pais,
                rfcInvalido,
                informacionBancaria,
                diasCredito);
                
            action.Should().Throw<ArgumentException>()
                .WithMessage("*RFC*")
                .WithParameterName("rfc");
        }
        
        [Fact]
        public void CrearProveedor_ConEmailInvalido_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = "Distribuidora XYZ";
            var nombreContacto = "Juan Pérez";
            var rfc = "XYZ123456ABC";
            var telefono = "555-123-4567";
            var emailInvalido = "esto-no-es-un-email";
            var direccion = "Calle Principal 123";
            var ciudad = "Ciudad de México";
            var codigoPostal = "12345";
            var pais = "México";
            var informacionBancaria = "Cuenta 12345-67890";
            var diasCredito = 30;

            // Act & Assert
            Action action = () => Domain.Proveedores.Entities.Proveedor.Crear(
                nombre,
                nombreContacto,
                emailInvalido,
                telefono,
                direccion,
                ciudad,
                codigoPostal,
                pais,
                rfc,
                informacionBancaria,
                diasCredito);
                
            action.Should().Throw<ArgumentException>()
                .WithMessage("*email*")
                .WithParameterName("email");
        }
        
        [Fact]
        public void ActualizarInformacion_ConDatosValidos_DebeActualizarInformacion()
        {
            // Arrange
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                "Nombre Original",
                "Contacto Original",
                "original@xyz.com",
                "555-123-4567",
                "Dirección Original",
                "Ciudad Original",
                "12345",
                "País Original",
                "XYZ123456ABC",
                "Cuenta Original",
                30);
                
            var nuevoNombre = "Nombre Actualizado";
            var nuevoNombreContacto = "Contacto Actualizado";
            var nuevoTelefono = "555-987-6543";
            var nuevoEmail = "actualizado@xyz.com";
            var nuevaDireccion = "Nueva Dirección 456";
            var nuevaCiudad = "Nueva Ciudad";
            var nuevoCodigoPostal = "54321";
            var nuevoPais = "Nuevo País";
            var nuevoRfc = "ABC987654XYZ";
            var nuevaInformacionBancaria = "Nueva Cuenta";
            var nuevosDiasCredito = 45;
            var nuevasObservaciones = "Observaciones actualizadas";
            
            // Act
            proveedor.ActualizarInformacion(
                nuevoNombre,
                nuevoNombreContacto,
                nuevoEmail,
                nuevoTelefono,
                nuevaDireccion,
                nuevaCiudad,
                nuevoCodigoPostal,
                nuevoPais,
                nuevoRfc,
                nuevaInformacionBancaria,
                nuevosDiasCredito);
            
            // Assert
            proveedor.Nombre.Should().Be(nuevoNombre);
            proveedor.NombreContacto.Should().Be(nuevoNombreContacto);
            proveedor.Telefono.Should().Be(nuevoTelefono);
            proveedor.Email.Should().Be(nuevoEmail);
            proveedor.Direccion.Should().Be(nuevaDireccion);
            proveedor.Ciudad.Should().Be(nuevaCiudad);
            proveedor.CodigoPostal.Should().Be(nuevoCodigoPostal);
            proveedor.Pais.Should().Be(nuevoPais);
            proveedor.RFC.Should().Be(nuevoRfc);
            proveedor.InformacionBancaria.Should().Be(nuevaInformacionBancaria);
            proveedor.DiasCredito.Should().Be(nuevosDiasCredito);
            
            // Verificar que se generó el evento de dominio
            proveedor.DomainEvents.Should().Contain(e => e is ProveedorActualizado);
        }
        
        [Fact]
        public void Desactivar_CuandoEstaActivo_DebeCambiarEstadoAInactivo()
        {
            // Arrange
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                "Distribuidora XYZ",
                "Juan Pérez",
                "contacto@xyz.com",
                "555-123-4567",
                "Calle Principal 123",
                "Ciudad de México",
                "12345",
                "México",
                "XYZ123456ABC",
                "Cuenta 12345-67890",
                30);
                
            // Act
            proveedor.Desactivar();
            
            // Assert
            proveedor.Activo.Should().BeFalse();
            
            // Verificar que se generó el evento de dominio
            proveedor.DomainEvents.Should().Contain(e => e is ProveedorDesactivado);
            var evento = proveedor.DomainEvents.OfType<ProveedorDesactivado>().Last();
            evento.ProveedorId.Should().Be(proveedor.Id);
        }
        
        [Fact]
        public void Activar_CuandoEstaInactivo_DebeCambiarEstadoAActivo()
        {
            // Arrange
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                "Distribuidora XYZ",
                "Juan Pérez",
                "contacto@xyz.com",
                "555-123-4567",
                "Calle Principal 123",
                "Ciudad de México",
                "12345",
                "México",
                "XYZ123456ABC",
                "Cuenta 12345-67890",
                30);
                
            proveedor.Desactivar(); // Lo desactivamos primero
            
            // Act
            proveedor.Activar();
            
            // Assert
            proveedor.Activo.Should().BeTrue();
            
            // Verificar que se generó el evento de dominio
            proveedor.DomainEvents.Should().Contain(e => e is ProveedorActivado);
            var evento = proveedor.DomainEvents.OfType<ProveedorActivado>().Last();
            evento.ProveedorId.Should().Be(proveedor.Id);
        }
        
        [Fact]
        public void AgregarContacto_ConDatosValidos_DebeAgregarContacto()
        {
            // Arrange
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                "Distribuidora XYZ",
                "Juan Pérez",
                "contacto@xyz.com",
                "555-123-4567",
                "Calle Principal 123",
                "Ciudad de México",
                "12345",
                "México",
                "XYZ123456ABC",
                "Cuenta 12345-67890",
                30);
                
            var nombreContacto = "Juan Pérez";
            var cargoContacto = "Gerente de Ventas";
            var telefonoContacto = "555-789-1234";
            var emailContacto = "juan@xyz.com";
            
            // Act
            var contacto = proveedor.AgregarContacto(
                nombreContacto,
                cargoContacto,
                telefonoContacto,
                emailContacto);
            
            // Assert
            proveedor.Contactos.Should().HaveCount(1);
            proveedor.Contactos.Should().Contain(contacto);
            
            contacto.ProveedorId.Should().Be(proveedor.Id);
            contacto.Nombre.Should().Be(nombreContacto);
            contacto.Cargo.Should().Be(cargoContacto);
            contacto.Telefono.ToString().Should().Be(telefonoContacto);
            contacto.Email.ToString().Should().Be(emailContacto);
            
            // Verificar que se generó el evento de dominio
            proveedor.DomainEvents.Should().Contain(e => e is ContactoProveedorAgregado);
        }
        
        [Fact]
        public void EliminarContacto_ContactoExistente_DebeEliminarContacto()
        {
            // Arrange
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                "Distribuidora XYZ",
                "Juan Pérez",
                "contacto@xyz.com",
                "555-123-4567",
                "Calle Principal 123",
                "Ciudad de México",
                "12345",
                "México",
                "XYZ123456ABC",
                "Cuenta 12345-67890",
                30);
                
            var contacto = proveedor.AgregarContacto(
                "Juan Pérez",
                "Gerente de Ventas",
                "555-789-1234",
                "juan@xyz.com");
                
            // Act
            proveedor.EliminarContacto(contacto.Id);
            
            // Assert
            proveedor.Contactos.Should().BeEmpty();
            
            // Verificar que se generó el evento de dominio
            proveedor.DomainEvents.Should().Contain(e => e is ContactoProveedorEliminado);
        }
        
        [Fact]
        public void EliminarContacto_ContactoNoExistente_DebeLanzarExcepcion()
        {
            // Arrange
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                "Distribuidora XYZ",
                "Juan Pérez",
                "contacto@xyz.com",
                "555-123-4567",
                "Calle Principal 123",
                "Ciudad de México",
                "12345",
                "México",
                "XYZ123456ABC",
                "Cuenta 12345-67890",
                30);
                
            var contactoIdInexistente = Guid.NewGuid();
            
            // Act & Assert
            Action action = () => proveedor.EliminarContacto(contactoIdInexistente);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*no existe*");
        }
    }
} 
