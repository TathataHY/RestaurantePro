namespace RestaurantePro.Domain.Proveedores.Entities
{
    /// <summary>
    /// Entidad que representa un contacto de un proveedor
    /// </summary>
    public class ContactoProveedor : EntityBase
    {
        /// <summary>
        /// ID del proveedor al que pertenece este contacto
        /// </summary>
        public Guid ProveedorId { get; private set; }
        
        /// <summary>
        /// Nombre del contacto
        /// </summary>
        public string Nombre { get; private set; }
        
        /// <summary>
        /// Cargo del contacto en la empresa
        /// </summary>
        public string Cargo { get; private set; }
        
        /// <summary>
        /// Teléfono del contacto
        /// </summary>
        public PhoneNumber Telefono { get; private set; }
        
        /// <summary>
        /// Email del contacto
        /// </summary>
        public Email Email { get; private set; }
        
        // Constructor privado para EF Core
        private ContactoProveedor() { }
        
        /// <summary>
        /// Crea un nuevo contacto para un proveedor
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombre">Nombre del contacto</param>
        /// <param name="cargo">Cargo del contacto</param>
        /// <param name="telefono">Teléfono del contacto</param>
        /// <param name="email">Email del contacto</param>
        /// <returns>Nuevo contacto de proveedor</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        internal static ContactoProveedor Crear(
            Guid proveedorId,
            string nombre,
            string cargo,
            string telefono,
            string email)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del contacto no puede estar vacío", nameof(nombre));
                
            if (string.IsNullOrWhiteSpace(cargo))
                throw new ArgumentException("El cargo del contacto no puede estar vacío", nameof(cargo));
                
            var emailVO = Email.Create(email);
            var telefonoVO = telefono != null ? PhoneNumber.Create(telefono) : null;
                
            var contacto = new ContactoProveedor
            {
                ProveedorId = proveedorId,
                Nombre = nombre,
                Cargo = cargo,
                Telefono = telefonoVO,
                Email = emailVO
            };
            
            return contacto;
        }
        
        /// <summary>
        /// Actualiza la información del contacto
        /// </summary>
        /// <param name="nombre">Nuevo nombre</param>
        /// <param name="cargo">Nuevo cargo</param>
        /// <param name="telefono">Nuevo teléfono</param>
        /// <param name="email">Nuevo email</param>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        public void ActualizarInformacion(
            string nombre,
            string cargo,
            string telefono,
            string email)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del contacto no puede estar vacío", nameof(nombre));
                
            if (string.IsNullOrWhiteSpace(cargo))
                throw new ArgumentException("El cargo del contacto no puede estar vacío", nameof(cargo));
                
            var emailVO = Email.Create(email);
            var telefonoVO = telefono != null ? PhoneNumber.Create(telefono) : null;
                
            Nombre = nombre;
            Cargo = cargo;
            Telefono = telefonoVO;
            Email = emailVO;
            MarkAsModified();
        }
    }
} 