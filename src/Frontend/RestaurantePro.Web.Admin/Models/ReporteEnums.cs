namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// Enumeración para tipos de reportes comerciales
/// </summary>
public enum TipoReporteComercial
{
    VentasDiarias = 1,
    VentasMensuales = 2,
    ProductosMasVendidos = 3,
    ClientesFrecuentes = 4,
    ComandasPorMesa = 5,
    Facturacion = 6,
    Promociones = 7,
    TiempoPromedioServicio = 8,
    IngresosPorCategoria = 9,
    AnalisisTendencias = 10
}

/// <summary>
/// Enumeración para tipos de reportes de inventario
/// </summary>
public enum TipoReporteInventario
{
    StockActual = 1,
    MovimientosInventario = 2,
    IngredientesVencidos = 3,
    StockBajo = 4,
    ValorInventario = 5,
    ConsumoIngredientes = 6,
    OrdenesCompra = 7,
    AnalisisCostos = 8,
    RotacionInventario = 9,
    PrediccionNecesidades = 10
}
