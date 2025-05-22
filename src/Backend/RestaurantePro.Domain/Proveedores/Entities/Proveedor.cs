namespace RestaurantePro.Domain.Proveedores.Entities
{
    using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
    
    /// <summary>
    /// Entidad que representa un proveedor en el sistema
    /// </summary>
    public class Proveedor : EntityBase, IAggregateRoot
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
        public Email Email { get; private set; }
        
        /// <summary>
        /// Teléfono del proveedor
        /// </summary>
        public PhoneNumber Telefono { get; private set; }
        
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
        
        private readonly List<ContactoProveedor> _contactos = new();
        
        /// <summary>
        /// Lista de contactos del proveedor
        /// </summary>
        public IReadOnlyCollection<ContactoProveedor> Contactos => _contactos.AsReadOnly();

        private readonly List<ValueObjects.ProveedorCategoria> _categorias = new();
        
        /// <summary>
        /// Lista de categorías asignadas al proveedor
        /// </summary>
        public IReadOnlyCollection<ValueObjects.ProveedorCategoria> Categorias => _categorias.AsReadOnly();

        /// <summary>
        /// Indica si el proveedor está activo
        /// </summary>
        public bool EstaActivo => Activo;

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
            Email email,
            PhoneNumber telefono,
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
            
            AddDomainEvent(new ProveedorRegistrado(Id, nombre));
            
            // Validar invariantes al crear el proveedor
            ValidarInvariantes();
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
            
            // Las validaciones de email y teléfono se realizan en los ValueObjects
            var emailVO = Email.Create(email);
            var telefonoVO = PhoneNumber.Create(telefono);
                
            if (diasCredito < 0)
                throw new ArgumentException("Los días de crédito no pueden ser negativos", nameof(diasCredito));
                
            // Validar formato de RFC
            if (!string.IsNullOrWhiteSpace(rfc) && rfc.Length < 10)
                throw new ArgumentException("El formato del RFC no es válido", nameof(rfc));
                
            return new Proveedor(
                nombre, 
                nombreContacto, 
                emailVO, 
                telefonoVO, 
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
            
            // Las validaciones de email y teléfono se realizan en los ValueObjects
            var emailVO = Email.Create(email);
            var telefonoVO = PhoneNumber.Create(telefono);
                
            if (diasCredito < 0)
                throw new ArgumentException("Los días de crédito no pueden ser negativos", nameof(diasCredito));
                
            // Validar formato de RFC
            if (!string.IsNullOrWhiteSpace(rfc) && rfc.Length < 10)
                throw new ArgumentException("El formato del RFC no es válido", nameof(rfc));
                
            Nombre = nombre;
            NombreContacto = nombreContacto;
            Email = emailVO;
            Telefono = telefonoVO;
            Direccion = direccion;
            Ciudad = ciudad;
            CodigoPostal = codigoPostal;
            Pais = pais;
            RFC = rfc;
            InformacionBancaria = informacionBancaria;
            DiasCredito = diasCredito;
            
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new ProveedorActualizado(Id, nombre, Email, Telefono, direccion));
        }
        
        /// <summary>
        /// Actualiza la dirección del proveedor
        /// </summary>
        /// <param name="direccion">Nueva dirección</param>
        public void ActualizarDireccion(string direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion))
                throw new ArgumentException("La dirección no puede estar vacía", nameof(direccion));

            Direccion = direccion;
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new ProveedorActualizado(Id, Nombre, Email, Telefono, direccion));
        }
        
        /// <summary>
        /// Actualiza el teléfono del proveedor
        /// </summary>
        /// <param name="telefono">Nuevo teléfono</param>
        public void ActualizarTelefono(string telefono)
        {
            var telefonoVO = PhoneNumber.Create(telefono);
                
            Telefono = telefonoVO;
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new ProveedorActualizado(Id, Nombre, Email, Telefono, Direccion));
        }
        
        /// <summary>
        /// Actualiza el email del proveedor
        /// </summary>
        /// <param name="email">Nuevo email</param>
        public void ActualizarEmail(string email)
        {
            var emailVO = Email.Create(email);
                
            Email = emailVO;
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new ProveedorActualizado(Id, Nombre, Email, Telefono, Direccion));
        }
        
        /// <summary>
        /// Actualiza el sitio web del proveedor
        /// </summary>
        /// <param name="sitioWeb">Nuevo sitio web</param>
        public void ActualizarSitioWeb(string sitioWeb)
        {
            // El sitio web puede ser nulo o vacío
            
            // Si tenemos un sitio web específico, es propiedad de NombreContacto en esta implementación
            if (!string.IsNullOrWhiteSpace(sitioWeb))
                NombreContacto = sitioWeb;
                
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new ProveedorActualizado(Id, Nombre, Email, Telefono, Direccion));
        }
        
        /// <summary>
        /// Actualiza las notas del proveedor
        /// </summary>
        /// <param name="notas">Nuevas notas</param>
        public void ActualizarNotas(string notas)
        {
            // Las notas pueden estar vacías
            AgregarObservaciones(notas);
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
            ValidarInvariantes();
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
            ValidarInvariantes();
            
            AddDomainEvent(new ProveedorActivado(Id, Nombre));
        }
        
        /// <summary>
        /// Desactiva el proveedor
        /// </summary>
        /// <param name="motivo">Motivo de la desactivación</param>
        public void Desactivar(string motivo)
        {
            if (!Activo)
                return;
                
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo de desactivación no puede estar vacío", nameof(motivo));
                
            Activo = false;
            Observaciones = motivo;
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new ProveedorDesactivado(Id, Nombre));
        }
        
        /// <summary>
        /// Agrega un nuevo contacto al proveedor
        /// </summary>
        /// <param name="nombre">Nombre del contacto</param>
        /// <param name="cargo">Cargo del contacto</param>
        /// <param name="telefono">Teléfono del contacto</param>
        /// <param name="email">Email del contacto</param>
        /// <param name="esPrincipal">Indica si es el contacto principal</param>
        /// <param name="notas">Notas sobre el contacto</param>
        /// <returns>El contacto agregado</returns>
        public ContactoProveedor AgregarContacto(
            string nombre, 
            string cargo, 
            string telefono, 
            string email, 
            bool esPrincipal = false, 
            string? notas = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del contacto es obligatorio", nameof(nombre));
            
            // Las validaciones de email y teléfono se realizan en los ValueObjects
            var contacto = ContactoProveedor.Crear(Id, nombre, cargo, telefono, email);
            _contactos.Add(contacto);
            
            // Si es contacto principal, actualizar el nombre de contacto principal
            if (esPrincipal)
            {
                NombreContacto = nombre;
            }
            
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new ContactoProveedorAgregado(Id, contacto.Id, nombre, cargo, contacto.Email, contacto.Telefono));
            
            return contacto;
        }
        
        /// <summary>
        /// Elimina un contacto del proveedor
        /// </summary>
        /// <param name="contactoId">ID del contacto a eliminar</param>
        public void EliminarContacto(Guid contactoId)
        {
            var contacto = _contactos.FirstOrDefault(c => c.Id == contactoId);
            
            if (contacto == null)
                throw new ArgumentException($"No existe un contacto con el ID {contactoId} para este proveedor", nameof(contactoId));
                
            _contactos.Remove(contacto);
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new ContactoProveedorEliminado(Id, contactoId, contacto.Nombre));
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
            ValidarInvariantes();
        }
        
        /// <summary>
        /// Valida todas las invariantes del agregado Proveedor.
        /// Se llama después de cada operación que modifica el estado para asegurar la consistencia.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si alguna invariante se viola</exception>
        private void ValidarInvariantes()
        {
            // Validar que los datos básicos obligatorios estén presentes
            if (string.IsNullOrWhiteSpace(Nombre))
                throw new InvalidOperationException("El nombre del proveedor no puede estar vacío");
                
            if (Email == null)
                throw new InvalidOperationException("El email del proveedor no puede ser nulo");
                
            if (Telefono == null)
                throw new InvalidOperationException("El teléfono del proveedor no puede ser nulo");
                
            // La validación del formato de email ya se realiza en el ValueObject Email
            
            // Validar que los días de crédito no sean negativos
            if (DiasCredito < 0)
                throw new InvalidOperationException($"Los días de crédito no pueden ser negativos. Valor actual: {DiasCredito}");
                
            // Validar consistencia del historial de órdenes y la última orden
            if (UltimaOrden.HasValue && _historialOrdenes.Count > 0)
            {
                var ultimaFechaHistorial = _historialOrdenes.Max();
                if (UltimaOrden.Value != ultimaFechaHistorial)
                    throw new InvalidOperationException($"Inconsistencia en las fechas de órdenes. Última orden: {UltimaOrden}, Última en historial: {ultimaFechaHistorial}");
            }
            
            // Validar que no haya fechas de órdenes futuras
            if (_historialOrdenes.Any(fecha => fecha > DateTime.Now))
                throw new InvalidOperationException("No puede haber fechas de órdenes en el futuro");
                
            // Validar que los contactos pertenezcan a este proveedor
            foreach (var contacto in _contactos)
            {
                if (contacto.ProveedorId != Id)
                    throw new InvalidOperationException($"El contacto {contacto.Id} no pertenece a este proveedor");
            }
        }

        /// <summary>
        /// Agrega una categoría al proveedor
        /// </summary>
        /// <param name="categoria">Categoría a agregar</param>
        /// <param name="porcentajeDescuento">Porcentaje de descuento para esta categoría</param>
        /// <param name="esProveedorPrincipal">Si es proveedor principal para esta categoría</param>
        /// <exception cref="InvalidOperationException">Si la categoría ya existe para este proveedor</exception>
        public void AgregarCategoria(Enums.CategoriaProveedor categoria, decimal porcentajeDescuento = 0, bool esProveedorPrincipal = false)
        {
            // Verificar que no exista ya la categoría
            if (_categorias.Any(c => c.Categoria == categoria))
                throw new InvalidOperationException($"El proveedor ya tiene asignada la categoría {categoria}");
                
            // Crear y agregar la categoría
            var nuevaCategoria = ValueObjects.ProveedorCategoria.Crear(categoria, porcentajeDescuento, esProveedorPrincipal);
            _categorias.Add(nuevaCategoria);
            
            // Emitir evento de dominio
            AddDomainEvent(new Events.ProveedorCategoriaAgregada(
                Id, 
                Nombre, 
                categoria, 
                porcentajeDescuento, 
                esProveedorPrincipal));
                
            MarkAsModified();
        }
        
        /// <summary>
        /// Elimina una categoría del proveedor
        /// </summary>
        /// <param name="categoria">Categoría a eliminar</param>
        /// <exception cref="InvalidOperationException">Si la categoría no existe para este proveedor</exception>
        public void EliminarCategoria(Enums.CategoriaProveedor categoria)
        {
            // Buscar la categoría
            var categoriaExistente = _categorias.FirstOrDefault(c => c.Categoria == categoria);
            if (categoriaExistente == null)
                throw new InvalidOperationException($"El proveedor no tiene asignada la categoría {categoria}");
                
            // Guardar el estado de proveedor principal antes de eliminar
            bool eraProveedorPrincipal = categoriaExistente.EsProveedorPrincipal;
            
            // Eliminar la categoría
            _categorias.RemoveAll(c => c.Categoria == categoria);
            
            // Emitir evento de dominio
            AddDomainEvent(new Events.ProveedorCategoriaEliminada(
                Id, 
                Nombre, 
                categoria, 
                eraProveedorPrincipal));
                
            MarkAsModified();
        }
        
        /// <summary>
        /// Actualiza el porcentaje de descuento para una categoría
        /// </summary>
        /// <param name="categoria">Categoría a actualizar</param>
        /// <param name="porcentajeDescuento">Nuevo porcentaje de descuento</param>
        /// <exception cref="InvalidOperationException">Si la categoría no existe para este proveedor</exception>
        public void ActualizarPorcentajeDescuento(Enums.CategoriaProveedor categoria, decimal porcentajeDescuento)
        {
            // Validar porcentaje
            if (porcentajeDescuento < 0 || porcentajeDescuento > 100)
                throw new ArgumentException("El porcentaje de descuento debe estar entre 0 y 100", nameof(porcentajeDescuento));
                
            // Buscar la categoría
            var index = _categorias.FindIndex(c => c.Categoria == categoria);
            if (index < 0)
                throw new InvalidOperationException($"El proveedor no tiene asignada la categoría {categoria}");
                
            var categoriaExistente = _categorias[index];
            
            // Si el porcentaje es el mismo, no hacer nada
            if (categoriaExistente.PorcentajeDescuento == porcentajeDescuento)
                return;
                
            // Guardar el porcentaje anterior
            var porcentajeAnterior = categoriaExistente.PorcentajeDescuento;
            
            // Actualizar la categoría (creando una nueva instancia ya que es un ValueObject)
            _categorias[index] = categoriaExistente.ConPorcentajeDescuento(porcentajeDescuento);
            
            // Emitir evento de dominio
            AddDomainEvent(new Events.ProveedorCategoriaActualizada(
                Id,
                Nombre,
                categoria,
                porcentajeAnterior,
                porcentajeDescuento,
                categoriaExistente.EsProveedorPrincipal,
                categoriaExistente.EsProveedorPrincipal));
                
            MarkAsModified();
        }
        
        /// <summary>
        /// Establece o quita el estado de proveedor principal para una categoría
        /// </summary>
        /// <param name="categoria">Categoría a actualizar</param>
        /// <param name="esProveedorPrincipal">Si debe ser proveedor principal</param>
        /// <exception cref="InvalidOperationException">Si la categoría no existe para este proveedor</exception>
        public void EstablecerProveedorPrincipal(Enums.CategoriaProveedor categoria, bool esProveedorPrincipal)
        {
            // Buscar la categoría
            var index = _categorias.FindIndex(c => c.Categoria == categoria);
            if (index < 0)
                throw new InvalidOperationException($"El proveedor no tiene asignada la categoría {categoria}");
                
            var categoriaExistente = _categorias[index];
            
            // Si el estado es el mismo, no hacer nada
            if (categoriaExistente.EsProveedorPrincipal == esProveedorPrincipal)
                return;
                
            // Actualizar la categoría (creando una nueva instancia ya que es un ValueObject)
            _categorias[index] = categoriaExistente.ConEstadoPrincipal(esProveedorPrincipal);
            
            // Emitir evento de dominio
            AddDomainEvent(new Events.ProveedorCategoriaActualizada(
                Id,
                Nombre,
                categoria,
                categoriaExistente.PorcentajeDescuento,
                categoriaExistente.PorcentajeDescuento,
                !esProveedorPrincipal,
                esProveedorPrincipal));
                
            MarkAsModified();
        }
        
        /// <summary>
        /// Verifica si el proveedor tiene una categoría específica
        /// </summary>
        /// <param name="categoria">Categoría a verificar</param>
        /// <returns>True si el proveedor tiene la categoría, False en caso contrario</returns>
        public bool TieneCategoria(Enums.CategoriaProveedor categoria)
        {
            return _categorias.Any(c => c.Categoria == categoria);
        }
        
        /// <summary>
        /// Verifica si el proveedor es el principal para una categoría específica
        /// </summary>
        /// <param name="categoria">Categoría a verificar</param>
        /// <returns>True si el proveedor es principal para esta categoría, False en caso contrario</returns>
        public bool EsProveedorPrincipalPara(Enums.CategoriaProveedor categoria)
        {
            var cat = _categorias.FirstOrDefault(c => c.Categoria == categoria);
            return cat != null && cat.EsProveedorPrincipal;
        }
        
        /// <summary>
        /// Obtiene el porcentaje de descuento para una categoría específica
        /// </summary>
        /// <param name="categoria">Categoría a consultar</param>
        /// <returns>Porcentaje de descuento o 0 si el proveedor no tiene la categoría</returns>
        public decimal ObtenerPorcentajeDescuento(Enums.CategoriaProveedor categoria)
        {
            var cat = _categorias.FirstOrDefault(c => c.Categoria == categoria);
            return cat?.PorcentajeDescuento ?? 0;
        }
    }
} 