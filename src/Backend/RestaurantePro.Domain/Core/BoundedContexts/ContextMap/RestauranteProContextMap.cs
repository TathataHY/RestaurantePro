namespace RestaurantePro.Domain.Core.BoundedContexts.ContextMap
{
    /// <summary>
    /// Enumera los tipos de relación entre bounded contexts
    /// </summary>
    public enum ContextRelationship
    {
        /// <summary>
        /// Un equipo upstream impone su modelo al equipo downstream
        /// </summary>
        ConformistUpstream,

        /// <summary>
        /// Un equipo downstream se adapta al modelo del equipo upstream
        /// </summary>
        ConformistDownstream,

        /// <summary>
        /// Relación de cooperación donde ambos equipos definen un modelo conjunto
        /// </summary>
        Partnership,

        /// <summary>
        /// Equipo downstream crea una capa anti-corrupción para traducir el modelo upstream
        /// </summary>
        AnticorruptionLayer,

        /// <summary>
        /// Dos contextos comparten un subconjunto del modelo de dominio
        /// </summary>
        SharedKernel,

        /// <summary>
        /// Un equipo proporciona servicio a otro equipo
        /// </summary>
        CustomerSupplier,

        /// <summary>
        /// Contextos separados sin comunicación directa
        /// </summary>
        SeparateWays
    }

    /// <summary>
    /// Define un Bounded Context en el sistema
    /// </summary>
    public class BoundedContext
    {
        public string Name { get; }
        public string Description { get; }
        public string ResponsibleTeam { get; }
        public List<string> MainEntities { get; }

        public BoundedContext(string name, string description, string responsibleTeam, List<string> mainEntities)
        {
            Name = name;
            Description = description;
            ResponsibleTeam = responsibleTeam;
            MainEntities = mainEntities;
        }
    }

    /// <summary>
    /// Define una relación entre dos Bounded Contexts
    /// </summary>
    public class ContextRelation
    {
        public BoundedContext Upstream { get; }
        public BoundedContext Downstream { get; }
        public ContextRelationship Relationship { get; }
        public string Description { get; }

        public ContextRelation(
            BoundedContext upstream,
            BoundedContext downstream,
            ContextRelationship relationship,
            string description)
        {
            Upstream = upstream;
            Downstream = downstream;
            Relationship = relationship;
            Description = description;
        }
    }

    /// <summary>
    /// Mapa de contextos completo del sistema - ACTUALIZADO (Enero 2025)
    /// Refleja la implementación real del dominio RestaurantePro
    /// </summary>
    public static class RestauranteProContextMap
    {
        // CONTEXTOS REALMENTE IMPLEMENTADOS 

        public static readonly BoundedContext CoreContext = new BoundedContext(
            "Core",
            "Elementos fundamentales compartidos: usuarios, productos, recetas y notificaciones",
            "Equipo Arquitectura",
            new List<string> { "Usuario", "Rol", "Permiso", "Producto", "ProductoCategoria", "Receta", "ProductoIngrediente", "Notificacion" }
        );

        public static readonly BoundedContext OperacionesContext = new BoundedContext(
            "Operaciones",
            "Gestión operativa diaria: comandas, reservaciones, mesas y preparaciones",
            "Equipo Operaciones",
            new List<string> { "Comanda", "ItemComanda", "Reservacion", "Mesa", "PreparacionDiaria" }
        );

        public static readonly BoundedContext ComercialContext = new BoundedContext(
            "Comercial",
            "Gestión comercial: clientes, facturación, pagos y promociones",
            "Equipo Comercial",
            new List<string> { "Cliente", "TarjetaFidelizacion", "HistorialPuntos", "Factura", "DetalleFactura", "Pago", "Promocion" }
        );

        public static readonly BoundedContext InventarioContext = new BoundedContext(
            "Inventario",
            "Control de stock, movimientos e inventario de ingredientes",
            "Equipo Inventario",
            new List<string> { "Ingrediente", "MovimientoInventario", "OrdenCompra", "ItemOrdenCompra" }
        );

        public static readonly BoundedContext ProveedoresContext = new BoundedContext(
            "Proveedores",
            "Gestión de proveedores y contactos",
            "Equipo Compras",
            new List<string> { "Proveedor", "ContactoProveedor" }
        );

        // RELACIONES ENTRE CONTEXTOS ACTUALIZADAS

        public static readonly List<ContextRelation> ContextRelations = new List<ContextRelation>
        {
            // Core → Operaciones: Proveedor-Cliente (Upstream-Downstream)
            new ContextRelation(
                CoreContext,
                OperacionesContext,
                ContextRelationship.CustomerSupplier,
                "Core provee productos, usuarios y recetas a Operaciones para crear comandas y gestionar preparaciones"
            ),

            // Operaciones → Inventario: Anti-Corruption Layer
            new ContextRelation(
                OperacionesContext,
                InventarioContext,
                ContextRelationship.AnticorruptionLayer,
                "Operaciones usa OperacionesInventarioIntegrationService para traducir conceptos de comandas a movimientos de inventario"
            ),

            // Operaciones → Comercial: Proveedor-Cliente
            new ContextRelation(
                OperacionesContext,
                ComercialContext,
                ContextRelationship.CustomerSupplier,
                "Operaciones provee comandas finalizadas a Comercial para facturación y gestión de puntos"
            ),

            // Core → Inventario: Conformista Downstream
            new ContextRelation(
                CoreContext,
                InventarioContext,
                ContextRelationship.ConformistDownstream,
                "Inventario se adapta al modelo de productos y recetas definido por Core"
            ),

            // Inventario → Proveedores: Partnership
            new ContextRelation(
                InventarioContext,
                ProveedoresContext,
                ContextRelationship.Partnership,
                "Colaboración estrecha para gestionar órdenes de compra y recepción de mercancía"
            ),

            // Comercial → Proveedores: Anti-Corruption Layer
            new ContextRelation(
                ComercialContext,
                ProveedoresContext,
                ContextRelationship.AnticorruptionLayer,
                "Comercial usa ServicioIntegracionProveedores para sincronizar información de facturación"
            ),

            // Core → Comercial: Conformista Downstream
            new ContextRelation(
                CoreContext,
                ComercialContext,
                ContextRelationship.ConformistDownstream,
                "Comercial se adapta al modelo de usuarios y productos definido por Core"
            ),

            // Core → Proveedores: Separados
            new ContextRelation(
                CoreContext,
                ProveedoresContext,
                ContextRelationship.SeparateWays,
                "Core y Proveedores mantienen modelos independientes con integración mínima"
            )
        };

        /// <summary>
        /// Servicios de integración implementados entre contextos
        /// </summary>
        public static readonly Dictionary<string, List<string>> IntegrationServices = 
            new Dictionary<string, List<string>>
        {
            ["Core-Operaciones"] = new List<string>
            {
                "ICoreOperacionesIntegrationService",
                "ComandaFinalizada_ActualizarProductosHandler"
            },

            ["Operaciones-Inventario"] = new List<string>
            {
                "IOperacionesInventarioIntegrationService",
                "ComandaCreada_VerificarDisponibilidadHandler",
                "ComandaModificada_ActualizarInventarioHandler"
            },

            ["Comercial-Proveedores"] = new List<string>
            {
                "IServicioIntegracionProveedores",
                "ProveedorActualizado_SincronizarInformacionHandler"
            }
        };

        /// <summary>
        /// Nuevas funcionalidades implementadas por contexto
        /// </summary>
        public static readonly Dictionary<string, List<string>> NewFeaturesByContext = 
            new Dictionary<string, List<string>>
        {
            ["Operaciones"] = new List<string>
            {
                "PreparacionDiaria - Gestión de preparaciones diarias del chef",
                "ServicioPreparaciones - Flujo híbrido de preparaciones",
                "Mesa - Gestión completa de mesas del restaurante",
                "MesaBuilder - Constructor fluido para mesas"
            },

            ["Core"] = new List<string>
            {
                "ProductoBuilder - Constructor fluido para productos",
                "RecetaService - Gestión de recetas e ingredientes",
                "UsuarioServiceCached - Caché para usuarios",
                "Result<T> Pattern - Implementado globalmente"
            },

            ["Comercial"] = new List<string>
            {
                "FacturaBuilder - Constructor fluido para facturas",
                "ServicioGestionFacturasVencidas - Gestión de facturas vencidas",
                "Pago - Entidad completa de pagos",
                "INotificationManager - Patrón de notificaciones"
            },

            ["Inventario"] = new List<string>
            {
                "IngredienteBuilder - Constructor fluido para ingredientes",
                "OrdenCompraBuilder - Constructor fluido para órdenes",
                "IngredienteFactory - Factory para ingredientes",
                "StockBajoPolicy - Política de stock bajo"
            },

            ["Proveedores"] = new List<string>
            {
                "ProveedorBuilder - Constructor fluido para proveedores",
                "ContactoProveedor - Entidad de contactos",
                "ProveedorCategoria - Value object de categorías"
            }
        };

        /// <summary>
        /// Patrones arquitectónicos implementados por contexto
        /// </summary>
        public static readonly Dictionary<string, List<string>> ArchitecturalPatterns = 
            new Dictionary<string, List<string>>
        {
            ["Builders"] = new List<string>
            {
                "ComandaBuilder (Operaciones)",
                "ReservacionBuilder (Operaciones)", 
                "MesaBuilder (Operaciones)",
                "FacturaBuilder (Comercial)",
                "ProductoBuilder (Core)",
                "IngredienteBuilder (Inventario)",
                "OrdenCompraBuilder (Inventario)",
                "ProveedorBuilder (Proveedores)"
            },

            ["Factories"] = new List<string>
            {
                "ClienteFactory (Comercial)",
                "IngredienteFactory (Inventario)"
            },

            ["IntegrationServices"] = new List<string>
            {
                "CoreOperacionesIntegrationService",
                "OperacionesInventarioIntegrationService",
                "ServicioIntegracionProveedores"
            },

            ["Specifications"] = new List<string>
            {
                "ClienteFrecuenteSpecification (Comercial)",
                "IngredienteDisponibleSpecification (Inventario)",
                "ProveedorActivoSpecification (Proveedores)",
                "ReservacionValidaSpecification (Operaciones)"
            }
        };
    }
}
