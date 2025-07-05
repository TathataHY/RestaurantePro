namespace RestaurantePro.Domain.UnitTests.Core.Usuarios.Entities
{
    public class UsuarioTests
    {
        [Fact]
        public void Crear_ConParametrosValidos_DebeCrearUsuarioPendienteConfirmacion()
        {
            // Arrange
            var nombreUsuario = "usuario1";
            var nombreCompleto = "Usuario Uno";
            var email = "usuario1@ejemplo.com";
            var rol = RolUsuario.Cajero;

            // Act
            var usuario = Usuario.Crear(nombreUsuario, nombreCompleto, email, rol);

            // Assert
            usuario.Should().NotBeNull();
            usuario.Id.Should().NotBe(Guid.Empty);
            usuario.NombreUsuario.Should().Be(nombreUsuario);
            usuario.NombreCompleto.Should().Be(nombreCompleto);
            usuario.Email.Should().Be(email);
            usuario.Estado.Should().Be(EstadoUsuario.PendienteConfirmacion);
            usuario.Roles.Should().ContainSingle(r => r == rol);
            usuario.EsAdministrador.Should().BeFalse();
            
            // Verificar evento de dominio
            usuario.DomainEvents.Should().ContainSingle();
            var evento = usuario.DomainEvents.First();
            evento.Should().BeOfType<UsuarioCreado>();
            var usuarioCreado = (UsuarioCreado)evento;
            usuarioCreado.UsuarioId.Should().Be(usuario.Id);
            usuarioCreado.NombreUsuario.Should().Be(nombreUsuario);
            usuarioCreado.Email.Should().Be(email);
            usuarioCreado.Estado.Should().Be(EstadoUsuario.PendienteConfirmacion);
        }
        
        [Theory]
        [InlineData("", "Usuario Uno", "usuario@ejemplo.com")]
        [InlineData(null, "Usuario Uno", "usuario@ejemplo.com")]
        [InlineData("   ", "Usuario Uno", "usuario@ejemplo.com")]
        public void Crear_ConNombreUsuarioInvalido_DebeLanzarExcepcion(string? nombreUsuarioInvalido, string nombreCompleto, string email)
        {
            // Arrange
            var rol = RolUsuario.Cajero;

            // Act & Assert
            Action action = () => Usuario.Crear(nombreUsuarioInvalido!, nombreCompleto, email, rol);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*usuario*");
        }
        
        [Theory]
        [InlineData("usuario1", "Usuario Uno", "")]
        [InlineData("usuario1", "Usuario Uno", null)]
        [InlineData("usuario1", "Usuario Uno", "   ")]
        public void Crear_ConEmailInvalido_DebeLanzarExcepcion(string nombreUsuario, string nombreCompleto, string? emailInvalido)
        {
            // Arrange
            var rol = RolUsuario.Cajero;

            // Act & Assert
            Action action = () => Usuario.Crear(nombreUsuario, nombreCompleto, emailInvalido!, rol);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*email*");
        }
        
        [Theory]
        [InlineData("usuario1", "Usuario Uno", "emailsinestructura")]
        [InlineData("usuario1", "Usuario Uno", "email@sinpunto")]
        [InlineData("usuario1", "Usuario Uno", "@sinuser.com")]
        public void Crear_ConEmailFormatoIncorrecto_DebeLanzarExcepcion(string nombreUsuario, string nombreCompleto, string emailFormatoIncorrecto)
        {
            // Arrange
            var rol = RolUsuario.Cajero;

            // Act & Assert
            Action action = () => Usuario.Crear(nombreUsuario, nombreCompleto, emailFormatoIncorrecto, rol);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*email*formato*");
        }
        
        [Fact]
        public void AsociarIdentity_ConIdValido_DebeAsociarIdentity()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            var identityId = "auth0|123456789";

            // Act
            usuario.AsociarIdentity(identityId);

            // Assert
            usuario.IdentityId.Should().Be(identityId);
            
            // Verificar evento de dominio
            var eventos = usuario.DomainEvents.Where(e => e is UsuarioIdentityAsociado);
            eventos.Should().ContainSingle();
            var evento = (UsuarioIdentityAsociado)eventos.First();
            evento.UsuarioId.Should().Be(usuario.Id);
            evento.IdentityId.Should().Be(identityId);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void AsociarIdentity_ConIdInvalido_DebeLanzarExcepcion(string? identityIdInvalido)
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();

            // Act & Assert
            Action action = () => usuario.AsociarIdentity(identityIdInvalido!);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*ID de Identity*");
        }
        
        [Fact]
        public void ConfirmarCuenta_UsuarioPendiente_DebeConfirmarCuenta()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();

            // Act
            usuario.ConfirmarCuenta();

            // Assert
            usuario.Estado.Should().Be(EstadoUsuario.Activo);
            
            // Verificar evento de dominio
            var eventos = usuario.DomainEvents.Where(e => e is UsuarioConfirmado);
            eventos.Should().ContainSingle();
            var evento = (UsuarioConfirmado)eventos.First();
            evento.UsuarioId.Should().Be(usuario.Id);
        }
        
        [Fact]
        public void ConfirmarCuenta_UsuarioNoEstaPendiente_NoDebeConfirmarCuenta()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            usuario.ConfirmarCuenta(); // Primero confirmamos
            
            // Limpiamos eventos anteriores
            ClearEvents(usuario);

            // Act
            usuario.ConfirmarCuenta(); // Intentamos confirmar nuevamente

            // Assert
            usuario.Estado.Should().Be(EstadoUsuario.Activo);
            
            // No debe generar evento
            var eventos = usuario.DomainEvents.Where(e => e is UsuarioConfirmado);
            eventos.Should().BeEmpty();
        }
        
        [Fact]
        public void Actualizar_ConDatosValidos_DebeActualizarDatos()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            usuario.ConfirmarCuenta();
            
            var nuevoNombreCompleto = "Nombre Actualizado";
            var nuevoEmail = "actualizado@ejemplo.com";
            
            // Act
            usuario.Actualizar(nuevoNombreCompleto, nuevoEmail);
            
            // Assert
            usuario.NombreCompleto.Should().Be(nuevoNombreCompleto);
            usuario.Email.Should().Be(nuevoEmail);
            
            // Verificar evento de dominio
            var eventos = usuario.DomainEvents.Where(e => e is UsuarioActualizado);
            eventos.Should().ContainSingle();
            var evento = (UsuarioActualizado)eventos.First();
            evento.UsuarioId.Should().Be(usuario.Id);
            evento.NombreCompleto.Should().Be(nuevoNombreCompleto);
            evento.Email.Should().Be(nuevoEmail);
        }
        
        [Fact]
        public void RegistrarAcceso_ConFechaValida_DebeRegistrarAcceso()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            usuario.ConfirmarCuenta();
            
            var fechaAcceso = new DateTime(2023, 10, 15, 14, 30, 0);
            
            // Act
            usuario.RegistrarAcceso(fechaAcceso);
            
            // Assert
            usuario.UltimoAcceso.Should().Be(fechaAcceso);
            
            // Verificar evento de dominio
            var eventos = usuario.DomainEvents.Where(e => e is UsuarioAccedio);
            eventos.Should().ContainSingle();
            var evento = (UsuarioAccedio)eventos.First();
            evento.UsuarioId.Should().Be(usuario.Id);
            evento.FechaAcceso.Should().Be(fechaAcceso);
        }
        
        [Fact]
        public void Activar_UsuarioInactivo_DebeActivarUsuario()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            usuario.ConfirmarCuenta();
            usuario.Desactivar();
            
            ClearEvents(usuario);
            
            // Act
            usuario.Activar();
            
            // Assert
            usuario.Estado.Should().Be(EstadoUsuario.Activo);
            
            // Verificar evento de dominio
            var eventos = usuario.DomainEvents.Where(e => e is UsuarioActivado);
            eventos.Should().ContainSingle();
            var evento = (UsuarioActivado)eventos.First();
            evento.UsuarioId.Should().Be(usuario.Id);
        }
        
        [Fact]
        public void Activar_UsuarioBloqueado_DebeLanzarExcepcion()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            usuario.ConfirmarCuenta();
            usuario.Bloquear("Motivo de bloqueo");
            
            // Act & Assert
            Action action = () => usuario.Activar();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*bloqueado*");
        }
        
        [Fact]
        public void Desactivar_UsuarioActivo_DebeDesactivarUsuario()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            usuario.ConfirmarCuenta();
            
            ClearEvents(usuario);
            
            // Act
            usuario.Desactivar();
            
            // Assert
            usuario.Estado.Should().Be(EstadoUsuario.Inactivo);
            
            // Verificar evento de dominio
            var eventos = usuario.DomainEvents.Where(e => e is UsuarioDesactivado);
            eventos.Should().ContainSingle();
            var evento = (UsuarioDesactivado)eventos.First();
            evento.UsuarioId.Should().Be(usuario.Id);
        }
        
        [Fact]
        public void Bloquear_UsuarioActivo_DebeBloquearUsuario()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            usuario.ConfirmarCuenta();
            
            var motivoBloqueo = "Violación de términos de servicio";
            ClearEvents(usuario);
            
            // Act
            usuario.Bloquear(motivoBloqueo);
            
            // Assert
            usuario.Estado.Should().Be(EstadoUsuario.Bloqueado);
            usuario.MotivoBloqueo.Should().Be(motivoBloqueo);
            
            // Verificar evento de dominio
            var eventos = usuario.DomainEvents.Where(e => e is UsuarioBloqueado);
            eventos.Should().ContainSingle();
            var evento = (UsuarioBloqueado)eventos.First();
            evento.UsuarioId.Should().Be(usuario.Id);
            evento.Motivo.Should().Be(motivoBloqueo);
        }
        
        [Fact]
        public void Desbloquear_UsuarioBloqueado_DebeDesbloquearUsuario()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            usuario.ConfirmarCuenta();
            usuario.Bloquear("Violación de términos de servicio");
            
            ClearEvents(usuario);
            
            // Act
            usuario.Desbloquear();
            
            // Assert
            usuario.Estado.Should().Be(EstadoUsuario.Activo);
            usuario.MotivoBloqueo.Should().BeNull();
            
            // Verificar evento de dominio
            var eventos = usuario.DomainEvents.Where(e => e is UsuarioDesbloqueado);
            eventos.Should().ContainSingle();
            var evento = (UsuarioDesbloqueado)eventos.First();
            evento.UsuarioId.Should().Be(usuario.Id);
        }
        
        [Fact]
        public void AsignarRol_RolNoAsignado_DebeAsignarRol()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            var nuevoRol = RolUsuario.Administrador;
            
            ClearEvents(usuario);
            
            // Act
            usuario.AsignarRol(nuevoRol);
            
            // Assert
            usuario.Roles.Should().Contain(nuevoRol);
            usuario.EsAdministrador.Should().BeTrue();
            
            // Verificar evento de dominio
            var eventos = usuario.DomainEvents.Where(e => e is RolAsignado);
            eventos.Should().ContainSingle();
            var evento = (RolAsignado)eventos.First();
            evento.UsuarioId.Should().Be(usuario.Id);
            evento.Rol.Should().Be(nuevoRol);
        }
        
        [Fact]
        public void RemoverRol_RolAsignado_DebeRemoverRol()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            usuario.AsignarRol(RolUsuario.Administrador);
            
            var rolARemover = RolUsuario.Cajero; // El rol original
            ClearEvents(usuario);
            
            // Act
            usuario.RemoverRol(rolARemover);
            
            // Assert
            usuario.Roles.Should().NotContain(rolARemover);
            usuario.Roles.Should().ContainSingle(r => r == RolUsuario.Administrador);
            usuario.EsAdministrador.Should().BeTrue();
            
            // Verificar evento de dominio
            var eventos = usuario.DomainEvents.Where(e => e is RolRemovido);
            eventos.Should().ContainSingle();
            var evento = (RolRemovido)eventos.First();
            evento.UsuarioId.Should().Be(usuario.Id);
            evento.Rol.Should().Be(rolARemover);
        }
        
        [Fact]
        public void RemoverRol_UltimoRol_DebeLanzarExcepcion()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            var unicoRol = RolUsuario.Cajero; // El rol original
            
            // Act & Assert
            Action action = () => usuario.RemoverRol(unicoRol);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*al menos un rol*");
        }
        
        [Fact]
        public void TieneRol_UsuarioConRol_DebeRetornarTrue()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            var rol = RolUsuario.Cajero; // El rol original
            
            // Act
            var resultado = usuario.TieneRol(rol);
            
            // Assert
            resultado.Should().BeTrue();
        }
        
        [Fact]
        public void TieneRol_UsuarioSinRol_DebeRetornarFalse()
        {
            // Arrange
            var usuario = CrearUsuarioPrueba();
            var rolNoAsignado = RolUsuario.Cocinero;
            
            // Act
            var resultado = usuario.TieneRol(rolNoAsignado);
            
            // Assert
            resultado.Should().BeFalse();
        }
        
        #region Helpers
        
        private Usuario CrearUsuarioPrueba()
        {
            return Usuario.Crear(
                "usuario1",
                "Usuario Uno",
                "usuario1@ejemplo.com",
                RolUsuario.Cajero
            );
        }
        
        private void ClearEvents(Usuario usuario)
        {
            var field = typeof(EntityBase).GetField("_domainEvents", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(usuario, new List<DomainEvent>());
        }
        
        #endregion
    }
} 