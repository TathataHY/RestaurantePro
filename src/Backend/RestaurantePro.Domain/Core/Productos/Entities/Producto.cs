namespace RestaurantePro.Domain.Core.Productos.Entities
{
    /// <summary>
    /// Agregado que representa un producto del restaurante.
    /// 
    /// Invariantes:
    /// - El nombre del producto no puede estar vacío
    /// - El precio debe ser mayor que cero
    /// - Debe pertenecer a una categoría válida
    /// 
    /// Ciclo de vida:
    /// - Creación/Activo → [Actualización → Activo]
    ///                   ↘ [Desactivación → Inactivo → Activación → Activo]
    /// 
    /// Reglas de negocio:
    /// - Un producto desactivado puede volver a activarse
    /// - Los datos del producto (nombre, descripción, precio) pueden actualizarse en cualquier momento
    /// - Cada cambio de estado genera eventos de dominio
    /// - Un producto desactivado no se muestra a los clientes ni puede ser ordenado
    /// </summary>
    public class Producto : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string? Nombre { get; private set; }

        /// <summary>
        /// Descripción del producto
        /// </summary>
        public string? Descripcion { get; private set; }

        /// <summary>
        /// Precio del producto
        /// </summary>
        public PrecioProducto? Precio { get; private set; }

        /// <summary>
        /// Identificador de la categoría a la que pertenece el producto
        /// </summary>
        public Guid CategoriaId { get; private set; }

        /// <summary>
        /// Nombre de la categoría a la que pertenece el producto
        /// </summary>
        public string? CategoriaNombre { get; private set; }

        /// <summary>
        /// Indica si el producto está activo
        /// </summary>
        public bool EstaActivo { get; private set; }
        
        /// <summary>
        /// Nivel de popularidad del producto en escala de 0-10
        /// </summary>
        public int Popularidad { get; private set; }

        /// <summary>
        /// Receta asociada al producto
        /// </summary>
        public ICollection<Receta> Recetas { get; private set; } = new List<Receta>();

        /// <summary>
        /// Fecha de expiración del producto
        /// </summary>
        public DateTime? FechaExpiracion { get; private set; }

        protected Producto() { }

        private Producto(string nombre, string descripcion, PrecioProducto precio, Guid categoriaId, string? categoriaNombre = null)
        {
            Id = Guid.NewGuid();
            Nombre = nombre;
            Descripcion = descripcion;
            Precio = precio;
            CategoriaId = categoriaId;
            CategoriaNombre = categoriaNombre ?? "Sin categoría";
            EstaActivo = true;
            Popularidad = 0; // Nuevo producto inicia con popularidad 0

            ValidarInvariantes();
            AddDomainEvent(new ProductoCreado(Id, Nombre!, Precio.Valor));
        }

        /// <summary>
        /// Crea una nueva instancia de un producto
        /// </summary>
        public static Producto Crear(string nombre, string descripcion, PrecioProducto precio, Guid categoriaId, string? categoriaNombre = null)
        {
            return new Producto(nombre, descripcion, precio, categoriaId, categoriaNombre);
        }

        /// <summary>
        /// Actualiza los datos del producto
        /// </summary>
        public void Actualizar(string nombre, string descripcion, PrecioProducto precio)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Precio = precio;
            MarkAsModified();

            ValidarInvariantes();
            AddDomainEvent(new ProductoActualizado(Id, Nombre!, Descripcion!, Precio!.Valor));
        }

        /// <summary>
        /// Actualiza la categoría del producto
        /// </summary>
        public void ActualizarCategoria(Guid categoriaId, string categoriaNombre)
        {
            if (CategoriaId == categoriaId && CategoriaNombre == categoriaNombre) return;

            CategoriaId = categoriaId;
            CategoriaNombre = categoriaNombre;
            MarkAsModified();

            ValidarInvariantes();
            AddDomainEvent(new ProductoCambioCategoria(Id, CategoriaId, CategoriaNombre!));
        }
        
        /// <summary>
        /// Actualiza el nivel de popularidad del producto
        /// </summary>
        /// <param name="nuevaPopularidad">Nuevo valor de popularidad (0-10)</param>
        public void ActualizarPopularidad(int nuevaPopularidad)
        {
            if (nuevaPopularidad < 0 || nuevaPopularidad > 10)
                throw new ArgumentOutOfRangeException(nameof(nuevaPopularidad), "La popularidad debe estar entre 0 y 10");
            
            if (Popularidad == nuevaPopularidad) return;
            
            int popularidadAnterior = Popularidad;
            Popularidad = nuevaPopularidad;
            MarkAsModified();
            
            AddDomainEvent(new PopularidadProductoActualizada(Id, popularidadAnterior, Popularidad));
        }
        
        /// <summary>
        /// Incrementa el nivel de popularidad del producto
        /// </summary>
        /// <param name="incremento">Cantidad a incrementar</param>
        public void IncrementarPopularidad(int incremento = 1)
        {
            int nuevaPopularidad = Math.Min(Popularidad + incremento, 10);
            ActualizarPopularidad(nuevaPopularidad);
        }
        
        /// <summary>
        /// Decrementa el nivel de popularidad del producto
        /// </summary>
        /// <param name="decremento">Cantidad a decrementar</param>
        public void DecrementarPopularidad(int decremento = 1)
        {
            int nuevaPopularidad = Math.Max(Popularidad - decremento, 0);
            ActualizarPopularidad(nuevaPopularidad);
        }

        /// <summary>
        /// Actualiza la fecha de expiración del producto
        /// </summary>
        public void ActualizarFechaExpiracion(DateTime? fechaExpiracion)
        {
            FechaExpiracion = fechaExpiracion;
            MarkAsModified();
        }

        /// <summary>
        /// Desactiva el producto
        /// </summary>
        public void Desactivar()
        {
            if (!EstaActivo) return;

            EstaActivo = false;
            MarkAsModified();

            ValidarInvariantes();
            AddDomainEvent(new ProductoDesactivado(Id));
        }

        /// <summary>
        /// Activa el producto
        /// </summary>
        public void Activar()
        {
            if (EstaActivo) return;

            EstaActivo = true;
            MarkAsModified();

            ValidarInvariantes();
            AddDomainEvent(new ProductoActivado(Id));
        }

        /// <summary>
        /// Marca la entidad como eliminada lógicamente, y la desactiva.
        /// </summary>
        public override void MarkAsDeleted()
        {
            base.MarkAsDeleted();
            Desactivar();
        }

        /// <summary>
        /// Valida las invariantes del agregado Producto
        /// </summary>
        private void ValidarInvariantes()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                throw new InvalidOperationException("El nombre del producto no puede estar vacío");
            }

            if (Precio == null || Precio.Valor <= 0)
            {
                throw new InvalidOperationException("El precio del producto debe ser mayor que cero");
            }

            if (CategoriaId == Guid.Empty)
            {
                throw new InvalidOperationException("El producto debe pertenecer a una categoría válida");
            }

            if (string.IsNullOrWhiteSpace(CategoriaNombre))
            {
                throw new InvalidOperationException("El nombre de la categoría no puede estar vacío");
            }
            
            if (Popularidad < 0 || Popularidad > 10)
            {
                throw new InvalidOperationException("La popularidad debe estar entre 0 y 10");
            }
        }
    }
}
