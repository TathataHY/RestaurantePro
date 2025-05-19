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
        }
    }
}
