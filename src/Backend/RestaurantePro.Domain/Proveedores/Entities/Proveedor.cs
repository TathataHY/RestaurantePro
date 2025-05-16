namespace RestaurantePro.Domain.Proveedores.Entities
{
    /// <summary>
    /// Entidad que representa un proveedor en el sistema
    /// </summary>
    public class Proveedor : EntityBase
    {
        /// <summary>
        /// Nombre del proveedor
        /// </summary>
        public string Nombre { get; private set; }
        
        /// <summary>
        /// Nombre del contacto principal
        /// </summary>
        public string NombreContacto { get; private set; }
        
        /// <summary>
        /// Email del proveedor
        /// </summary>
        public string Email { get; private set; }
        
        /// <summary>
        /// Teléfono del proveedor
        /// </summary>
        public string Telefono { get; private set; }
        
        /// <summary>
        /// Dirección del proveedor
        /// </summary>
        public string Direccion { get; private set; }
        
        /// <summary>
        /// Ciudad del proveedor
        /// </summary>
        public string Ciudad { get; private set; }
        
        /// <summary>
        /// Código postal del proveedor
        /// </summary>
        public string CodigoPostal { get; private set; }
        
        /// <summary>
        /// País del proveedor
        /// </summary>
        public string Pais { get; private set; }
        
        /// <summary>
        /// RFC del proveedor
        /// </summary>
        public string RFC { get; private set; }
        
        /// <summary>
        /// Información bancaria del proveedor
        /// </summary>
        public string InformacionBancaria { get; private set; }
        
        /// <summary>
        /// Días de crédito otorgados por el proveedor
        /// </summary>
        public int DiasCredito { get; private set; }
        
        /// <summary>
        /// Indica si el proveedor está activo
        /// </summary>
        public bool Activo { get; private set; }
        
        /// <summary>
        /// Fecha de registro del proveedor
        /// </summary>
        public DateTime FechaRegistro { get; private set; }
        
        /// <summary>
        /// Fecha de la última orden realizada a este proveedor
        /// </summary>
        public DateTime? UltimaOrden { get; private set; }
        
        /// <summary>
        /// Observaciones sobre el proveedor
        /// </summary>
        public string Observaciones { get; private set; }
        
        private readonly List<DateTime> _historialOrdenes = new();
        
        /// <summary>
        /// Historial de fechas de órdenes realizadas a este proveedor
        /// </summary>
        public IReadOnlyList<DateTime> HistorialOrdenes => _historialOrdenes.AsReadOnly();

        /// <summary>
        /// Constructor protegido para EF Core
        /// </summary>
        protected Proveedor() { }
        
        /// <summary>
        /// Constructor para crear un nuevo proveedor
        /// </summary>
        private Proveedor(
            string nombre,
            string nombreContacto,
            string email,
            string telefono,
            string direccion,
            string ciudad,
            string codigoPostal,
            string pais,
            string rfc,
            string informacionBancaria,
            int diasCredito)
        {
            Nombre = nombre;
            NombreContacto = nombreContacto;
            Email = email;
            Telefono = telefono;
            Direccion = direccion;
            Ciudad = ciudad;
            CodigoPostal = codigoPostal;
            Pais = pais;
            RFC = rfc;
            InformacionBancaria = informacionBancaria;
            DiasCredito = diasCredito;
            Activo = true;
            FechaRegistro = DateTime.Now;
            
            AddDomainEvent(new ProveedorRegistradoEvent(Id, nombre));
        }
        
        /// <summary>
        /// Factory method para crear un nuevo proveedor
        /// </summary>
        public static Proveedor Crear(
            string nombre,
            string nombreContacto,
            string email,
            string telefono,
            string direccion,
            string ciudad,
            string codigoPostal,
            string pais,
            string rfc,
            string informacionBancaria,
            int diasCredito)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del proveedor es obligatorio", nameof(nombre));
                
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email del proveedor es obligatorio", nameof(email));
                
            if (diasCredito < 0)
                throw new ArgumentException("Los días de crédito no pueden ser negativos", nameof(diasCredito));
                
            return new Proveedor(
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
        }
        
        /// <summary>
        /// Actualiza la información del proveedor
        /// </summary>
        public void ActualizarInformacion(
            string nombre,
            string nombreContacto,
            string email,
            string telefono,
            string direccion,
            string ciudad,
            string codigoPostal,
            string pais,
            string rfc,
            string informacionBancaria,
            int diasCredito)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del proveedor es obligatorio", nameof(nombre));
                
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email del proveedor es obligatorio", nameof(email));
                
            if (diasCredito < 0)
                throw new ArgumentException("Los días de crédito no pueden ser negativos", nameof(diasCredito));
                
            Nombre = nombre;
            NombreContacto = nombreContacto;
            Email = email;
            Telefono = telefono;
            Direccion = direccion;
            Ciudad = ciudad;
            CodigoPostal = codigoPostal;
            Pais = pais;
            RFC = rfc;
            InformacionBancaria = informacionBancaria;
            DiasCredito = diasCredito;
            
            MarkAsModified();
            AddDomainEvent(new ProveedorActualizadoEvent(Id, nombre));
        }
        
        /// <summary>
        /// Agrega observaciones al proveedor
        /// </summary>
        public void AgregarObservaciones(string observaciones)
        {
            if (string.IsNullOrWhiteSpace(observaciones))
                throw new ArgumentException("Las observaciones no pueden estar vacías", nameof(observaciones));
                
            Observaciones = observaciones;
            MarkAsModified();
        }
        
        /// <summary>
        /// Activa el proveedor
        /// </summary>
        public void Activar()
        {
            if (Activo)
                return;
                
            Activo = true;
            MarkAsModified();
            AddDomainEvent(new ProveedorActivadoEvent(Id, Nombre));
        }
        
        /// <summary>
        /// Desactiva el proveedor
        /// </summary>
        public void Desactivar()
        {
            if (!Activo)
                return;
                
            Activo = false;
            MarkAsModified();
            AddDomainEvent(new ProveedorDesactivadoEvent(Id, Nombre));
        }
        
        /// <summary>
        /// Registra una nueva orden con este proveedor
        /// </summary>
        public void RegistrarOrden(DateTime fechaOrden)
        {
            if (fechaOrden > DateTime.Now)
                throw new ArgumentException("La fecha de la orden no puede ser futura", nameof(fechaOrden));
                
            _historialOrdenes.Add(fechaOrden);
            UltimaOrden = fechaOrden;
            MarkAsModified();
        }
    }
} 