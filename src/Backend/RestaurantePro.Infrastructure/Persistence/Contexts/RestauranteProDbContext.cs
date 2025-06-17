using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Linq;

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
        
        protected RestauranteProDbContext(
            DbContextOptions options,
            ILogger<RestauranteProDbContext> logger) : base(options)
        {
            _logger = logger;
        }
        
        // Core - Productos
        public DbSet<Domain.Core.Productos.Entities.Producto> Productos { get; set; }
        
        // Core - Usuarios  
        public DbSet<Domain.Core.Usuarios.Entities.Usuario> Usuarios { get; set; }
        
        // Core - Notificaciones
        public DbSet<Domain.Core.Notificaciones.Entities.Notificacion> Notificaciones { get; set; }
        
        // Comercial - Clientes
        public DbSet<Domain.Comercial.Clientes.Entities.Cliente> Clientes { get; set; }
        
        // Comercial - Fidelización
        public DbSet<Domain.Comercial.Clientes.Entities.TarjetaFidelizacion> TarjetasFidelizacion { get; set; }
        
        // Comercial - Facturación
        public DbSet<Domain.Comercial.Facturacion.Entities.Factura> Facturas { get; set; }
        
        // Operaciones - Comandas
        public DbSet<Domain.Operaciones.Comandas.Entities.Comanda> Comandas { get; set; }
        public DbSet<Domain.Operaciones.Comandas.Entities.ItemComanda> ItemsComanda { get; set; }
        
        // Operaciones - Reservaciones
        public DbSet<Domain.Operaciones.Reservaciones.Entities.Reservacion> Reservaciones { get; set; }
        
        // Operaciones - Mesas
        public DbSet<Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa> Mesas { get; set; }
        
        // Operaciones - Preparaciones
        public DbSet<Domain.Operaciones.Preparaciones.Entities.PreparacionDiaria> Preparaciones { get; set; }
        
        // Inventario - Ingredientes
        public DbSet<Domain.Inventario.Ingredientes.Entities.Ingrediente> Ingredientes { get; set; }
        public DbSet<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> MovimientosInventario { get; set; }
        
        // Inventario - Órdenes de Compra
        public DbSet<Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra> OrdenesCompra { get; set; }
        
        // Proveedores
        public DbSet<Domain.Proveedores.Entities.Proveedor> Proveedores { get; set; }
        public DbSet<Domain.Proveedores.Entities.ContactoProveedor> ContactosProveedor { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Ignorar la clase base de eventos de dominio para que no se cree una tabla
            modelBuilder.Ignore<Domain.Core.Base.Events.DomainEvent>();
            
            // Aplicar configuraciones de entidades
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Configurar convenciones globales
            ConfigurarConvencionesGlobales(modelBuilder);

            // Filtro global para entidades con borrado lógico (soft-delete)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!entityType.IsOwned() && typeof(Domain.Core.Base.EntityBase).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(ConvertFilterExpression<Domain.Core.Base.EntityBase>(
                            e => !e.EstaEliminado, entityType.ClrType));
                }
            }
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

        private static LambdaExpression ConvertFilterExpression<TInterface>(
            Expression<Func<TInterface, bool>> filterExpression,
            Type entityType)
        {
            var newParam = Expression.Parameter(entityType);
            var newBody = ReplacingExpressionVisitor.Replace(filterExpression.Parameters.Single(), newParam, filterExpression.Body);
            return Expression.Lambda(newBody, newParam);
        }

        private class ReplacingExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldExpression;
            private readonly Expression _newExpression;

            private ReplacingExpressionVisitor(Expression oldExpression, Expression newExpression)
            {
                _oldExpression = oldExpression;
                _newExpression = newExpression;
            }

            public override Expression? Visit(Expression? node)
            {
                return node == _oldExpression ? _newExpression : base.Visit(node);
            }

            public static Expression Replace(Expression oldExpression, Expression newExpression, Expression body)
            {
                return new ReplacingExpressionVisitor(oldExpression, newExpression).Visit(body);
            }
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var resultado = await base.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Se guardaron {Count} cambios en la base de datos", resultado);
                return resultado;
            }
            catch (System.Exception ex)
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