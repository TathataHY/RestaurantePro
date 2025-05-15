# Arquitectura de la API - RestaurantePro

## Visión General

La API de RestaurantePro está diseñada como una API RESTful moderna utilizando ASP.NET Core 8, siguiendo principios de Clean Architecture y CQRS (Command Query Responsibility Segregation). Es el núcleo del sistema que conecta la aplicación móvil de comandas con el portal web de analítica.

## Componentes Clave

### Controllers

Los controladores son ligeros y se limitan a:
- Recibir las solicitudes HTTP
- Validar el modelo de entrada básico
- Delegar la lógica de negocio a los handlers de comandos/queries
- Devolver respuestas HTTP apropiadas

```csharp
[ApiController]
[Route("api/[controller]")]
public class ComandasController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComandasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ComandaDto>>> GetComandas()
    {
        var query = new GetComandasQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateComanda([FromBody] CreateComandaCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetComanda), new { id = result }, result);
    }

    // Otros endpoints...
}
```

### Middleware

Cadena de middleware configurada para:
- Manejo global de excepciones
- Logging de solicitudes y respuestas
- Compresión de respuestas
- Autenticación y autorización
- CORS (Cross-Origin Resource Sharing)
- Validaciones de entrada

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Middleware de manejo de excepciones
    app.UseExceptionHandler("/error");
    
    // Middleware de seguridad
    app.UseHttpsRedirection();
    app.UseHsts();
    
    // Middleware de autenticación y autorización
    app.UseAuthentication();
    app.UseAuthorization();
    
    // Otros middleware...
    app.UseRouting();
    app.UseCors("AllowedOrigins");
    
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHub<ComandaHub>("/hubs/comandas");
    });
}
```

### SignalR Hubs

Hubs para comunicación en tiempo real:
- `ComandaHub`: Notificaciones sobre comandas entre meseros y cocina
- `InventarioHub`: Alertas de stock bajo para administradores

```csharp
public class ComandaHub : Hub
{
    public async Task NuevaComanda(ComandaDto comanda)
    {
        await Clients.Group("Cocina").SendAsync("RecibirNuevaComanda", comanda);
    }
    
    public async Task ActualizarEstadoComanda(int comandaId, string estado)
    {
        await Clients.All.SendAsync("ComandaActualizada", comandaId, estado);
    }
    
    public async Task JoinGroup(string group)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
    }
}
```

## Flujo de Solicitudes

1. **Solicitud HTTP entrante**
   - Pasa por middleware de autenticación/autorización
   - Se validan encabezados y parámetros básicos

2. **Controlador API**
   - Recibe la solicitud
   - Mapea a un comando/consulta
   - Envía a MediatR

3. **Validador**
   - Valida las reglas de negocio del comando/consulta
   - Rechaza solicitudes inválidas

4. **Handler**
   - Ejecuta la lógica de negocio
   - Interactúa con el repositorio/unidad de trabajo
   - Devuelve resultado

5. **Controlador API**
   - Formatea la respuesta HTTP
   - Devuelve los datos o confirmación al cliente

## Seguridad

### Autenticación

- JWT (JSON Web Tokens) para autenticación sin estado
- Configuración de Identity para gestión de usuarios

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Configuration["JwtSettings:Issuer"],
            ValidAudience = Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Configuration["JwtSettings:Key"]))
        };
    });
```

### Autorización

- Políticas basadas en roles (Admin, Mesero, Cocinero)
- Autorización a nivel de recurso para comandas y mesas

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("MeseroCocinero", policy => 
        policy.RequireRole("Mesero", "Cocinero"));
});
```

## Documentación API

- Integración de Swagger/OpenAPI para documentación automática
- Ejemplos de solicitudes y respuestas
- Autenticación en Swagger UI

```csharp
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "RestaurantePro API", 
        Version = "v1",
        Description = "API para el sistema de gestión de restaurantes RestaurantePro"
    });
    
    // Configuración de seguridad para Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "Bearer" 
                }
            },
            new string[] {}
        }
    });
});
```

## Gestión de Datos

- Uso del patrón Unit of Work para mantener la consistencia transaccional
- Consultas optimizadas con Dapper para operaciones de sólo lectura
- Entity Framework Core para operaciones de escritura

```csharp
public class ComandaQueryService : IComandaQueryService
{
    private readonly string _connectionString;
    
    public ComandaQueryService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public async Task<IEnumerable<ComandaListDto>> GetComandasByMesaAsync(int mesaId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<ComandaListDto>(
            @"SELECT c.Id, c.FechaHora, c.Estado, c.Total, u.Nombre + ' ' + u.Apellido as Mesero
              FROM Comandas c
              INNER JOIN Usuarios u ON c.MeseroId = u.Id
              WHERE c.MesaId = @MesaId
              ORDER BY c.FechaHora DESC",
            new { MesaId = mesaId });
    }
}
```

## Caché

- Implementación de caché distribuida para mejorar el rendimiento
- Invalidación selectiva de caché en operaciones de escritura

```csharp
public async Task<IEnumerable<ProductoDto>> GetProductosAsync()
{
    var cacheKey = "productos_all";
    
    // Intentar obtener del caché
    var cached = await _cache.GetStringAsync(cacheKey);
    if (!string.IsNullOrEmpty(cached))
    {
        return JsonSerializer.Deserialize<IEnumerable<ProductoDto>>(cached);
    }
    
    // Si no está en caché, obtener de la base de datos
    var productos = await _productoRepository.GetAllAsync();
    var productosDto = _mapper.Map<IEnumerable<ProductoDto>>(productos);
    
    // Guardar en caché por 10 minutos
    var cacheOptions = new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };
    
    await _cache.SetStringAsync(
        cacheKey, 
        JsonSerializer.Serialize(productosDto),
        cacheOptions);
    
    return productosDto;
}
```

## Manejo de Errores

- Middleware personalizado para capturar y registrar excepciones
- Respuestas de error estandarizadas
- Logging detallado para facilitar la solución de problemas

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ApiErrorResponse();
        
        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Title = "Validation error";
                response.Status = StatusCodes.Status400BadRequest;
                response.Detail = "One or more validation errors occurred";
                response.Errors = validationEx.Errors;
                break;
                
            case NotFoundException notFoundEx:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Title = "Resource not found";
                response.Status = StatusCodes.Status404NotFound;
                response.Detail = notFoundEx.Message;
                break;
                
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Title = "Server error";
                response.Status = StatusCodes.Status500InternalServerError;
                response.Detail = "An internal server error has occurred";
                break;
        }
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

## Monitoreo y Logging

- Integración con Application Insights o Prometheus para monitoreo
- Logging estructurado con Serilog
- Trazas de rendimiento para endpoints críticos

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/restaurantepro-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
```

## Escalabilidad

- Diseño sin estado para permitir escalabilidad horizontal
- Uso de Redis para caché distribuida y gestión de sesiones

## Consideraciones de Despliegue

- Configuración para contenedores Docker
- Configuración para Azure App Service
- Variables de entorno y configuración por ambiente

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RestaurantePro.Api/RestaurantePro.Api.csproj", "RestaurantePro.Api/"]
COPY ["RestaurantePro.Application/RestaurantePro.Application.csproj", "RestaurantePro.Application/"]
COPY ["RestaurantePro.Domain/RestaurantePro.Domain.csproj", "RestaurantePro.Domain/"]
COPY ["RestaurantePro.Infrastructure/RestaurantePro.Infrastructure.csproj", "RestaurantePro.Infrastructure/"]
RUN dotnet restore "RestaurantePro.Api/RestaurantePro.Api.csproj"
COPY . .
WORKDIR "/src/RestaurantePro.Api"
RUN dotnet build "RestaurantePro.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RestaurantePro.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestaurantePro.Api.dll"]
``` 