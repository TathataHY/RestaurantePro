namespace RestaurantePro.Domain.Proveedores.Entities
{
    /// <summary>
    /// Entidad que representa un proveedor de productos o servicios
    /// </summary>
    public class Proveedor : EntityBase
    {
        /// <summary>
        /// Nombre del proveedor
        /// </summary>
        public string Nombre { get; private set; }
        
        /// <summary>
        /// RFC (Registro Federal de Contribuyentes) del proveedor
        /// </summary>
        public string RFC { get; private set; }
        
        /// <summary>
        /// Teléfono principal del proveedor
        /// </summary>
        public PhoneNumber Telefono { get; private set; }
        
        /// <summary>
        /// Email de contacto del proveedor
        /// </summary>
        public Email Email { get; private set; }
        
        /// <summary>
        /// Dirección del proveedor
        /// </summary>
        public string Direccion { get; private set; }
        
        /// <summary>
        /// Observaciones generales sobre el proveedor
        /// </summary>
        public string Observaciones { get; private set; }
        
        /// <summary>
        /// Indica si el proveedor está activo
        /// </summary>
        public bool Activo { get; private set; }
        
        /// <summary>
        /// Lista de contactos del proveedor
        /// </summary>
        private readonly List<ContactoProveedor> _contactos = new List<ContactoProveedor>();
        
        /// <summary>
        /// Acceso de solo lectura a los contactos del proveedor
        /// </summary>
        public IReadOnlyCollection<ContactoProveedor> Contactos => _contactos.AsReadOnly();
        
        // Constructor privado para EF Core
        private Proveedor() { }
        
        /// <summary>
        /// Crea un nuevo proveedor
        /// </summary>
        /// <param name="nombre">Nombre del proveedor</param>
        /// <param name="rfc">RFC del proveedor</param>
        /// <param name="telefono">Teléfono del proveedor</param>
        /// <param name="email">Email del proveedor</param>
        /// <param name="direccion">Dirección del proveedor</param>
        /// <param name="observaciones">Observaciones adicionales</param>
        /// <returns>Nuevo proveedor</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        public static Proveedor Crear(
            string nombre,
            string rfc,
            string telefono,
            string email,
            string direccion,
            string observaciones = null)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del proveedor no puede estar vacío", nameof(nombre));
                
            if (string.IsNullOrWhiteSpace(rfc))
                throw new ArgumentException("El RFC del proveedor no puede estar vacío", nameof(rfc));
                
            if (rfc.Length < 12)
                throw new ArgumentException("El RFC debe tener al menos 12 caracteres", nameof(rfc));
                
            if (string.IsNullOrWhiteSpace(direccion))
                throw new ArgumentException("La dirección del proveedor no puede estar vacía", nameof(direccion));
                
            var emailVO = Email.Create(email);
            var telefonoVO = telefono != null ? PhoneNumber.Create(telefono) : null;
            
            var proveedor = new Proveedor
            {
                Nombre = nombre,
                RFC = rfc,
                Telefono = telefonoVO,
                Email = emailVO,
                Direccion = direccion,
                Observaciones = observaciones ?? string.Empty,
                Activo = true // Por defecto, el proveedor se crea activo
            };
            
            // Agregar evento de creación
            proveedor.AddDomainEvent(new Events.ProveedorCreado(
                proveedor.Id, 
                nombre, 
                rfc, 
                emailVO));
                
            return proveedor;
        }
        
        /// <summary>
        /// Actualiza la información general del proveedor
        /// </summary>
        /// <param name="nombre">Nuevo nombre</param>
        /// <param name="telefono">Nuevo teléfono</param>
        /// <param name="email">Nuevo email</param>
        /// <param name="direccion">Nueva dirección</param>
        /// <param name="observaciones">Nuevas observaciones</param>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        public void ActualizarInformacion(
            string nombre,
            string telefono,
            string email,
            string direccion,
            string observaciones = null)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del proveedor no puede estar vacío", nameof(nombre));
                
            if (string.IsNullOrWhiteSpace(direccion))
                throw new ArgumentException("La dirección del proveedor no puede estar vacía", nameof(direccion));
                
            var emailVO = Email.Create(email);
            var telefonoVO = telefono != null ? PhoneNumber.Create(telefono) : null;
                
            Nombre = nombre;
            Telefono = telefonoVO;
            Email = emailVO;
            Direccion = direccion;
            Observaciones = observaciones ?? string.Empty;
            MarkAsModified();
            
            // Agregar evento de actualización
            AddDomainEvent(new Events.ProveedorActualizado(
                Id, 
                nombre, 
                telefonoVO, 
                emailVO));
        }
        
        /// <summary>
        /// Activa el proveedor
        /// </summary>
        public void Activar()
        {
            if (Activo)
                return; // Ya está activo, no hacemos nada
                
            Activo = true;
            MarkAsModified();
            
            // Agregar evento de activación
            AddDomainEvent(new Events.ProveedorActivado(Id));
        }
        
        /// <summary>
        /// Desactiva el proveedor
        /// </summary>
        public void Desactivar()
        {
            if (!Activo)
                return; // Ya está inactivo, no hacemos nada
                
            Activo = false;
            MarkAsModified();
            
            // Agregar evento de desactivación
            AddDomainEvent(new Events.ProveedorDesactivado(Id));
        }
        
        /// <summary>
        /// Agrega un nuevo contacto al proveedor
        /// </summary>
        /// <param name="nombre">Nombre del contacto</param>
        /// <param name="cargo">Cargo del contacto</param>
        /// <param name="telefono">Teléfono del contacto</param>
        /// <param name="email">Email del contacto</param>
        /// <returns>El contacto agregado</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        public ContactoProveedor AgregarContacto(
            string nombre,
            string cargo,
            string telefono,
            string email)
        {
            var contacto = ContactoProveedor.Crear(Id, nombre, cargo, telefono, email);
            _contactos.Add(contacto);
            MarkAsModified();
            
            // Agregar evento de contacto agregado
            AddDomainEvent(new Events.ContactoProveedorAgregado(
                Id, 
                contacto.Id, 
                nombre, 
                cargo));
                
            return contacto;
        }
        
        /// <summary>
        /// Elimina un contacto del proveedor
        /// </summary>
        /// <param name="contactoId">ID del contacto a eliminar</param>
        /// <exception cref="ArgumentException">Si el contacto no existe</exception>
        public void EliminarContacto(Guid contactoId)
        {
            var contacto = _contactos.FirstOrDefault(c => c.Id == contactoId);
            if (contacto == null)
                throw new ArgumentException($"No existe un contacto con el ID {contactoId} en este proveedor", nameof(contactoId));
                
            _contactos.Remove(contacto);
            MarkAsModified();
            
            // Agregar evento de contacto eliminado
            AddDomainEvent(new Events.ContactoProveedorEliminado(
                Id, 
                contactoId));
        }
    }
} 