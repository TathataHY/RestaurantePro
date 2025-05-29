namespace RestaurantePro.Domain.Core.BoundedContexts
{
    /// <summary>
    /// Configuración y documentación de los elementos compartidos y límites entre contextos
    /// Actualizado para reflejar la estructura real del dominio implementado (Enero 2025)
    /// </summary>
    public static class BoundedContextsConfiguration
    {
        /// <summary>
        /// Elementos que son compartidos entre todos los bounded contexts (SharedKernel)
        /// </summary>
        public static readonly List<string> SharedKernelElements = new List<string>
        {
            // ValueObjects
            "PrecioProducto",
            "Email", 
            "PhoneNumber",
            "ClienteNombre",
            "ProveedorCategoria",

            // Interfaces Base
            "IRepository",
            "IUnitOfWork", 
            "ISpecification",
            "IDateTimeService",
            "INotificationService",
            "IEmailService",
            "ICacheService",

            // Patrones de Arquitectura
            "Result<T>",
            "INotificationManager",
            "NotificationManager",

            // Excepciones Base
            "DomainException",
            "ValidationException",
            "EntityNotFoundException",

            // Entidades Base
            "EntityBase",
            "IAggregateRoot",
            "DomainEvent",

            // Factories y Builders
            "IEntityFactory<T>",
            "EntityFactoryBase<T>",
            "BuilderBase<T>",

            // Servicios de Integración
            "IntegrationServiceBase"
        };

        /// <summary>
        /// Entidades principales por contexto delimitado (actualizado)
        /// </summary>
        public static readonly Dictionary<string, List<string>> ContextEntities = 
            new Dictionary<string, List<string>>
        {
            // CORE - Elementos fundamentales compartidos
            ["Core"] = new List<string>
            {
                "Usuario", "Rol", "Permiso",
                "Producto", "ProductoCategoria", "Receta", "ProductoIngrediente",
                "Notificacion"
            },

            // OPERACIONES - Gestión diaria del restaurante
            ["Operaciones"] = new List<string>
            {
                "Comanda", "ItemComanda", 
                "Reservacion", "Mesa",
                "PreparacionDiaria"  // ¡NUEVA FUNCIONALIDAD!
            },

            // COMERCIAL - Clientes, ventas y facturación
            ["Comercial"] = new List<string>
            {
                "Cliente", "TarjetaFidelizacion", "HistorialPuntos",
                "Factura", "DetalleFactura", 
                "Pago", "Promocion"
            },

            // INVENTARIO - Gestión de stock y compras
            ["Inventario"] = new List<string>
            {
                "Ingrediente", "MovimientoInventario",
                "OrdenCompra", "ItemOrdenCompra"
            },

            // PROVEEDORES - Gestión de proveedores
            ["Proveedores"] = new List<string>
            {
                "Proveedor", "ContactoProveedor"
            }
        };

        /// <summary>
        /// Servicios principales por contexto (actualizado)
        /// </summary>
        public static readonly Dictionary<string, List<string>> ContextServices =
            new Dictionary<string, List<string>>
        {
            ["Core"] = new List<string>
            {
                "ICoreServiceFacade",
                "IUsuarioService", "IUsuarioServiceCached",
                "IRecetaService", "IRecetaServiceCached", 
                "IProductoCategoriaService", "IProductoCategoriaServiceCached",
                "ICoreOperacionesIntegrationService"
            },

            ["Operaciones"] = new List<string>
            {
                "IOperacionesServiceFacade",
                "IServicioPreparaciones",  // ¡NUEVO SERVICIO!
                "IOperacionesInventarioIntegrationService"
            },

            ["Comercial"] = new List<string>
            {
                "IComercialServiceFacade",
                "IServicioFidelizacion",
                "IServicioFacturacion",
                "IServicioGestionFacturasVencidas",
                "IServicioIntegracionProveedores"
            },

            ["Inventario"] = new List<string>
            {
                "IInventarioServiceFacade",
                "IVerificadorStock",
                "IGeneradorOrdenesCompra",
                "IServicioNotificacionesInventario"
            },

            ["Proveedores"] = new List<string>
            {
                "IProveedoresServiceFacade"
            }
        };

        /// <summary>
        /// Describe cómo un bounded context traduce conceptos de otro bounded context
        /// </summary>
        public static readonly Dictionary<string, Dictionary<string, string>> TranslationMappings =
            new Dictionary<string, Dictionary<string, string>>
        {
            // Cómo Operaciones traduce conceptos de Core
            ["Operaciones-Core"] = new Dictionary<string, string>
            {
                ["Producto"] = "ItemComanda",
                ["Usuario"] = "MeseroComanda",
                ["Receta"] = "PreparacionRequerida"
            },

            // Cómo Inventario traduce conceptos de Core  
            ["Inventario-Core"] = new Dictionary<string, string>
            {
                ["Producto"] = "ProductoInventario",
                ["Ingrediente"] = "ItemStock",
                ["Receta"] = "RequisitoInventario"
            },

            // Cómo Comercial traduce conceptos de Operaciones
            ["Comercial-Operaciones"] = new Dictionary<string, string>
            {
                ["Comanda"] = "VentaCliente",
                ["ItemComanda"] = "DetalleVenta",
                ["Reservacion"] = "CompromiseCliente"
            },

            // Cómo Operaciones traduce conceptos de Inventario
            ["Operaciones-Inventario"] = new Dictionary<string, string>
            {
                ["Ingrediente"] = "ComponentePreparacion",
                ["Stock"] = "DisponibilidadPreparacion"
            }
        };

        /// <summary>
        /// Define los límites explícitos entre contextos - qué operaciones cruzan fronteras
        /// </summary>
        public static readonly Dictionary<string, List<string>> ContextBoundaries =
            new Dictionary<string, List<string>>
        {
            // Core → Operaciones
            ["Core-Operaciones"] = new List<string>
            {
                "ObtenerProductoPorId",
                "VerificarDisponibilidadProducto", 
                "ObtenerPrecioProducto",
                "ActualizarEstadisticasProducto",
                "VerificarRecetaProducto"
            },

            // Operaciones → Inventario  
            ["Operaciones-Inventario"] = new List<string>
            {
                "VerificarDisponibilidadIngredientes",
                "ReservarIngredientes",
                "ConfirmarConsumoIngredientes",
                "LiberarReservaIngredientes"
            },

            // Operaciones → Comercial
            ["Operaciones-Comercial"] = new List<string>
            {
                "NotificarComandaFinalizada",
                "ActualizarPuntosFidelizacion",
                "GenerarFacturaComanda"
            },

            // Comercial → Proveedores
            ["Comercial-Proveedores"] = new List<string>
            {
                "SincronizarInformacionProveedor",
                "ActualizarDatosFacturacion"
            },

            // Inventario → Proveedores
            ["Inventario-Proveedores"] = new List<string>
            {
                "CrearOrdenCompraProveedor",
                "NotificarRecepcionMercancia"
            }
        };

        /// <summary>
        /// Nuevas funcionalidades implementadas (Enero 2025)
        /// </summary>
        public static readonly Dictionary<string, List<string>> NewFeatures =
            new Dictionary<string, List<string>>
        {
            ["PreparacionesDiarias"] = new List<string>
            {
                "Entidad: PreparacionDiaria",
                "Servicio: IServicioPreparaciones",
                "Flujo: Híbrido preparaciones + al momento",
                "Integración: OperacionesServiceFacade"
            },

            ["PatronesArquitectonicos"] = new List<string>
            {
                "Builders: 8 implementados (ComandaBuilder, ReservacionBuilder, etc.)",
                "Factories: 2 implementados (ClienteFactory, IngredienteFactory)",
                "Result Pattern: Implementado en todo el dominio",
                "Notification Pattern: INotificationManager global"
            },

            ["ServiciosIntegracion"] = new List<string>
            {
                "CoreOperacionesIntegrationService",
                "OperacionesInventarioIntegrationService", 
                "ServicioIntegracionProveedores",
                "Anti-corruption layers implementados"
            }
        };

        /// <summary>
        /// Eventos de dominio principales por contexto
        /// </summary>
        public static readonly Dictionary<string, List<string>> ContextEvents =
            new Dictionary<string, List<string>>
        {
            ["Core"] = new List<string>
            {
                "UsuarioCreado", "ProductoCreado", "RecetaActualizada"
            },

            ["Operaciones"] = new List<string>
            {
                "ComandaCreada", "ComandaFinalizada", "ComandaModificada",
                "ReservacionCreada", "ReservacionConfirmada", "ReservacionCancelada",
                "PreparacionDiariaCreada", "PreparacionDiariaConsumida"  // ¡NUEVOS!
            },

            ["Comercial"] = new List<string>
            {
                "ClienteCreado", "ClienteDesactivado", 
                "PuntosAgregados", "FacturaGenerada",
                "PagoRealizado", "ProveedorActualizado"
            },

            ["Inventario"] = new List<string>
            {
                "IngredienteCreado", "StockActualizado", "StockBajoMinimo",
                "OrdenCompraCreada", "OrdenCompraRecibida",
                "MovimientoInventarioRegistrado"
            },

            ["Proveedores"] = new List<string>
            {
                "ProveedorRegistrado", "ContactoProveedorAgregado",
                "ProveedorActualizado"
            }
        };
    }
}
