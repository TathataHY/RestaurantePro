# Capa de Infraestructura - RestaurantePro

Esta capa implementa los detalles técnicos y proporciona implementaciones concretas para las abstracciones definidas en la capa de aplicación.

## Estructura de Módulos Principales

```
Infrastructure/
├── Persistence/             # Persistencia de datos
│   ├── Contexts/            # Contextos de EF Core
│   ├── Migrations/          # Migraciones de base de datos
│   ├── Configuration/       # Configuración de entidades
│   ├── Interceptors/        # Interceptores para auditoría, etc.
│   └── Repositories/        # Implementaciones de repositorios
│       ├── Comercial/       # Repositorios para el módulo Comercial
│       ├── Operaciones/     # Repositorios para el módulo Operaciones
│       ├── Inventario/      # Repositorios para el módulo Inventario
│       ├── Catalogo/        # Repositorios para el módulo Catálogo
│       └── Finanzas/        # Repositorios para el módulo Finanzas
│
├── Identity/                # Autenticación y autorización
│   ├── Models/              # Modelos para identidad
│   ├── Services/            # Servicios de identidad
│   └── Configuration/       # Configuración de identidad
│
├── ExternalServices/        # Integración con servicios externos
│   ├── Payment/             # Servicios de pago (ej. PayPal, Stripe)
│   ├── Email/               # Servicios de correo electrónico
│   ├── SMS/                 # Servicios de mensajería SMS
│   └── FileStorage/         # Almacenamiento de archivos
│
├── BackgroundTasks/         # Tareas en segundo plano
│   ├── Jobs/                # Definición de trabajos
│   ├── Workers/             # Workers para procesamiento
│   └── Schedulers/          # Planificadores de tareas
│
├── Logging/                 # Configuración de logging
│   ├── Providers/           # Proveedores de logging
│   └── Enrichers/           # Enriquecedores de logs
│
├── DependencyInjection/     # Registro de servicios
│   ├── PersistenceSetup.cs  # Configuración de persistencia
│   ├── IdentitySetup.cs     # Configuración de identidad
│   └── InfrastructureSetup.cs # Configuración general
│
└── Common/                  # Componentes comunes
    ├── Extensions/          # Extensiones útiles
    ├── Helpers/             # Clases auxiliares
    └── Constants/           # Constantes de infraestructura
```

## Implementación de Repositorios

Los repositorios siguen la misma estructura modular que la capa de aplicación:

```
Repositories/
├── Comercial/
│   ├── ClienteRepository.cs
│   ├── PromocionRepository.cs
│   └── FidelizacionRepository.cs
│
├── Operaciones/
│   ├── ComandaRepository.cs
│   ├── MesaRepository.cs
│   └── ReservacionRepository.cs
│
├── Base/
│   ├── RepositoryBase.cs
│   └── UnitOfWork.cs
```

## Configuración de Entity Framework Core

```csharp
// RestauranteProDbContext.cs
public class RestauranteProDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    
    public RestauranteProDbContext(
        DbContextOptions<RestauranteProDbContext> options,
        ICurrentUserService currentUserService,
        IDateTime dateTime) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }
    
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Comanda> Comandas { get; set; }
    public DbSet<Mesa> Mesas { get; set; }
    // Otros DbSets...
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Aplicar todas las configuraciones
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Aplicar auditoría automática
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreadoPor = _currentUserService.UserId;
                    entry.Entity.FechaCreacion = _dateTime.Now;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModificadoPor = _currentUserService.UserId;
                    entry.Entity.FechaModificacion = _dateTime.Now;
                    break;
            }
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

## Ejemplo de Implementación de Repositorio

```csharp
// ClienteRepository.cs
public class ClienteRepository : IClienteRepository
{
    private readonly RestauranteProDbContext _context;
    
    public ClienteRepository(RestauranteProDbContext context)
    {
        _context = context;
    }
    
    public async Task<Cliente> GetByIdAsync(int id)
    {
        return await _context.Clientes.FindAsync(id);
    }
    
    public async Task<IEnumerable<Cliente>> GetAllAsync()
    {
        return await _context.Clientes.ToListAsync();
    }
    
    public async Task<IEnumerable<Cliente>> GetActivosAsync()
    {
        return await _context.Clientes
            .Where(c => c.Activo)
            .ToListAsync();
    }
    
    public async Task<Cliente> GetByEmailAsync(string email)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Email == email);
    }
    
    public async Task<Cliente> AddAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
        return cliente;
    }
    
    public async Task UpdateAsync(Cliente cliente)
    {
        _context.Entry(cliente).State = EntityState.Modified;
    }
    
    public async Task DeleteAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            cliente.Activo = false;
            _context.Entry(cliente).State = EntityState.Modified;
        }
    }
}
```

## Implementación del Unit of Work

```csharp
// UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly RestauranteProDbContext _context;
    private IDbContextTransaction _transaction;
    
    public UnitOfWork(RestauranteProDbContext context)
    {
        _context = context;
    }
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        try
        {
            await _transaction?.CommitAsync();
        }
        finally
        {
            await _transaction?.DisposeAsync();
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        try
        {
            await _transaction?.RollbackAsync();
        }
        finally
        {
            await _transaction?.DisposeAsync();
        }
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
    public void Dispose()
    {
        _context.Dispose();
        _transaction?.Dispose();
    }
}
```

## Registro de Dependencias

```csharp
// InfrastructureServiceCollectionExtensions.cs
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar base de datos
        services.AddDbContext<RestauranteProDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(RestauranteProDbContext).Assembly.FullName)));
                
        // Registrar repositorios
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IComandaRepository, ComandaRepository>();
        services.AddScoped<IMesaRepository, MesaRepository>();
        // Otros repositorios...
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Servicios externos
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<ISmsService, SmsService>();
        
        // Servicios de identidad y otros servicios
        
        return services;
    }
}
``` 