namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Entities
{
    public class ClienteTests
    {
        [Fact]
        public void Crear_ConDatosValidos_DebeCrearCliente()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var email = "juan.perez@example.com";
            var telefono = "+34612345678";

            // Act
            var cliente = Cliente.Crear(nombre, email, telefono);

            // Assert
            cliente.Should().NotBeNull();
            cliente.Nombre.Should().Be(nombre);
            cliente.Email.Should().Be(email);
            cliente.Telefono.Should().Be(telefono);
            cliente.EstaActivo.Should().BeTrue();
            cliente.PuntosAcumulados.Should().Be(0);
            cliente.Id.Should().NotBe(Guid.Empty);
            cliente.DomainEvents.Should().ContainSingle(e => e is ClienteCreado);
        }

        [Fact]
        public void AgregarPuntos_ClienteActivo_DebeAgregarPuntos()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            var puntosAAgregar = 50;

            // Act
            cliente.AgregarPuntos(puntosAAgregar);

            // Assert
            cliente.PuntosAcumulados.Should().Be(puntosAAgregar);
            cliente.DomainEvents.Should().ContainSingle(e => e is PuntosAgregados);
            var evento = cliente.DomainEvents.OfType<PuntosAgregados>().First();
            evento.Cantidad.Should().Be(puntosAAgregar);
            evento.PuntosTotales.Should().Be(puntosAAgregar);
        }

        [Fact]
        public void AgregarPuntos_ClienteInactivo_DebeLanzarException()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.Desactivar();

            // Act
            Action action = () => cliente.AgregarPuntos(50);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*inactivo*");
        }

        [Fact]
        public void AgregarPuntos_CantidadNegativa_DebeLanzarException()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");

            // Act
            Action action = () => cliente.AgregarPuntos(-10);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*mayor a cero*");
        }

        [Fact]
        public void Desactivar_ClienteActivo_DebeDesactivarCliente()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");

            // Act
            cliente.Desactivar();

            // Assert
            cliente.EstaActivo.Should().BeFalse();
            cliente.DomainEvents.Should().ContainSingle(e => e is ClienteDesactivado);
        }

        [Fact]
        public void Desactivar_ClienteYaInactivo_NoDebeGenerarEvento()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.Desactivar();
            cliente.ClearDomainEvents();

            // Act
            cliente.Desactivar();

            // Assert
            cliente.EstaActivo.Should().BeFalse();
            cliente.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Reactivar_ClienteInactivo_DebeReactivarCliente()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.Desactivar();
            cliente.ClearDomainEvents();

            // Act
            cliente.Reactivar();

            // Assert
            cliente.EstaActivo.Should().BeTrue();
            cliente.DomainEvents.Should().ContainSingle(e => e is ClienteReactivado);
        }

        [Fact]
        public void ActualizarInformacionContacto_NuevosValues_DebeActualizarInformacion()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            var nuevoEmail = "nuevo.juan@example.com";
            var nuevoTelefono = "687654321";

            // Act
            cliente.ActualizarInformacionContacto(nuevoEmail, nuevoTelefono);

            // Assert
            cliente.Email.Should().Be(nuevoEmail);
            cliente.Telefono.Should().Be(nuevoTelefono);
            cliente.DomainEvents.Should().ContainSingle(e => e is InformacionContactoActualizada);
        }

        [Fact]
        public void ActualizarInformacionContacto_MismosValues_NoDebeGenerarEvento()
        {
            // Arrange
            var email = "juan@example.com";
            var telefono = "612345678";
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, email, telefono);
            cliente.ClearDomainEvents();

            // Act
            cliente.ActualizarInformacionContacto(email, telefono);

            // Assert
            cliente.Email.Should().Be(email);
            cliente.Telefono.Should().Be(telefono);
            cliente.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void AsociarTarjetaFidelizacion_ClienteActivo_DebeAsociarTarjeta()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            var tarjetaId = Guid.NewGuid();

            // Act
            cliente.AsociarTarjetaFidelizacion(tarjetaId);

            // Assert
            cliente.TarjetaFidelizacionPrincipalId.Should().Be(tarjetaId);
            cliente.DomainEvents.Should().ContainSingle(e => e is TarjetaFidelizacionAsociada);
            var evento = cliente.DomainEvents.OfType<TarjetaFidelizacionAsociada>().First();
            evento.ClienteId.Should().Be(cliente.Id);
            evento.TarjetaFidelizacionId.Should().Be(tarjetaId);
        }

        [Fact]
        public void AsociarTarjetaFidelizacion_ClienteInactivo_DebeLanzarException()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.Desactivar();
            var tarjetaId = Guid.NewGuid();

            // Act
            Action action = () => cliente.AsociarTarjetaFidelizacion(tarjetaId);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*inactivo*");
        }

        [Fact]
        public void RestarPuntos_ClienteActivoConPuntosSuficientes_DebeRestarPuntos()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.AgregarPuntos(100);
            cliente.ClearDomainEvents();
            var puntosARestar = 50;
            var motivo = "Descuento en próxima visita";

            // Act
            cliente.RestarPuntos(puntosARestar, motivo);

            // Assert
            cliente.PuntosAcumulados.Should().Be(50);
            cliente.DomainEvents.Should().ContainSingle(e => e is PuntosFidelizacionCanjeados);
            var evento = cliente.DomainEvents.OfType<PuntosFidelizacionCanjeados>().First();
            evento.ClienteId.Should().Be(cliente.Id);
            evento.Cantidad.Should().Be(puntosARestar);
            evento.PuntosRestantes.Should().Be(50);
            evento.Motivo.Should().Be(motivo);
        }

        [Fact]
        public void RestarPuntos_ClienteInactivo_DebeLanzarException()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.AgregarPuntos(100);
            cliente.Desactivar();

            // Act
            Action action = () => cliente.RestarPuntos(50, "Descuento");

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*inactivo*");
        }

        [Fact]
        public void RestarPuntos_PuntosInsuficientes_DebeLanzarException()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.AgregarPuntos(30);

            // Act
            Action action = () => cliente.RestarPuntos(50, "Descuento");

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*No hay suficientes puntos*");
        }

        [Fact]
        public void RestarPuntos_CantidadNegativa_DebeLanzarException()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.AgregarPuntos(100);

            // Act
            Action action = () => cliente.RestarPuntos(-10, "Descuento");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*mayor a cero*");
        }

        #region Pruebas de Validación de Invariantes
        
        [Fact]
        public void ValidarInvariantes_PuntosNegativos_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            
            // Manipular directamente los puntos para crear inconsistencia
            typeof(Cliente)
                .GetProperty("PuntosAcumulados")
                .SetValue(cliente, -50); // Valor inválido
            
            // Act
            Action action = () => cliente.AgregarPuntos(10); // Esta operación llamará a ValidarInvariantes
            
            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*puntos acumulados*negativos*");
        }
        
        [Fact]
        public void ValidarInvariantes_VisitasNegativas_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            
            // Manipular directamente las visitas para crear inconsistencia
            typeof(Cliente)
                .GetProperty("CantidadVisitas")
                .SetValue(cliente, -5); // Valor inválido
            
            // Act
            Action action = () => cliente.RegistrarVisita(); // Esta operación llamará a ValidarInvariantes
            
            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*visitas*negativa*");
        }
        
        [Fact]
        public void ValidarInvariantes_EmailInvalido_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            
            // Manipular directamente el email para crear inconsistencia
            typeof(Cliente)
                .GetProperty("Email")
                .SetValue(cliente, "emailinvalido"); // Formato inválido sin @ ni punto
            
            // Act
            Action action = () => cliente.ActualizarInformacionContacto("nuevoemailtambieninvalido", "612345678"); 
            
            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*email*no tiene un formato válido*");
        }
        
        [Fact]
        public void ValidarInvariantes_TelefonoVacio_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            
            // Manipular directamente el teléfono para crear inconsistencia
            typeof(Cliente)
                .GetProperty("Telefono")
                .SetValue(cliente, " "); // Valor inválido (espacio en blanco)
            
            // Act - Llamar a ValidarInvariantes usando reflexión
            Action action = () => typeof(Cliente)
                .GetMethod("ValidarInvariantes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(cliente, null);
            
            // Assert
            action.Should().Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("*teléfono*no puede estar vacío*");
        }
        
        [Fact]
        public void ValidarInvariantes_NombreNulo_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            
            // Manipular directamente el nombre para crear inconsistencia
            typeof(Cliente)
                .GetProperty("Nombre")
                .SetValue(cliente, null); // Valor inválido (null)
            
            // Act
            Action action = () => cliente.RegistrarVisita(); // Esta operación llamará a ValidarInvariantes
            
            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*nombre*no puede ser nulo*");
        }
        
        #endregion
    }
}

