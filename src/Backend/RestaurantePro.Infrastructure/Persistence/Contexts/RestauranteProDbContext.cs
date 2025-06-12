using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Core.Base.Entities;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Mesas.Entities;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Persistence.Interceptors;

namespace RestaurantePro.Infrastructure.Persistence.Contexts
{
    public class RestauranteProDbContext : DbContext, IApplicationDbContext
    {
        private readonly ILogger<RestauranteProDbContext> _logger;
        
        public RestauranteProDbContext(
            DbContextOptions<RestauranteProDbContext> options,
            ILogger<RestauranteProDbContext> logger) : base(options)
        {
            _logger = logger;
        }
        
        // Core - Productos
        public DbSet<Producto> Productos => Set<Producto>();
        
        // Core - Usuarios  
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        
        // Core - Notificaciones
        public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
        
        // Core - Otros
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();
        public DbSet<IngredienteProducto> IngredientesProductos { get; set; }
        public DbSet<ProductoIngrediente> ProductoIngredientes { get; set; }
        
        // Comercial - Clientes
        public DbSet<Cliente> Clientes => Set<Cliente>();
        
        // Comercial - Fidelización
        public DbSet<TarjetaFidelizacion> TarjetasFidelizacion => Set<TarjetaFidelizacion>();
        
        // Comercial - Facturación
        public DbSet<Factura> Facturas => Set<Factura>();
        
        // Comercial - Pagos
        public DbSet<Pago> Pagos { get; set; }
        
        // Operaciones - Comandas
        public DbSet<Comanda> Comandas => Set<Comanda>();
        public DbSet<ItemComanda> ItemsComanda => Set<ItemComanda>();
        public DbSet<ComandaDetalle> ComandaDetalles { get; set; }
        public DbSet<ComandaDetallePersonalizacion> ComandaDetallePersonalizaciones { get; set; }
        
        // Operaciones - Reservaciones
        public DbSet<Reservacion> Reservaciones => Set<Reservacion>();
        
        // Operaciones - Mesas
        public DbSet<Mesa> Mesas => Set<Mesa>();
        
        // Operaciones - Preparaciones
        public DbSet<PreparacionDiaria> Preparaciones => Set<PreparacionDiaria>();
        
        // Inventario - Movimientos
        public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
        public DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }
        
        // Inventario - Órdenes de Compra
        public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
        
        // Proveedores
        public DbSet<Proveedor> Proveedores => Set<Proveedor>();
        public DbSet<ContactoProveedor> ContactosProveedor => Set<ContactoProveedor>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Aplicar configuraciones de entidades
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Configurar convenciones globales
            ConfigurarConvencionesGlobales(modelBuilder);
        }
        
        private void ConfigurarConvencionesGlobales(ModelBuilder modelBuilder)
        {
            // Configuraciones globales para todas las entidades
            // Por ejemplo:
            
            // 1. Todos los strings tienen longitud máxima de 256 por defecto
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
                    {
                        property.SetMaxLength(256);
                    }
                }
            }
            
            // 2. Todos los decimales tienen precisión 18, 2 por defecto
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetProperties())
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Aplicar auditoría automática (en el futuro)
                
                var resultado = await base.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Se guardaron {Count} cambios en la base de datos", resultado);
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios en la base de datos");
                throw;
            }
        }
        
        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await Database.BeginTransactionAsync(cancellationToken);
        }
    }
} 