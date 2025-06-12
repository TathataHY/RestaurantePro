# Capa de Infraestructura - RestaurantePro

Esta capa implementa los detalles técnicos y proporciona implementaciones concretas para las abstracciones definidas en las capas de dominio y aplicación, manteniendo la misma estructura de contextos.

## 🏗️ **Estructura de Contextos (Alineada con Domain)**

```
Infrastructure/
├── Persistence/             # Persistencia de datos por contextos
│   ├── Contexts/            # Contextos de EF Core
│   │   ├── RestauranteProDbContext.cs
│   │   ├── CoreDbContext.cs
│   │   ├── ComercialDbContext.cs
│   │   ├── OperacionesDbContext.cs
│   │   ├── InventarioDbContext.cs
│   │   └── ProveedoresDbContext.cs
│   ├── Migrations/          # Migraciones de base de datos por contexto
│   │   ├── Core/
│   │   ├── Comercial/
│   │   ├── Operaciones/
│   │   ├── Inventario/
│   │   └── Proveedores/
│   ├── Configurations/      # Configuración de entidades por contexto
│   │   ├── Core/            # Configuraciones del contexto Core
│   │   │   ├── ProductoConfiguration.cs
│   │   │   ├── UsuarioConfiguration.cs
│   │   │   ├── NotificacionConfiguration.cs
│   │   │   └── RecetaConfiguration.cs
│   │   ├── Comercial/       # Configuraciones del contexto Comercial
│   │   │   ├── ClienteConfiguration.cs
│   │   │   ├── FacturaConfiguration.cs
│   │   │   └── TarjetaFidelizacionConfiguration.cs
│   │   ├── Operaciones/     # Configuraciones del contexto Operaciones
│   │   │   ├── ComandaConfiguration.cs
│   │   │   ├── ReservacionConfiguration.cs
│   │   │   ├── MesaConfiguration.cs
│   │   │   └── PreparacionDiariaConfiguration.cs
│   │   ├── Inventario/      # Configuraciones del contexto Inventario
│   │   │   ├── IngredienteConfiguration.cs
│   │   │   ├── MovimientoInventarioConfiguration.cs
│   │   │   └── OrdenCompraConfiguration.cs
│   │   └── Proveedores/     # Configuraciones del contexto Proveedores
│   │       ├── ProveedorConfiguration.cs
│   │       └── ContactoProveedorConfiguration.cs
│   ├── Interceptors/        # Interceptores para auditoría, logging, etc.
│   │   ├── AuditableEntityInterceptor.cs
│   │   ├── DomainEventInterceptor.cs
│   │   └── SoftDeleteInterceptor.cs
│   └── Repositories/        # Implementaciones de repositorios por contexto
│       ├── Core/            # Repositorios del contexto Core
│       │   ├── ProductoRepository.cs
│       │   ├── UsuarioRepository.cs
│       │   ├── NotificacionRepository.cs
│       │   └── RecetaRepository.cs
│       ├── Comercial/       # Repositorios del contexto Comercial
│       │   ├── ClienteRepository.cs
│       │   ├── FacturaRepository.cs
│       │   └── TarjetaFidelizacionRepository.cs
│       ├── Operaciones/     # Repositorios del contexto Operaciones
│       │   ├── ComandaRepository.cs
│       │   ├── ReservacionRepository.cs
│       │   ├── MesaRepository.cs
│       │   └── PreparacionDiariaRepository.cs
│       ├── Inventario/      # Repositorios del contexto Inventario
│       │   ├── IngredienteRepository.cs
│       │   ├── MovimientoInventarioRepository.cs
│       │   └── OrdenCompraRepository.cs
│       ├── Proveedores/     # Repositorios del contexto Proveedores
│       │   ├── ProveedorRepository.cs
│       │   └── ContactoProveedorRepository.cs
│       └── Base/            # Repositorios base y comunes
│           ├── Repository.cs
│           └── UnitOfWork.cs
│
├── Identity/                # Autenticación y autorización
│   ├── Models/              # Modelos para identidad (AspNetCore.Identity)
│   │   ├── ApplicationUser.cs
│   │   ├── ApplicationRole.cs
│   │   └── ApplicationUserRole.cs
│   ├── Services/            # Servicios de identidad
│   │   ├── IdentityService.cs
│   │   ├── JwtTokenService.cs
│   │   └── PermissionService.cs
│   ├── Configuration/       # Configuración de identidad
│   │   ├── IdentityConfiguration.cs
│   │   └── JwtConfiguration.cs
│   └── Extensions/          # Extensiones de identidad
│       ├── ClaimsPrincipalExtensions.cs
│       └── IdentityResultExtensions.cs
│
├── ExternalServices/        # Integración con servicios externos
│   ├── Payment/             # Servicios de pago
│   │   ├── PayPalService.cs
│   │   ├── StripeService.cs
│   │   └── Interfaces/
│   │       └── IPaymentService.cs
│   ├── Email/               # Servicios de correo electrónico
│   │   ├── EmailService.cs
│   │   ├── SendGridService.cs
│   │   └── Interfaces/
│   │       └── IEmailService.cs
│   ├── SMS/                 # Servicios de mensajería SMS
│   │   ├── TwilioService.cs
│   │   └── Interfaces/
│   │       └── ISMSService.cs
│   ├── FileStorage/         # Almacenamiento de archivos
│   │   ├── LocalFileService.cs
│   │   ├── AzureBlobService.cs
│   │   └── Interfaces/
│   │       └── IFileStorageService.cs
│   └── Integrations/        # Integraciones específicas por contexto
│       ├── SAT/             # Integración con SAT (facturación México)
│       │   ├── SATService.cs
│       │   └── Models/
│       └── POS/             # Integración con sistemas POS
│           ├── POSService.cs
│           └── Models/
│
├── BackgroundTasks/         # Tareas en segundo plano por contexto
│   ├── Jobs/                # Definición de trabajos por contexto
│   │   ├── Core/
│   │   │   ├── NotificationCleanupJob.cs
│   │   │   └── UserInactivityJob.cs
│   │   ├── Comercial/
│   │   │   ├── LoyaltyPointsExpirationJob.cs
│   │   │   └── InvoiceReminderJob.cs
│   │   ├── Operaciones/
│   │   │   ├── TableCleanupJob.cs
│   │   │   └── ReservationReminderJob.cs
│   │   ├── Inventario/
│   │   │   ├── LowStockAlertJob.cs
│   │   │   └── ExpirationCheckJob.cs
│   │   └── Proveedores/
│   │       ├── SupplierEvaluationJob.cs
│   │       └── ContractRenewalJob.cs
│   ├── Workers/             # Workers para procesamiento
│   │   ├── EmailWorker.cs
│   │   ├── NotificationWorker.cs
│   │   └── ReportGenerationWorker.cs
│   ├── Schedulers/          # Planificadores de tareas
│   │   ├── HangfireScheduler.cs
│   │   └── QuartzScheduler.cs
│   └── Interfaces/          # Interfaces de background tasks
│       ├── IBackgroundJob.cs
│       └── IJobScheduler.cs
│
├── Caching/                 # Sistema de caché distribuido
│   ├── Services/            # Servicios de caché por contexto
│   │   ├── RedisCacheService.cs
│   │   ├── MemoryCacheService.cs
│   │   └── Interfaces/
│   │       └── ICacheService.cs
│   ├── Configuration/       # Configuración de caché
│   │   ├── CacheConfiguration.cs
│   │   └── CachePolicies.cs
│   └── Extensions/          # Extensiones de caché
│       ├── CacheExtensions.cs
│       └── CacheKeyGenerator.cs
│
├── Logging/                 # Configuración de logging
│   ├── Providers/           # Proveedores de logging
│   │   ├── SerilogProvider.cs
│   │   └── ApplicationInsightsProvider.cs
│   ├── Enrichers/           # Enriquecedores de logs
│   │   ├── UserEnricher.cs
│   │   ├── CorrelationEnricher.cs
│   │   └── ContextEnricher.cs
│   └── Configuration/       # Configuración de logging
│       ├── LoggingConfiguration.cs
│       └── LoggingPolicies.cs
│
├── Monitoring/              # Monitoreo y observabilidad
│   ├── HealthChecks/        # Health checks por contexto
│   │   ├── DatabaseHealthCheck.cs
│   │   ├── ExternalServiceHealthCheck.cs
│   │   └── CacheHealthCheck.cs
│   ├── Metrics/             # Métricas personalizadas
│   │   ├── BusinessMetrics.cs
│   │   └── PerformanceMetrics.cs
│   └── Telemetry/           # Telemetría
│       ├── ApplicationTelemetry.cs
│       └── CustomTelemetry.cs
│
├── DependencyInjection/     # Registro de servicios por contexto
│   ├── PersistenceSetup.cs  # Configuración de persistencia
│   ├── IdentitySetup.cs     # Configuración de identidad
│   ├── ExternalServicesSetup.cs # Configuración de servicios externos
│   ├── CachingSetup.cs      # Configuración de caché
│   ├── LoggingSetup.cs      # Configuración de logging
│   ├── BackgroundTasksSetup.cs # Configuración de tareas background
│   └── InfrastructureSetup.cs # Configuración general
│
└── Common/                  # Componentes comunes de infraestructura
    ├── Extensions/          # Extensiones útiles
    │   ├── DbContextExtensions.cs
    │   ├── ConfigurationExtensions.cs
    │   └── ServiceCollectionExtensions.cs
    ├── Helpers/             # Clases auxiliares
    │   ├── ConnectionStringHelper.cs
    │   ├── EncryptionHelper.cs
    │   └── FileHelper.cs
    ├── Constants/           # Constantes de infraestructura
    │   ├── ConnectionNames.cs
    │   ├── CacheKeys.cs
    │   └── TableNames.cs
    └── Migrations/          # Utilidades para migraciones
        ├── MigrationExtensions.cs
        └── SeedDataExtensions.cs
```

## 🗄️ **Configuración de Entity Framework Core por Contextos**

### **🔧 DbContext Principal**
```csharp
// RestauranteProDbContext.cs
public class RestauranteProDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    
    public RestauranteProDbContext(
        DbContextOptions<RestauranteProDbContext> options,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IDomainEventDispatcher domainEventDispatcher) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _domainEventDispatcher = domainEventDispatcher;
    }
    
    // DbSets por contexto - Core
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }
    public DbSet<Receta> Recetas { get; set; }
    public DbSet<IngredienteReceta> IngredientesRecetas { get; set; }
    
    // DbSets por contexto - Comercial
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<TarjetaFidelizacion> TarjetasFidelizacion { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<DetalleFactura> DetallesFacturas { get; set; }
    
    // DbSets por contexto - Operaciones
    public DbSet<Comanda> Comandas { get; set; }
    public DbSet<ItemComanda> ItemsComandas { get; set; }
    public DbSet<Reservacion> Reservaciones { get; set; }
    public DbSet<Mesa> Mesas { get; set; }
    public DbSet<PreparacionDiaria> PreparacionesDiarias { get; set; }
    
    // DbSets por contexto - Inventario
    public DbSet<Ingrediente> Ingredientes { get; set; }
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
    public DbSet<OrdenCompra> OrdenesCompra { get; set; }
    
    // DbSets por contexto - Proveedores
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<ContactoProveedor> ContactosProveedores { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Aplicar configuraciones por contexto
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly(),
            type => type.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)));
        
        // Configurar convenciones globales
        ConfigurarConvencionesGlobales(modelBuilder);
        
        // Configurar filtros globales
        ConfigurarFiltrosGlobales(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Aplicar auditoría automática
        AplicarAuditoria();
        
        // Publicar eventos de dominio antes de guardar
        await PublicarEventosDominio();
        
        var resultado = await base.SaveChangesAsync(cancellationToken);
        
        return resultado;
    }
    
    private void AplicarAuditoria()
    {
        foreach (var entry in ChangeTracker.Entries<EntityBase<Guid>>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.FechaCreacion = _dateTimeService.Now;
                    entry.Entity.CreadoPor = _currentUserService.UsuarioId;
                    break;
                case EntityState.Modified:
                    entry.Entity.FechaModificacion = _dateTimeService.Now;
                    entry.Entity.ModificadoPor = _currentUserService.UsuarioId;
                    break;
            }
        }
    }
    
    private async Task PublicarEventosDominio()
    {
        var entidadesConEventos = ChangeTracker.Entries<EntityBase<Guid>>()
            .Where(e => e.Entity.EventosDominio.Any())
            .Select(e => e.Entity)
            .ToList();
            
        foreach (var entidad in entidadesConEventos)
        {
            var eventos = entidad.EventosDominio.ToList();
            entidad.LimpiarEventosDominio();
            
            foreach (var evento in eventos)
            {
                await _domainEventDispatcher.PublishAsync(evento);
            }
        }
    }
}
```

### **📊 Configuraciones de Entidades por Contexto**

```csharp
// Core/ProductoConfiguration.cs
public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos", "Core");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.Descripcion)
            .HasMaxLength(500);
            
        builder.Property(p => p.Precio)
            .HasPrecision(18, 2);
            
        // Configurar enum como string
        builder.Property(p => p.Categoria)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        // Configurar relaciones
        builder.HasMany(p => p.Recetas)
            .WithOne()
            .HasForeignKey("ProductoId")
            .OnDelete(DeleteBehavior.Cascade);
            
        // Índices
        builder.HasIndex(p => p.Nombre)
            .HasDatabaseName("IX_Productos_Nombre");
            
        builder.HasIndex(p => new { p.Categoria, p.Activo })
            .HasDatabaseName("IX_Productos_Categoria_Activo");
    }
}
```

## 🏪 **Implementación de Repositorios por Contexto**

### **📦 Repositorio Base**
```csharp
// Base/RepositoryBase.cs
public abstract class RepositoryBase<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : EntityBase<TKey>
    where TKey : IEquatable<TKey>
{
    protected readonly RestauranteProDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger<RepositoryBase<TEntity, TKey>> _logger;
    
    protected RepositoryBase(
        RestauranteProDbContext context,
        ILogger<RepositoryBase<TEntity, TKey>> logger)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _logger = logger;
    }
    
    public virtual async Task<TEntity?> ObtenerPorIdAsync(TKey id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public virtual async Task<IEnumerable<TEntity>> ObtenerTodosAsync()
    {
        return await _dbSet.Where(e => e.Activo).ToListAsync();
    }
    
    public virtual async Task<IEnumerable<TEntity>> ObtenerPorSpecAsync<TSpec>(TSpec specification)
        where TSpec : ISpecification<TEntity>
    {
        return await _dbSet.Where(specification.Criteria).ToListAsync();
    }
    
    public virtual async Task<TEntity> AgregarAsync(TEntity entity)
    {
        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity;
    }
    
    public virtual Task ActualizarAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
    
    public virtual Task EliminarAsync(TKey id)
    {
        // Eliminación lógica
        var entity = _dbSet.Find(id);
        if (entity != null)
        {
            entity.Activo = false;
            _dbSet.Update(entity);
        }
        return Task.CompletedTask;
    }
}
```

### **🛒 Repositorio Específico - Ejemplo Comercial**
```csharp
// Comercial/ClienteRepository.cs
public class ClienteRepository : RepositoryBase<Cliente, Guid>, IClienteRepository
{
    public ClienteRepository(
        RestauranteProDbContext context,
        ILogger<ClienteRepository> logger) 
        : base(context, logger)
    {
    }
    
    public async Task<Cliente?> ObtenerPorEmailAsync(string email)
    {
        return await _dbSet
            .Include(c => c.TarjetaFidelizacion)
            .FirstOrDefaultAsync(c => c.Email.Valor == email && c.Activo);
    }
    
    public async Task<IEnumerable<Cliente>> ObtenerClientesFrecuentesAsync()
    {
        return await _dbSet
            .Include(c => c.TarjetaFidelizacion)
            .Where(c => c.Activo && c.TarjetaFidelizacion != null)
            .OrderByDescending(c => c.TarjetaFidelizacion.PuntosAcumulados)
            .Take(100)
            .ToListAsync();
    }
    
    public async Task<PaginatedList<Cliente>> ObtenerPaginadoAsync(
        int pageNumber, 
        int pageSize, 
        string? filtro = null)
    {
        var query = _dbSet.Where(c => c.Activo);
        
        if (!string.IsNullOrEmpty(filtro))
        {
            query = query.Where(c => 
                c.Nombre.NombreCompleto.Contains(filtro) ||
                c.Email.Valor.Contains(filtro) ||
                c.Telefono.Numero.Contains(filtro));
        }
        
        var count = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return new PaginatedList<Cliente>(items, count, pageNumber, pageSize);
    }
}
```

## 🔄 **Unit of Work Pattern**

```csharp
// Base/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly RestauranteProDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;
    
    // Repositorios por contexto
    private IProductoRepository? _productoRepository;
    private IUsuarioRepository? _usuarioRepository;
    private IClienteRepository? _clienteRepository;
    private IComandaRepository? _comandaRepository;
    private IIngredienteRepository? _ingredienteRepository;
    private IProveedorRepository? _proveedorRepository;
    
    public UnitOfWork(RestauranteProDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    // Propiedades lazy para repositorios por contexto
    public IProductoRepository Productos => 
        _productoRepository ??= new ProductoRepository(_context, _logger.CreateLogger<ProductoRepository>());
        
    public IUsuarioRepository Usuarios => 
        _usuarioRepository ??= new UsuarioRepository(_context, _logger.CreateLogger<UsuarioRepository>());
        
    public IClienteRepository Clientes => 
        _clienteRepository ??= new ClienteRepository(_context, _logger.CreateLogger<ClienteRepository>());
        
    public IComandaRepository Comandas => 
        _comandaRepository ??= new ComandaRepository(_context, _logger.CreateLogger<ComandaRepository>());
        
    public IIngredienteRepository Ingredientes => 
        _ingredienteRepository ??= new IngredienteRepository(_context, _logger.CreateLogger<IngredienteRepository>());
        
    public IProveedorRepository Proveedores => 
        _proveedorRepository ??= new ProveedorRepository(_context, _logger.CreateLogger<ProveedorRepository>());
    
    public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var resultado = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Se guardaron {Count} cambios en la base de datos", resultado);
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar cambios en la base de datos");
            throw;
        }
    }
    
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Ya existe una transacción activa");
        }
        
        _currentTransaction = await _context.Database.BeginTransactionAsync();
        _logger.LogInformation("Transacción iniciada: {TransactionId}", _currentTransaction.TransactionId);
        return _currentTransaction;
    }
    
    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No hay transacción activa para confirmar");
        }
        
        try
        {
            await _currentTransaction.CommitAsync();
            _logger.LogInformation("Transacción confirmada: {TransactionId}", _currentTransaction.TransactionId);
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            return;
        }
        
        try
        {
            await _currentTransaction.RollbackAsync();
            _logger.LogInformation("Transacción revertida: {TransactionId}", _currentTransaction.TransactionId);
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
    
    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
```

## 🔧 **Registro de Dependencias por Contexto**

```csharp
// InfrastructureServiceCollectionExtensions.cs
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuración de base de datos
        services.AddPersistenceServices(configuration);
        
        // Servicios de identidad
        services.AddIdentityServices(configuration);
        
        // Servicios externos
        services.AddExternalServices(configuration);
        
        // Sistema de caché
        services.AddCachingServices(configuration);
        
        // Logging
        services.AddLoggingServices(configuration);
        
        // Background tasks
        services.AddBackgroundTasksServices(configuration);
        
        // Monitoring
        services.AddMonitoringServices(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<RestauranteProDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(RestauranteProDbContext).Assembly.FullName)));
        
        // Repositorios por contexto - Core
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        services.AddScoped<IRecetaRepository, RecetaRepository>();
        
        // Repositorios por contexto - Comercial
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IFacturaRepository, FacturaRepository>();
        services.AddScoped<ITarjetaFidelizacionRepository, TarjetaFidelizacionRepository>();
        
        // Repositorios por contexto - Operaciones
        services.AddScoped<IComandaRepository, ComandaRepository>();
        services.AddScoped<IReservacionRepository, ReservacionRepository>();
        services.AddScoped<IMesaRepository, MesaRepository>();
        services.AddScoped<IPreparacionDiariaRepository, PreparacionDiariaRepository>();
        
        // Repositorios por contexto - Inventario
        services.AddScoped<IIngredienteRepository, IngredienteRepository>();
        services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
        services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
        
        // Repositorios por contexto - Proveedores
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<IContactoProveedorRepository, ContactoProveedorRepository>();
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}
```

## 🎯 **Beneficios de esta Arquitectura**

### **✅ Ventajas Organizacionales**
- **Consistencia** con la estructura de Domain y Application
- **Separación clara** de responsabilidades por contexto
- **Escalabilidad** horizontal por módulos de negocio
- **Mantenibilidad** mejorada con código organizado

### **✅ Ventajas Técnicas**
- **Entity Framework Core** optimizado por contexto
- **Repositorios especializados** para cada dominio
- **Unit of Work** transaccional robusto
- **Configuraciones específicas** por entidad
- **Migraciones organizadas** por contexto

### **✅ Ventajas de Rendimiento**
- **Consultas optimizadas** específicas por contexto
- **Índices específicos** para cada dominio
- **Caché distribuido** por módulos
- **Lazy loading inteligente** según el contexto

## 🚀 **Próximos Pasos de Implementación**

1. **📁 Reestructurar carpetas** según la nueva organización
2. **🗄️ Configurar Entity Framework** por contextos
3. **📊 Implementar configuraciones** de entidades específicas
4. **🏪 Desarrollar repositorios** especializados por contexto
5. **🔄 Implementar Unit of Work** con transacciones
6. **⚡ Configurar caché distribuido** y optimizaciones
7. **📈 Implementar monitoreo** y health checks
8. **🔧 Configurar servicios externos** e integraciones 