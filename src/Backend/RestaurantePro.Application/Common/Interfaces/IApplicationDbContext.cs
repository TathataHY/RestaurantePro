namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el contexto de base de datos de la aplicación
/// Proporciona acceso a todas las entidades del dominio
/// </summary>
public interface IApplicationDbContext
{
    // Core - Productos
    DbSet<Producto> Productos { get; }
    DbSet<Receta> Recetas { get; }
    
    // Core - Usuarios  
    DbSet<Usuario> Usuarios { get; }
    
    // Core - Notificaciones
    DbSet<Notificacion> Notificaciones { get; }
    
    // Comercial - Clientes
    DbSet<Cliente> Clientes { get; }
    
    // Comercial - Fidelización
    DbSet<TarjetaFidelizacion> TarjetasFidelizacion { get; }
    
    // Comercial - Facturación
    DbSet<Factura> Facturas { get; }
    
    // Comercial - Promociones  
    DbSet<Promocion> Promociones { get; }
    // TODO: Agregar cuando las entidades estén disponibles en el dominio
    // DbSet<AplicacionPromocion> AplicacionesPromocion { get; }
    // DbSet<DescuentoFactura> DescuentosFactura { get; }
    // DbSet<DescuentoComanda> DescuentosComanda { get; }
    
    // Operaciones - Comandas
    DbSet<Comanda> Comandas { get; }
    DbSet<ItemComanda> ItemsComanda { get; }
    
    // Operaciones - Reservaciones
    DbSet<Reservacion> Reservaciones { get; }
    
    // Operaciones - Mesas
    DbSet<Mesa> Mesas { get; }
    
    // Operaciones - Preparaciones
    DbSet<PreparacionDiaria> Preparaciones { get; }
    
    // Inventario - Ingredientes
    DbSet<Ingrediente> Ingredientes { get; }
    DbSet<MovimientoInventario> MovimientosInventario { get; }
    
    // Inventario - Órdenes de Compra
    DbSet<OrdenCompra> OrdenesCompra { get; }
    
    // Proveedores
    DbSet<Proveedor> Proveedores { get; }
    DbSet<ContactoProveedor> ContactosProveedor { get; }
    
    // Auditoría
    // TODO: Agregar cuando la entidad esté disponible en el dominio
    // DbSet<RegistroAuditoria> RegistrosAuditoria { get; }
    
    // 🆕 Entidades de Auditoría y Seguridad (cuando estén disponibles en el dominio)
    // DbSet<EventoAuditoria> EventosAuditoria { get; }
    // DbSet<HistorialPassword> HistorialPasswords { get; }
    // DbSet<SesionUsuario> SesionesUsuario { get; }
    
    // 🆕 Entidades de Administración de Usuarios (cuando estén disponibles en el dominio)
    // DbSet<ActualizacionUsuarioProgramada> ActualizacionesUsuariosProgramadas { get; }
    // DbSet<AprobacionCambioUsuario> AprobacionesCambiosUsuario { get; }
    // DbSet<BackupUsuario> BackupsUsuarios { get; }
    // DbSet<TareaProgramada> TareasProgramadas { get; }
    
    // 🆕 Entidades de Reservaciones (cuando estén disponibles en el dominio)
    // DbSet<AuditoriaReservacion> AuditoriasReservaciones { get; }
    
    // 🆕 Entidades de Usuarios - relaciones (cuando estén disponibles en el dominio)
    // DbSet<UsuarioSucursal> UsuarioSucursales { get; }
    // DbSet<PerfilUsuario> PerfilesUsuario { get; }
    // DbSet<Sucursal> Sucursales { get; }
    
    // 🆕 Entidades de Seguridad (cuando estén disponibles en el dominio)
    // DbSet<IPBloqueada> IPsBloqueadas { get; }
    
    /// <summary>
    /// Guarda los cambios de forma asíncrona
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de entidades afectadas</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Comienza una transacción de base de datos
    /// </summary>
    /// <returns>Transacción de base de datos</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
} 