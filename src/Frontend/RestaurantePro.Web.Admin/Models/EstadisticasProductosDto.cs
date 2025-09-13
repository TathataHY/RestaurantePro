namespace RestaurantePro.Web.Admin.Models;

public class EstadisticasProductosDto
{
    public int TotalProductos { get; set; }
    public int ProductosActivos { get; set; }
    public int ProductosInactivos { get; set; }
    public decimal PrecioPromedio { get; set; }
    public decimal PrecioMinimo { get; set; }
    public decimal PrecioMaximo { get; set; }
    public List<ProductosPorCategoriaDto> ProductosPorCategoria { get; set; } = new();
    public List<ProductosPorPopularidadDto> ProductosPorPopularidad { get; set; } = new();
    public List<ProductoDto> ProductosRecientes { get; set; } = new();
}

public class ProductosPorCategoriaDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class ProductosPorPopularidadDto
{
    public int Nivel { get; set; }
    public int Cantidad { get; set; }
}
