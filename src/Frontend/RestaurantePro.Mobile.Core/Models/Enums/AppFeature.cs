namespace RestaurantePro.Mobile.Core.Models.Enums;

/// <summary>
/// Funcionalidades principales de la aplicación móvil
/// Agrupa permisos relacionados por área funcional
/// </summary>
public enum AppFeature
{
    /// <summary>
    /// Funcionalidad completa de gestión de comandas
    /// Incluye: crear, ver, modificar, cerrar comandas
    /// Rol principal: Meseros
    /// </summary>
    GestionComandas = 1,
    
    /// <summary>
    /// Funcionalidad de gestión de mesas
    /// Incluye: ver estados, cambiar estados, asignar mesas
    /// Rol principal: Meseros
    /// </summary>
    GestionMesas = 2,
    
    /// <summary>
    /// Funcionalidad de cocina y preparaciones
    /// Incluye: ver órdenes pendientes, actualizar estados, completar
    /// Rol principal: Cocineros
    /// </summary>
    Cocina = 3,
    
    /// <summary>
    /// Funcionalidad de caja y facturación
    /// Incluye: generar facturas, procesar pagos, aplicar promociones
    /// Rol principal: Cajeros
    /// </summary>
    Caja = 4,
    
    /// <summary>
    /// Funcionalidad de consulta de productos y menú
    /// Incluye: consultar productos, ver detalles
    /// Acceso: Todos los roles operativos
    /// </summary>
    ConsultaProductos = 5,
    
    /// <summary>
    /// Funcionalidad básica de atención al cliente
    /// Incluye: consultar clientes, usar tarjetas fidelización
    /// Acceso: Meseros, Cajeros
    /// </summary>
    AtencionCliente = 6,
    
    /// <summary>
    /// Funcionalidad de consulta de inventario
    /// Incluye: disponibilidad ingredientes, alertas
    /// Acceso: Cocineros principalmente
    /// </summary>
    ConsultaInventario = 7,
    
    /// <summary>
    /// Funcionalidad de gestión de reservaciones
    /// Incluye: consultar, confirmar reservaciones
    /// Acceso: Meseros, Supervisores
    /// </summary>
    Reservaciones = 8,
    
    /// <summary>
    /// Funcionalidad de supervisión y administración
    /// Incluye: reportes básicos, gestión personal, supervisión
    /// Acceso: Supervisores, Gerentes
    /// </summary>
    Supervision = 9,
    
    /// <summary>
    /// Funcionalidad de configuración personal
    /// Incluye: perfil, configuraciones personales
    /// Acceso: Todos los usuarios autenticados
    /// </summary>
    ConfiguracionPersonal = 10
}
