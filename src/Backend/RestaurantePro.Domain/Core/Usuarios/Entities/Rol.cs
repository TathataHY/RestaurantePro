namespace RestaurantePro.Domain.Core.Usuarios.Entities
{
    /// <summary>
    /// Entidad que representa un rol de usuario en el sistema
    /// </summary>
    public class Rol : EntityBase
    {
        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Nombre { get; private set; }
        
        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string Descripcion { get; private set; }
        
        /// <summary>
        /// Tipo de usuario asociado con este rol
        /// </summary>
        public TipoUsuario TipoUsuario { get; private set; }
        
        /// <summary>
        /// Indica si este rol es asignable a usuarios
        /// </summary>
        public bool EsAsignable { get; private set; }
        
        /// <summary>
        /// Indica si este rol es el predeterminado para nuevos usuarios
        /// </summary>
        public bool EsPredeterminado { get; private set; }
        
        /// <summary>
        /// Constructor protegido para EF Core
        /// </summary>
        protected Rol() { }
        
        /// <summary>
        /// Constructor privado utilizado por el factory method
        /// </summary>
        private Rol(string nombre, string descripcion, TipoUsuario tipoUsuario, bool esAsignable, bool esPredeterminado)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            TipoUsuario = tipoUsuario;
            EsAsignable = esAsignable;
            EsPredeterminado = esPredeterminado;
        }
        
        /// <summary>
        /// Factory method para crear un nuevo rol
        /// </summary>
        public static Rol Crear(string nombre, string descripcion, TipoUsuario tipoUsuario, bool esAsignable = true, bool esPredeterminado = false)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(nombre));
                
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ArgumentException("La descripción del rol no puede estar vacía", nameof(descripcion));
                
            var rol = new Rol(nombre, descripcion, tipoUsuario, esAsignable, esPredeterminado);
            
            // Agregar evento de dominio
            rol.AddDomainEvent(new RolCreado(rol.Id, nombre, tipoUsuario));
            
            return rol;
        }
        
        /// <summary>
        /// Actualiza las propiedades del rol
        /// </summary>
        public void Actualizar(string nombre, string descripcion, bool esAsignable)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(nombre));
                
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ArgumentException("La descripción del rol no puede estar vacía", nameof(descripcion));
                
            var nombreAnterior = Nombre;
            
            Nombre = nombre;
            Descripcion = descripcion;
            EsAsignable = esAsignable;
            
            MarkAsModified();
            
            // Agregar evento de dominio
            AddDomainEvent(new RolActualizado(Id, nombreAnterior, nombre, TipoUsuario, esAsignable));
        }
        
        /// <summary>
        /// Establece un rol como predeterminado
        /// </summary>
        public void EstablecerComoPredeterminado(bool esPredeterminado)
        {
            if (EsPredeterminado == esPredeterminado)
                return;
                
            EsPredeterminado = esPredeterminado;
            MarkAsModified();
            
            // Agregar evento de dominio
            AddDomainEvent(new RolPredeterminadoActualizado(Id, Nombre, esPredeterminado));
        }
    }
} 