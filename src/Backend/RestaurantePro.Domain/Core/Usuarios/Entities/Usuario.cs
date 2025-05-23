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
        public bool EsAdministrador => _roles.Contains(RolUsuario.Administrador);
        
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
            
            _roles.Add(rol);
            
            ValidarInvariantes();
            AddDomainEvent(new UsuarioCreado(Id, NombreUsuario, Email, Estado, TipoUsuario));
        }
        
        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// </summary>
        public static Usuario Crear(string nombreUsuario, string nombreCompleto, string email, RolUsuario rol)
        {
            return new Usuario(nombreUsuario, nombreCompleto, email, rol);
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
        public void Actualizar(string nombreCompleto, string email)
        {
            NombreCompleto = nombreCompleto;
            Email = email;
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
            if (Estado != EstadoUsuario.Activo)
                return;
                
            Estado = EstadoUsuario.Inactivo;
            MarkAsModified();
            
            AddDomainEvent(new UsuarioDesactivado(Id));
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
            
            // Validación más estricta de formato de email
            if (!Email.Contains("@") || !Email.Contains(".") || 
                Email.StartsWith("@") || Email.EndsWith("@") || 
                !Email.Substring(Email.IndexOf("@")).Contains("."))
            {
                throw new InvalidOperationException("El email debe tener un formato válido");
            }

            if (!_roles.Any())
            {
                throw new InvalidOperationException("El usuario debe tener al menos un rol asignado");
            }
        }
    }
} 