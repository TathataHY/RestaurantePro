using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Usuario> Usuarios { get; set; }
        DbSet<Producto> Productos { get; set; }
        DbSet<Categoria> Categorias { get; set; }
        DbSet<Ingrediente> Ingredientes { get; set; }
        DbSet<IngredienteProducto> IngredientesProductos { get; set; }
        DbSet<Mesa> Mesas { get; set; }
        DbSet<Comanda> Comandas { get; set; }
        DbSet<ComandaDetalle> ComandaDetalles { get; set; }
        DbSet<ComandaDetallePersonalizacion> ComandaDetallePersonalizaciones { get; set; }
        DbSet<Pago> Pagos { get; set; }
        DbSet<Reservacion> Reservaciones { get; set; }
        DbSet<HistorialEstadoMesa> HistorialEstadosMesa { get; set; }
        
        // Nuevas entidades para el sistema de fidelización
        DbSet<Cliente> Clientes { get; set; }
        DbSet<Promocion> Promociones { get; set; }
        DbSet<TarjetaFidelizacion> TarjetasFidelizacion { get; set; }
        DbSet<NivelFidelizacion> NivelesFidelizacion { get; set; }
        DbSet<HistorialPuntos> HistorialPuntos { get; set; }
        
        // Entidades para el sistema de inventario
        DbSet<Inventario> Inventarios { get; set; }
        DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        DbSet<Proveedor> Proveedores { get; set; }
        DbSet<OrdenCompra> OrdenesCompra { get; set; }
        DbSet<DetalleOrdenCompra> DetallesOrdenCompra { get; set; }
        DbSet<ProveedorCategoria> ProveedoresCategorias { get; set; }
        DbSet<ProveedorIngrediente> ProveedoresIngredientes { get; set; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
} 