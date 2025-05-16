namespace RestaurantePro.Domain.UnitTests.Proveedores.Entities
{
    public class ProveedorTests
    {
        [Fact]
        public void CrearProveedor_ConParametrosValidos_DebeCrearProveedorActivo()
        {
            // Arrange
            var nombre = "Distribuidora XYZ";
            var rfc = "XYZ123456ABC";
            var telefono = "555-123-4567";
            var email = "contacto@xyz.com";
            var direccion = "Calle Principal 123, Ciudad";
            var observaciones = "Proveedor principal de lácteos";

            // Act
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                nombre,
                rfc,
                telefono,
                email,
                direccion,
                observaciones);

            // Assert
            proveedor.Should().NotBeNull();
            proveedor.Id.Should().NotBe(Guid.Empty);
            proveedor.Nombre.Should().Be(nombre);
            proveedor.RFC.Should().Be(rfc);
            proveedor.Telefono.Should().Be(telefono);
            proveedor.Email.Should().Be(email);
            proveedor.Direccion.Should().Be(direccion);
            proveedor.Observaciones.Should().Be(observaciones);
            proveedor.Activo.Should().BeTrue();
            
            // Verificamos que se generó el evento de dominio
            proveedor.DomainEvents.Should().ContainSingle(e => e is ProveedorCreado);
            var evento = proveedor.DomainEvents.OfType<ProveedorCreado>().First();
            evento.ProveedorId.Should().Be(proveedor.Id);
            evento.Nombre.Should().Be(nombre);
        }

        [Fact]
        public void CrearProveedor_ConNombreVacio_DebeLanzarExcepcion()
        {
            // Arrange
            var nombreVacio = string.Empty;
            var rfc = "XYZ123456ABC";
            var telefono = "555-123-4567";
            var email = "contacto@xyz.com";
            var direccion = "Calle Principal 123, Ciudad";

            // Act & Assert
            Action action = () => Domain.Proveedores.Entities.Proveedor.Crear(
                nombreVacio, 
                rfc, 
                telefono, 
                email, 
                direccion);
                
            action.Should().Throw<ArgumentException>()
                .WithMessage("*nombre*")
                .WithParameterName("nombre");
        }
        
        [Fact]
        public void CrearProveedor_ConRFCInvalido_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = "Distribuidora XYZ";
            var rfcInvalido = "123"; // RFC demasiado corto
            var telefono = "555-123-4567";
            var email = "contacto@xyz.com";
            var direccion = "Calle Principal 123, Ciudad";

            // Act & Assert
            Action action = () => Domain.Proveedores.Entities.Proveedor.Crear(
                nombre,
                rfcInvalido,
                telefono,
                email,
                direccion);
                
            action.Should().Throw<ArgumentException>()
                .WithMessage("*RFC*")
                .WithParameterName("rfc");
        }
        
        [Fact]
        public void CrearProveedor_ConEmailInvalido_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = "Distribuidora XYZ";
            var rfc = "XYZ123456ABC";
            var telefono = "555-123-4567";
            var emailInvalido = "esto-no-es-un-email";
            var direccion = "Calle Principal 123, Ciudad";

            // Act & Assert
            Action action = () => Domain.Proveedores.Entities.Proveedor.Crear(
                nombre,
                rfc,
                telefono,
                emailInvalido,
                direccion);
                
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
                "XYZ123456ABC",
                "555-123-4567",
                "original@xyz.com",
                "Dirección Original");
                
            var nuevoNombre = "Nombre Actualizado";
            var nuevoTelefono = "555-987-6543";
            var nuevoEmail = "actualizado@xyz.com";
            var nuevaDireccion = "Nueva Dirección 456";
            var nuevasObservaciones = "Observaciones actualizadas";
            
            // Act
            proveedor.ActualizarInformacion(
                nuevoNombre,
                nuevoTelefono,
                nuevoEmail,
                nuevaDireccion,
                nuevasObservaciones);
            
            // Assert
            proveedor.Nombre.Should().Be(nuevoNombre);
            proveedor.Telefono.Should().Be(nuevoTelefono);
            proveedor.Email.Should().Be(nuevoEmail);
            proveedor.Direccion.Should().Be(nuevaDireccion);
            proveedor.Observaciones.Should().Be(nuevasObservaciones);
            
            // Verificar que se generó el evento de dominio
            proveedor.DomainEvents.Should().Contain(e => e is ProveedorActualizado);
        }
        
        [Fact]
        public void Desactivar_CuandoEstaActivo_DebeCambiarEstadoAInactivo()
        {
            // Arrange
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                "Distribuidora XYZ",
                "XYZ123456ABC",
                "555-123-4567",
                "contacto@xyz.com",
                "Calle Principal 123, Ciudad");
                
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
                "XYZ123456ABC",
                "555-123-4567",
                "contacto@xyz.com",
                "Calle Principal 123, Ciudad");
                
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
                "XYZ123456ABC",
                "555-123-4567",
                "contacto@xyz.com",
                "Calle Principal 123, Ciudad");
                
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
            contacto.Telefono.Should().Be(telefonoContacto);
            contacto.Email.Should().Be(emailContacto);
            
            // Verificar que se generó el evento de dominio
            proveedor.DomainEvents.Should().Contain(e => e is ContactoProveedorAgregado);
        }
        
        [Fact]
        public void EliminarContacto_ContactoExistente_DebeEliminarContacto()
        {
            // Arrange
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                "Distribuidora XYZ",
                "XYZ123456ABC",
                "555-123-4567",
                "contacto@xyz.com",
                "Calle Principal 123, Ciudad");
                
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
                "XYZ123456ABC",
                "555-123-4567",
                "contacto@xyz.com",
                "Calle Principal 123, Ciudad");
                
            var contactoIdInexistente = Guid.NewGuid();
            
            // Act & Assert
            Action action = () => proveedor.EliminarContacto(contactoIdInexistente);
            action.Should().Throw<ArgumentException>()
                .WithMessage("*no existe*");
        }
    }
} 