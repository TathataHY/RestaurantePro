namespace RestaurantePro.Domain.Core.Usuarios.Entities
{
    /// <summary>
    /// Agregado que representa un usuario del sistema.
    /// 
    /// Invariantes:
    /// - El nombre de usuario no puede estar vacío
    /// - El email debe ser válido
    /// - El usuario debe tener al menos un rol asignado
    /// 
    /// Ciclo de vida:
    /// - Creación/PendienteConfirmación → [Confirmación → Activo]
    ///                                  ↘ [Expiración → Inactivo]
    /// - Activo → [Actualización → Activo]
    ///         ↘ [Desactivación → Inactivo → Activación → Activo]
    ///         ↘ [Bloqueo → Bloqueado → Desbloqueo → Activo]
    /// 
    /// Reglas de negocio:
    /// - Un usuario desactivado puede volver a activarse
    /// - Un usuario bloqueado solo puede desbloquearse por un administrador
    /// - El cambio de roles solo puede realizarse sobre usuarios activos
    /// - Cada cambio de estado genera eventos de dominio
    /// </summary>
    public class Usuario : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Nombre de usuario para login
        /// </summary>
        public string NombreUsuario { get; private set; }

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string NombreCompleto { get; private set; }

        /// <summary>
        /// Email del usuario
        /// </summary>
        public string Email { get; private set; }
        
        /// <summary>
        /// Referencia al identificador del usuario en el sistema de autenticación
        /// </summary>
        public string? IdentityId { get; private set; }
        
        /// <summary>
        /// Estado actual del usuario
        /// </summary>
        public EstadoUsuario Estado { get; private set; }
        
        /// <summary>
        /// Tipo de usuario
        /// </summary>
        public TipoUsuario TipoUsuario { get; private set; }

        /// <summary>
        /// Hash de la contraseña del usuario
        /// </summary>
        public string? PasswordHash { get; private set; }
        
        /// <summary>
        /// Salt usado para el hash de la contraseña
        /// </summary>
        public string? Salt { get; private set; }
        
        /// <summary>
        /// Rol principal del usuario en el sistema
        /// </summary>
        public string Rol { get; private set; }
        
        /// <summary>
        /// Nivel de acceso del usuario (1-10, donde 10 es máximo)
        /// </summary>
        public int NivelAcceso { get; private set; }
        
        /// <summary>
        /// Lista de permisos específicos del usuario
        /// </summary>
        private readonly List<string> _permisos = new();
        
        /// <summary>
        /// Permisos específicos del usuario (solo lectura)
        /// </summary>
        public IReadOnlyCollection<string> Permisos => _permisos.AsReadOnly();
        
        /// <summary>
        /// ID del supervisor directo del usuario
        /// </summary>
        public Guid? SupervisorId { get; private set; }
        
        /// <summary>
        /// Departamento al que pertenece el usuario
        /// </summary>
        public string? Departamento { get; private set; }
        
        /// <summary>
        /// Posición o cargo del usuario
        /// </summary>
        public string? Posicion { get; private set; }
        
        /// <summary>
        /// Teléfono del usuario
        /// </summary>
        public string? Telefono { get; private set; }
        
        /// <summary>
        /// Identificación fiscal/legal del usuario (DNI, Cédula, etc.)
        /// </summary>
        public string? Identificacion { get; private set; }
        
        /// <summary>
        /// Roles asignados al usuario
        /// </summary>
        private readonly List<RolUsuario> _roles = new();
        
        /// <summary>
        /// Colección de roles asignados al usuario (solo lectura)
        /// </summary>
        public IReadOnlyCollection<RolUsuario> Roles => _roles.AsReadOnly();
        
        /// <summary>
        /// Fecha de último acceso al sistema
        /// </summary>
        public DateTime? UltimoAcceso { get; private set; }
        
        /// <summary>
        /// Motivo de bloqueo si el usuario está bloqueado
        /// </summary>
        public string? MotivoBloqueo { get; private set; }
        
        /// <summary>
        /// Indica si el usuario es un administrador del sistema
        /// </summary>
        public bool EsAdministrador => _roles.Contains(RolUsuario.Administrador) || Rol == "Administrador";
        
        protected Usuario() { }
        
        private Usuario(string nombreUsuario, string nombreCompleto, string email, RolUsuario rol)
        {
            Id = Guid.NewGuid();
            NombreUsuario = nombreUsuario;
            NombreCompleto = nombreCompleto;
            Email = email;
            Estado = EstadoUsuario.PendienteConfirmacion;
            MotivoBloqueo = null;
            UltimoAcceso = null;
            TipoUsuario = TipoUsuario.Empleado;
            
            Rol = MapearRolUsuarioAString(rol);
            NivelAcceso = AsignarNivelAccesoSegunRol(rol);
            
            _roles.Add(rol);
            
            ValidarInvariantes();
            // El evento se dispara desde el método Crear() para incluir la contraseña
        }
        
        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// </summary>
        public static Usuario Crear(string nombreUsuario, string nombreCompleto, string email, RolUsuario rol, string password = "")
        {
            var usuario = new Usuario(nombreUsuario, nombreCompleto, email, rol);
            // Actualizar el evento para incluir la contraseña y el rol
            usuario.ClearDomainEvents();
            usuario.AddDomainEvent(new UsuarioCreado(usuario.Id, usuario.NombreUsuario, usuario.Email, usuario.Estado, password, usuario.Rol, usuario.TipoUsuario));
            return usuario;
        }
        
        /// <summary>
        /// Asocia el usuario con su cuenta de Identity
        /// </summary>
        public void AsociarIdentity(string identityId)
        {
            if (string.IsNullOrWhiteSpace(identityId))
                throw new InvalidOperationException("El ID de Identity no puede estar vacío");
                
            IdentityId = identityId;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioIdentityAsociado(Id, identityId));
        }
        
        /// <summary>
        /// Confirma la cuenta del usuario
        /// </summary>
        public void ConfirmarCuenta()
        {
            if (Estado != EstadoUsuario.PendienteConfirmacion)
                return;
                
            Estado = EstadoUsuario.Activo;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioConfirmado(Id));
        }
        
        /// <summary>
        /// Actualiza los datos del usuario
        /// </summary>
        public void Actualizar(string nombreCompleto, string email, string? telefono = null, string? nombreUsuario = null)
        {
            NombreCompleto = nombreCompleto;
            Email = email;
            if (telefono != null)
            {
                Telefono = telefono;
            }
            if (nombreUsuario != null)
            {
                NombreUsuario = nombreUsuario;
            }
            MarkAsModified();
            
            ValidarInvariantes();
            AddDomainEvent(new UsuarioActualizado(Id, NombreCompleto, Email));
        }
        
        /// <summary>
        /// Registra un acceso del usuario al sistema
        /// </summary>
        public void RegistrarAcceso(DateTime fechaAcceso)
        {
            UltimoAcceso = fechaAcceso;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioAccedio(Id, fechaAcceso));
        }
        
        /// <summary>
        /// Activa un usuario inactivo
        /// </summary>
        public void Activar()
        {
            if (Estado == EstadoUsuario.Activo)
                return;
                
            if (Estado == EstadoUsuario.Bloqueado)
                throw new InvalidOperationException("No se puede activar un usuario bloqueado, debe desbloquearse primero");
                
            Estado = EstadoUsuario.Activo;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioActivado(Id));
        }
        
        /// <summary>
        /// Desactiva un usuario activo
        /// </summary>
        public void Desactivar()
        {
            // Log de debugging
            Console.WriteLine($"🔍 Desactivar() llamado - Estado actual: {Estado}");
            
            if (Estado == EstadoUsuario.Inactivo || Estado == EstadoUsuario.Bloqueado)
            {
                Console.WriteLine($"🔍 Usuario ya está {Estado}, retornando sin cambios");
                return;
            }
                
            Console.WriteLine($"🔍 Cambiando estado de {Estado} a Inactivo");
            Estado = EstadoUsuario.Inactivo;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioDesactivado(Id));
            Console.WriteLine($"🔍 Usuario desactivado exitosamente - Nuevo estado: {Estado}");
        }
        
        /// <summary>
        /// Bloquea un usuario por infracciones o políticas de seguridad
        /// </summary>
        public void Bloquear(string motivo)
        {
            if (Estado == EstadoUsuario.Bloqueado)
                return;
                
            Estado = EstadoUsuario.Bloqueado;
            MotivoBloqueo = motivo;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioBloqueado(Id, motivo));
        }
        
        /// <summary>
        /// Desbloquea un usuario bloqueado
        /// </summary>
        public void Desbloquear()
        {
            if (Estado != EstadoUsuario.Bloqueado)
                return;
                
            Estado = EstadoUsuario.Activo;
            MotivoBloqueo = null;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioDesbloqueado(Id));
        }
        
        /// <summary>
        /// Asigna un nuevo rol al usuario
        /// </summary>
        public void AsignarRol(RolUsuario rol)
        {
            if (_roles.Contains(rol))
                return;
                
            _roles.Add(rol);
            MarkAsModified();
            
            AddDomainEvent(new RolAsignado(Id, rol));
        }
        
        /// <summary>
        /// Asigna un nuevo rol al usuario mediante su identificador
        /// </summary>
        public void AsignarRol(Guid rolId)
        {
            MarkAsModified();
            
            AddDomainEvent(new RolAsignadoPorId(Id, rolId));
        }
        
        /// <summary>
        /// Remueve un rol asignado al usuario
        /// </summary>
        public void RemoverRol(RolUsuario rol)
        {
            if (!_roles.Contains(rol))
                return;
                
            if (_roles.Count == 1)
                throw new InvalidOperationException("El usuario debe tener al menos un rol asignado");
                
            _roles.Remove(rol);
            MarkAsModified();
            
            AddDomainEvent(new RolRemovido(Id, rol));
        }
        
        /// <summary>
        /// Verifica si el usuario tiene un rol específico
        /// </summary>
        public bool TieneRol(RolUsuario rol)
        {
            return _roles.Contains(rol);
        }
        
        /// <summary>
        /// Establece el tipo de usuario
        /// </summary>
        public void EstablecerTipo(TipoUsuario tipoUsuario)
        {
            if (TipoUsuario == tipoUsuario)
                return;
                
            TipoUsuario = tipoUsuario;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioTipoActualizado(Id, TipoUsuario));
        }
        
        /// <summary>
        /// Establece la contraseña hasheada del usuario
        /// </summary>
        public void EstablecerPassword(string passwordHash, string salt)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new InvalidOperationException("El hash de la contraseña no puede estar vacío");
                
            if (string.IsNullOrWhiteSpace(salt))
                throw new InvalidOperationException("El salt no puede estar vacío");
                
            PasswordHash = passwordHash;
            Salt = salt;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioPasswordCambiado(Id));
        }
        
        /// <summary>
        /// Establece el rol principal del usuario
        /// </summary>
        public void EstablecerRol(string rol, int nivelAcceso)
        {
            if (string.IsNullOrWhiteSpace(rol))
                throw new InvalidOperationException("El rol no puede estar vacío");
                
            if (nivelAcceso < 1 || nivelAcceso > 10)
                throw new InvalidOperationException("El nivel de acceso debe estar entre 1 y 10");
                
            Rol = rol;
            NivelAcceso = nivelAcceso;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioRolCambiado(Id, rol, nivelAcceso));
        }
        
        /// <summary>
        /// Agrega un permiso específico al usuario
        /// </summary>
        public void AgregarPermiso(string permiso)
        {
            if (string.IsNullOrWhiteSpace(permiso))
                throw new InvalidOperationException("El permiso no puede estar vacío");
                
            if (!_permisos.Contains(permiso))
            {
                _permisos.Add(permiso);
                MarkAsModified();
                
                AddDomainEvent(new UsuarioPermisoAgregado(Id, permiso));
            }
        }
        
        /// <summary>
        /// Remueve un permiso específico del usuario
        /// </summary>
        public void RemoverPermiso(string permiso)
        {
            if (_permisos.Remove(permiso))
            {
                MarkAsModified();
                AddDomainEvent(new UsuarioPermisoRemovido(Id, permiso));
            }
        }
        
        /// <summary>
        /// Verifica si el usuario tiene un permiso específico
        /// </summary>
        public bool TienePermiso(string permiso)
        {
            return _permisos.Contains(permiso) || EsAdministrador;
        }
        
        /// <summary>
        /// Establece la información organizacional del usuario
        /// </summary>
        public void EstablecerInformacionOrganizacional(Guid? supervisorId, string? departamento, string? posicion)
        {
            SupervisorId = supervisorId;
            Departamento = departamento;
            Posicion = posicion;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioInformacionOrganizacionalActualizada(Id, supervisorId, departamento, posicion));
        }
        
        /// <summary>
        /// Establece la identificación del usuario
        /// </summary>
        public void EstablecerIdentificacion(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
                throw new InvalidOperationException("La identificación no puede estar vacía");
                
            Identificacion = identificacion;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioIdentificacionEstablecida(Id, identificacion));
        }
        
        /// <summary>
        /// Elimina lógicamente el usuario (soft delete)
        /// </summary>
        public void EliminarLogicamente()
        {
            if (EstaEliminado)
                return; // Ya está eliminado usando EntityBase
                
            MarkAsDeleted(); // Usar método de EntityBase
            
            AddDomainEvent(new UsuarioEliminadoLogicamente(Id, DateTime.UtcNow));
        }
        
        /// <summary>
        /// Restaura un usuario eliminado lógicamente
        /// </summary>
        public void RestaurarDeEliminacionLogica()
        {
            if (!EstaEliminado)
                return; // No está eliminado
                
            // Revertir la eliminación lógica de EntityBase
            var propertyInfo = typeof(EntityBase).GetProperty("EstaEliminado", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            propertyInfo?.SetValue(this, false);
            
            Estado = EstadoUsuario.Activo;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioRestauradoDeEliminacion(Id));
        }
        
        /// <summary>
        /// Mapea un RolUsuario enum a string
        /// </summary>
        private static string MapearRolUsuarioAString(RolUsuario rol)
        {
            return rol switch
            {
                RolUsuario.Administrador => "Administrador",
                RolUsuario.Gerente => "Gerente",
                RolUsuario.Cajero => "Cajero",
                RolUsuario.Mesero => "Mesero",
                RolUsuario.Cocinero => "Cocinero",
                RolUsuario.EncargadoInventario => "EncargadoInventario",
                RolUsuario.Empleado => "Empleado",
                _ => "Empleado"
            };
        }
        
        /// <summary>
        /// Asigna nivel de acceso según el rol
        /// </summary>
        private static int AsignarNivelAccesoSegunRol(RolUsuario rol)
        {
            return rol switch
            {
                RolUsuario.Administrador => 10,
                RolUsuario.Gerente => 8,
                RolUsuario.EncargadoInventario => 6,
                RolUsuario.Cajero => 4,
                RolUsuario.Mesero => 3,
                RolUsuario.Cocinero => 3,
                RolUsuario.Empleado => 2,
                _ => 1
            };
        }
        
        /// <summary>
        /// Valida las invariantes del agregado Usuario
        /// </summary>
        private void ValidarInvariantes()
        {
            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                throw new InvalidOperationException("El nombre de usuario no puede estar vacío");
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                throw new InvalidOperationException("El email no puede estar vacío");
            }
            
            // Validación de formato de email más robusta
            if (!EsEmailValido(Email))
            {
                throw new InvalidOperationException("El email debe tener un formato válido");
            }

            if (!_roles.Any())
            {
                throw new InvalidOperationException("El usuario debe tener al menos un rol asignado");
            }
        }
        
        /// <summary>
        /// Valida si un email tiene un formato válido
        /// </summary>
        private static bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Usar expresión regular más robusta para validar email
                var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return System.Text.RegularExpressions.Regex.IsMatch(email, emailRegex);
            }
            catch
            {
                return false;
            }
        }
    }
} 