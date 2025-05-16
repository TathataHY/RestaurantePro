
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
    /// Mapa de contextos completo del sistema
    /// </summary>
    public static class RestauranteProContextMap
    {
        // Definición de Bounded Contexts
        public static readonly BoundedContext CatalogoContext = new BoundedContext(
            "Catálogo",
            "Gestión de productos, categorías e ingredientes",
            "Equipo Productos",
            new List<string> { "Producto", "Categoria", "Ingrediente" }
        );
        
        public static readonly BoundedContext OperacionesContext = new BoundedContext(
            "Operaciones",
            "Gestión de comandas, mesas y reservaciones",
            "Equipo Operaciones",
            new List<string> { "Comanda", "Mesa", "Reservacion" }
        );
        
        public static readonly BoundedContext InventarioContext = new BoundedContext(
            "Inventario",
            "Control de stock, movimientos y compras",
            "Equipo Inventario",
            new List<string> { "Inventario", "MovimientoInventario", "OrdenCompra" }
        );
        
        public static readonly BoundedContext ClientesContext = new BoundedContext(
            "Clientes",
            "Gestión de clientes, fidelización y promociones",
            "Equipo Comercial",
            new List<string> { "Cliente", "TarjetaFidelizacion", "Promocion" }
        );
        
        public static readonly BoundedContext ProveedoresContext = new BoundedContext(
            "Proveedores",
            "Gestión de proveedores",
            "Equipo Compras",
            new List<string> { "Proveedor", "ProveedorCategoria" }
        );
        
        public static readonly BoundedContext PagosContext = new BoundedContext(
            "Pagos",
            "Gestión de pagos y transacciones financieras",
            "Equipo Finanzas",
            new List<string> { "Pago", "Transaccion" }
        );
        
        public static readonly BoundedContext IdentidadContext = new BoundedContext(
            "Identidad",
            "Gestión de usuarios, roles y permisos",
            "Equipo Seguridad",
            new List<string> { "Usuario", "Rol", "Permiso" }
        );
        
        // Definición de relaciones entre contextos
        public static readonly List<ContextRelation> ContextRelations = new List<ContextRelation>
        {
            new ContextRelation(
                CatalogoContext,
                OperacionesContext,
                ContextRelationship.CustomerSupplier,
                "Operaciones consume productos del Catálogo para crear comandas"
            ),
            
            new ContextRelation(
                InventarioContext,
                CatalogoContext,
                ContextRelationship.ConformistDownstream,
                "Inventario se adapta al modelo de productos definido por Catálogo"
            ),
            
            new ContextRelation(
                OperacionesContext,
                PagosContext,
                ContextRelationship.CustomerSupplier,
                "Pagos procesa transacciones para las comandas de Operaciones"
            ),
            
            new ContextRelation(
                ProveedoresContext,
                InventarioContext,
                ContextRelationship.Partnership,
                "Colaboración para gestionar la compra y recepción de productos"
            ),
            
            new ContextRelation(
                ClientesContext,
                OperacionesContext,
                ContextRelationship.AnticorruptionLayer,
                "Clientes usa una capa de traducción para consumir datos de Operaciones"
            ),
            
            new ContextRelation(
                IdentidadContext,
                OperacionesContext,
                ContextRelationship.ConformistUpstream,
                "Identidad proporciona usuarios y roles a Operaciones"
            )
        };
    }
}
