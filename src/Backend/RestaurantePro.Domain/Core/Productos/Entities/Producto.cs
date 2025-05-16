
namespace RestaurantePro.Domain.Core.Productos.Entities
{
    /// <summary>
    /// Entidad que representa un producto del restaurante
    /// </summary>
    public class Producto : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string Nombre { get; private set; }

        /// <summary>
        /// Descripción del producto
        /// </summary>
        public string Descripcion { get; private set; }

        /// <summary>
        /// Precio del producto
        /// </summary>
        public PrecioProducto Precio { get; private set; }

        /// <summary>
        /// Identificador de la categoría a la que pertenece el producto
        /// </summary>
        public Guid CategoriaId { get; private set; }

        /// <summary>
        /// Indica si el producto está activo
        /// </summary>
        public bool EstaActivo { get; private set; }

        protected Producto() { }

        private Producto(string nombre, string descripcion, PrecioProducto precio, Guid categoriaId)
        {
            Id = Guid.NewGuid();
            Nombre = nombre;
            Descripcion = descripcion;
            Precio = precio;
            CategoriaId = categoriaId;
            EstaActivo = true;

            AddDomainEvent(new ProductoCreado(Id, Nombre, Precio.Valor));
        }

        /// <summary>
        /// Crea una nueva instancia de un producto
        /// </summary>
        public static Producto Crear(string nombre, string descripcion, PrecioProducto precio, Guid categoriaId)
        {
            return new Producto(nombre, descripcion, precio, categoriaId);
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

            AddDomainEvent(new ProductoActualizado(Id, Nombre, Descripcion, Precio.Valor));
        }

        /// <summary>
        /// Desactiva el producto
        /// </summary>
        public void Desactivar()
        {
            if (!EstaActivo) return;

            EstaActivo = false;
            MarkAsModified();

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

            AddDomainEvent(new ProductoActivado(Id));
        }
    }
}
