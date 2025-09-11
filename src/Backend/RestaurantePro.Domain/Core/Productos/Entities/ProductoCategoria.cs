namespace RestaurantePro.Domain.Core.Productos.Entities
{
    /// <summary>
    /// Agregado que representa una categoría de productos en el menú.
    /// 
    /// Invariantes:
    /// - El nombre de la categoría no puede estar vacío
    /// - El orden debe ser mayor o igual a cero
    /// 
    /// Ciclo de vida:
    /// - Creación/Activo → [Actualización → Activo]
    ///                   ↘ [Desactivación → Inactivo → Activación → Activo]
    /// 
    /// Reglas de negocio:
    /// - Una categoría desactivada puede volver a activarse
    /// - Los datos de la categoría (nombre, descripción, orden) pueden actualizarse en cualquier momento
    /// - Cada cambio de estado genera eventos de dominio
    /// - Una categoría desactivada no se muestra en el menú
    /// </summary>
    public class ProductoCategoria : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Nombre de la categoría
        /// </summary>
        public string Nombre { get; private set; }

        /// <summary>
        /// Descripción de la categoría
        /// </summary>
        public string Descripcion { get; private set; }

        /// <summary>
        /// Orden de visualización de la categoría
        /// </summary>
        public int Orden { get; private set; }

        /// <summary>
        /// Color de la categoría (formato hexadecimal)
        /// </summary>
        public string Color { get; private set; }

        /// <summary>
        /// Icono de la categoría (emoji o código de icono)
        /// </summary>
        public string Icono { get; private set; }

        /// <summary>
        /// Indica si la categoría está activa
        /// </summary>
        public bool EstaActivo { get; private set; }

        protected ProductoCategoria() { }

        private ProductoCategoria(string nombre, string descripcion, int orden, string color, string icono)
        {
            Id = Guid.NewGuid();
            Nombre = nombre;
            Descripcion = descripcion;
            Orden = orden;
            Color = color;
            Icono = icono;
            EstaActivo = true;

            ValidarInvariantes();
            AddDomainEvent(new Events.ProductoCategoria.ProductoCategoriaCreada(Id, Nombre));
        }

        /// <summary>
        /// Crea una nueva instancia de una categoría de productos
        /// </summary>
        public static ProductoCategoria Crear(string nombre, string descripcion, int orden, string color, string icono)
        {
            return new ProductoCategoria(nombre, descripcion, orden, color, icono);
        }

        /// <summary>
        /// Actualiza los datos de la categoría
        /// </summary>
        public void Actualizar(string nombre, string descripcion, int orden, string color, string icono)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Orden = orden;
            Color = color;
            Icono = icono;
            MarkAsModified();

            ValidarInvariantes();
            AddDomainEvent(new Events.ProductoCategoria.ProductoCategoriaActualizada(Id, Nombre, Descripcion, Orden));
        }

        /// <summary>
        /// Desactiva la categoría
        /// </summary>
        public void Desactivar()
        {
            if (!EstaActivo) return;

            EstaActivo = false;
            MarkAsModified();

            ValidarInvariantes();
            AddDomainEvent(new Events.ProductoCategoria.ProductoCategoriaDesactivada(Id));
        }

        /// <summary>
        /// Activa la categoría
        /// </summary>
        public void Activar()
        {
            if (EstaActivo) return;

            EstaActivo = true;
            MarkAsModified();

            ValidarInvariantes();
            AddDomainEvent(new Events.ProductoCategoria.ProductoCategoriaActivada(Id));
        }

        /// <summary>
        /// Valida las invariantes del agregado ProductoCategoria
        /// </summary>
        private void ValidarInvariantes()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                throw new InvalidOperationException("El nombre de la categoría no puede estar vacío");
            }

            if (Orden < 0)
            {
                throw new InvalidOperationException("El orden debe ser mayor o igual a cero");
            }
        }
    }
} 