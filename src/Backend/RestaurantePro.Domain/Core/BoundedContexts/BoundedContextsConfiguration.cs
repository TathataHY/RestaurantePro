
namespace RestaurantePro.Domain.Core.BoundedContexts
{
    /// <summary>
    /// Configuración y documentación de los elementos compartidos y límites entre contextos
    /// </summary>
    public static class BoundedContextsConfiguration
    {
        /// <summary>
        /// Elementos que son compartidos entre todos los bounded contexts (SharedKernel)
        /// </summary>
        public static readonly List<string> SharedKernelElements = new List<string>
        {
            // ValueObjects
            "Money",
            "Email",
            "PhoneNumber",
            "Address",

            // Interfaces
            "IRepository",
            "IUnitOfWork",
            "ISpecification",
            "IEmailService",
            "INotificationService",

            // Excepciones
            "DomainException",
            "ValidationException",
            "AuthorizationException",

            // Entidades base
            "BaseEntity",
            "AuditableEntity"
        };

        /// <summary>
        /// Describe cómo un bounded context traduce conceptos de otro bounded context
        /// </summary>
        public static readonly Dictionary<string, Dictionary<string, string>> TranslationMappings =
            new Dictionary<string, Dictionary<string, string>>
        {
            // Cómo Inventario traduce conceptos de Catálogo
            ["Inventario-Catalogo"] = new Dictionary<string, string>
            {
                ["Producto"] = "ItemInventario",
                ["Ingrediente"] = "ComponenteInventario"
            },

            // Cómo Clientes traduce conceptos de Operaciones
            ["Clientes-Operaciones"] = new Dictionary<string, string>
            {
                ["Comanda"] = "CompraCliente",
                ["ComandaDetalle"] = "ItemCompra"
            }
        };

        /// <summary>
        /// Define los límites explícitos entre contextos - qué operaciones cruzan fronteras
        /// </summary>
        public static readonly Dictionary<string, List<string>> ContextBoundaries =
            new Dictionary<string, List<string>>
        {
            // Integraciones permitidas entre Operaciones y Catálogo
            ["Operaciones-Catalogo"] = new List<string>
            {
                "ObtenerProductoPorId",
                "VerificarDisponibilidadProducto",
                "ObtenerPrecioProducto"
            },

            // Integraciones permitidas entre Operaciones e Inventario
            ["Operaciones-Inventario"] = new List<string>
            {
                "ReservarInventario",
                "ConfirmarConsumoInventario",
                "LiberarReservaInventario"
            },

            // Integraciones permitidas entre Operaciones y Pagos
            ["Operaciones-Pagos"] = new List<string>
            {
                "CrearPagoComanda",
                "ConsultarEstadoPago"
            }
        };
    }
}
