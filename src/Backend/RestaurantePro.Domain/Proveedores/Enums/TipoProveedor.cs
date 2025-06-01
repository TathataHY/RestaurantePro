namespace RestaurantePro.Domain.Proveedores.Enums;

/// <summary>
/// Tipo de proveedor según el servicio que proporciona
/// </summary>
public enum TipoProveedor
{
    /// <summary>
    /// Proveedor de ingredientes y materias primas
    /// </summary>
    Ingredientes = 1,

    /// <summary>
    /// Proveedor de bebidas alcohólicas y no alcohólicas
    /// </summary>
    Bebidas = 2,

    /// <summary>
    /// Proveedor de carnes y productos cárnicos
    /// </summary>
    Carnes = 3,

    /// <summary>
    /// Proveedor de verduras y productos frescos
    /// </summary>
    Verduras = 4,

    /// <summary>
    /// Proveedor de lácteos y derivados
    /// </summary>
    Lacteos = 5,

    /// <summary>
    /// Proveedor de productos de panadería
    /// </summary>
    Panaderia = 6,

    /// <summary>
    /// Proveedor de productos de limpieza
    /// </summary>
    Limpieza = 7,

    /// <summary>
    /// Proveedor de equipos y utensilios de cocina
    /// </summary>
    Equipos = 8,

    /// <summary>
    /// Proveedor de servicios de mantenimiento
    /// </summary>
    Mantenimiento = 9,

    /// <summary>
    /// Proveedor de materiales desechables
    /// </summary>
    Desechables = 10,

    /// <summary>
    /// Proveedor mixto que ofrece múltiples categorías
    /// </summary>
    Mixto = 11,

    /// <summary>
    /// Proveedor local de la misma ciudad o región
    /// </summary>
    Local = 12,

    /// <summary>
    /// Proveedor nacional del mismo país
    /// </summary>
    Nacional = 13,

    /// <summary>
    /// Proveedor internacional de otros países
    /// </summary>
    Internacional = 14
} 