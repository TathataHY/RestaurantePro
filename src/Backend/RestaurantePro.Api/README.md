# Capa de API - RestaurantePro

Esta capa expone las funcionalidades del sistema a través de una Web API RESTful organizada por contextos de dominio siguiendo los principios de Clean Architecture.

## 🏗️ **Estructura de Contextos de Dominio**

```
RestaurantePro.Api/
├── Controllers/                    # Controladores organizados por contextos
│   ├── Core/                      # Contexto Core - Funcionalidades centrales
│   │   ├── ProductosController.cs        # ✅ Gestión de productos del menú
│   │   ├── UsuariosController.cs         # 🔄 Gestión de usuarios del sistema
│   │   ├── NotificacionesController.cs   # 🔄 Sistema de notificaciones
│   │   └── RecetasController.cs          # ⬜ Gestión de recetas (próximo)
│   │
│   ├── Comercial/                 # Contexto Comercial - Ventas y clientes
│   │   ├── ClientesController.cs         # 🔄 Gestión de clientes
│   │   ├── FacturasController.cs         # 🔄 Facturación y fiscalización
│   │   ├── TarjetasFidelizacionController.cs # ⬜ Programa de fidelización
│   │   ├── PromocionesController.cs      # 🔄 Sistema de promociones
│   │   └── ReportesComercialController.cs # 🔄 Reportes comerciales
│   │
│   ├── Operaciones/               # Contexto Operaciones - Restaurante
│   │   ├── ComandasController.cs         # 🔄 Gestión de comandas/pedidos
│   │   ├── ReservacionesController.cs    # 🔄 Sistema de reservaciones
│   │   ├── MesasController.cs            # 🔄 Gestión de mesas
│   │   ├── PreparacionesController.cs    # 🔄 Control de cocina
│   │   └── ReportesOperacionesController.cs # 🔄 Reportes operativos
│   │
│   ├── Inventario/                # Contexto Inventario - Gestión de stock
│   │   ├── IngredientesController.cs     # 🔄 Gestión de ingredientes
│   │   ├── OrdenesCompraController.cs    # 🔄 Órdenes de compra
│   │   ├── MovimientosInventarioController.cs # ⬜ Control de movimientos
│   │   └── ReportesInventarioController.cs # 🔄 Reportes de inventario
│   │
│   └── Proveedores/               # Contexto Proveedores - Gestión externa
│       ├── ProveedoresController.cs      # 🔄 Gestión de proveedores
│       ├── ContactosProveedorController.cs # ⬜ Contactos de proveedores
│       └── EvaluacionesProveedorController.cs # ⬜ Evaluación de proveedores
│
├── Middleware/                    # Middleware personalizado
│   └── ExceptionMiddleware.cs     # ✅ Manejo global de excepciones
│
├── Filters/                       # Filtros de acción y autorización
│   └── ApiExceptionFilterAttribute.cs    # ✅ Filtro de excepciones API
│
├── Extensions/                    # Extensiones para configuración
│   ├── ApiServicesExtensions.cs  # ✅ Configuración de servicios API
│   ├── AuthorizationExtensions.cs # ✅ Configuración de autorización
│   ├── SwaggerExtensions.cs       # ⬜ Configuración de Swagger (pendiente)
│   ├── MiddlewareExtensions.cs    # ⬜ Extensiones de middleware (pendiente)
│   └── SeedDataExtensions.cs     # ✅ Configuración de datos semilla
│
├── Models/                        # ⬜ Modelos específicos de la API (pendiente)
│   ├── Requests/                  # ⬜ Modelos de solicitud personalizados
│   ├── Responses/                 # ⬜ Modelos de respuesta personalizados
│   └── Validation/                # ⬜ Validadores específicos de API
│
├── Common/                        # Componentes comunes de API
│   ├── ApiResponse.cs             # ✅ Envoltura de respuesta estándar
│   ├── PaginatedList.cs           # ⬜ Modelo para paginación (pendiente)
│   └── SortingOptions.cs          # ⬜ Opciones de ordenamiento (pendiente)
│
├── Configuration/                 # ⬜ Configuración avanzada de la API (pendiente)
│   ├── AuthorizationConfig.cs     # ⬜ Configuración de autorización avanzada
│   ├── CorsConfig.cs              # ⬜ Configuración de CORS
│   └── SwaggerConfig.cs           # ⬜ Configuración detallada de Swagger
│
├── GlobalUsings.cs                # ✅ Importaciones globales
├── Program.cs                     # ✅ Configuración principal
└── README.md                      # 📖 Este archivo
```

## 📊 **Estado de Implementación**

### **Leyenda de Estado:**
- **✅**: Completamente implementado y funcional
- **🔄**: Implementado pero en proceso de corrección/mejora
- **⬜**: Pendiente de implementación

### **Contexto Core - Funcionalidades Centrales**
| Controlador | Estado | Descripción | Endpoints |
|-------------|--------|-------------|-----------|
| **ProductosController** | ✅ | Gestión completa de productos del menú | GET, POST, PUT, DELETE |
| **UsuariosController** | 🔄 | Gestión de usuarios del sistema | En corrección |
| **NotificacionesController** | 🔄 | Sistema de notificaciones | En corrección |
| **RecetasController** | ⬜ | Gestión de recetas | Pendiente |

### **Contexto Comercial - Ventas y Clientes**
| Controlador | Estado | Descripción | Endpoints |
|-------------|--------|-------------|-----------|
| **ClientesController** | 🔄 | Gestión de clientes | En corrección |
| **FacturasController** | 🔄 | Facturación y fiscalización | En corrección |
| **TarjetasFidelizacionController** | ⬜ | Programa de fidelización | Pendiente |
| **PromocionesController** | 🔄 | Sistema de promociones | En corrección |
| **ReportesComercialController** | 🔄 | Reportes comerciales | En corrección |

### **Contexto Operaciones - Restaurante**
| Controlador | Estado | Descripción | Endpoints |
|-------------|--------|-------------|-----------|
| **ComandasController** | 🔄 | Gestión de comandas/pedidos | En corrección |
| **ReservacionesController** | 🔄 | Sistema de reservaciones | En corrección |
| **MesasController** | 🔄 | Gestión de mesas | En corrección |
| **PreparacionesController** | 🔄 | Control de cocina | En corrección |
| **ReportesOperacionesController** | 🔄 | Reportes operativos | En corrección |

### **Contexto Inventario - Gestión de Stock**
| Controlador | Estado | Descripción | Endpoints |
|-------------|--------|-------------|-----------|
| **IngredientesController** | 🔄 | Gestión de ingredientes | En corrección |
| **OrdenesCompraController** | 🔄 | Órdenes de compra | En corrección |
| **MovimientosInventarioController** | ⬜ | Control de movimientos | Pendiente |
| **ReportesInventarioController** | 🔄 | Reportes de inventario | En corrección |

### **Contexto Proveedores - Gestión Externa**
| Controlador | Estado | Descripción | Endpoints |
|-------------|--------|-------------|-----------|
| **ProveedoresController** | 🔄 | Gestión de proveedores | En corrección |
| **ContactosProveedorController** | ⬜ | Contactos de proveedores | Pendiente |
| **EvaluacionesProveedorController** | ⬜ | Evaluación de proveedores | Pendiente |

## 🎯 **Ejemplo de Controlador Implementado**

```csharp
// ProductosController.cs - Ejemplo completo funcional
[ApiController]
[Route("api/core/productos")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(IMediator mediator, ILogger<ProductosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los productos con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ProductoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ProductoDto>>>> GetProductos(
        [FromQuery] ObtenerProductosPaginadosQuery query)
    {
        _logger.LogInformation("📋 GET /api/core/productos");
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            var errorResponse = ApiResponse<PaginatedList<ProductoDto>>.ErrorResponse(
                result.Errors, "Error al obtener productos", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<PaginatedList<ProductoDto>>.SuccessResponse(
            result.Value, "Productos obtenidos exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Obtiene un producto específico por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> GetProducto(Guid id)
    {
        _logger.LogInformation("🔍 GET /api/core/productos/{Id}", id);
        
        var query = new ObtenerProductoPorIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Producto no encontrado", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<ProductoDto>.SuccessResponse(
            result.Value, "Producto obtenido exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> CrearProducto(
        [FromBody] CrearProductoCommand command)
    {
        _logger.LogInformation("➕ POST /api/core/productos");
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al crear producto", StatusCodes.Status400BadRequest);
            return BadRequest(errorResponse);
        }

        var response = ApiResponse<ProductoDto>.SuccessResponse(
            result.Value, "Producto creado exitosamente");
            
        return CreatedAtAction(
            nameof(GetProducto),
            new { id = result.Value.Id },
            response);
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> ActualizarProducto(
        Guid id, [FromBody] ActualizarProductoCommand command)
    {
        _logger.LogInformation("✏️ PUT /api/core/productos/{Id}", id);

        // Asignar el ID de la URL al comando
        command.Id = id;

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            var statusCode = result.Errors.Any(e => e.Contains("no encontrado")) 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;
                
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al actualizar producto", statusCode);
            return StatusCode(statusCode, errorResponse);
        }

        var response = ApiResponse<ProductoDto>.SuccessResponse(
            result.Value, "Producto actualizado exitosamente");
        return Ok(response);
    }

    /// <summary>
    /// Elimina un producto
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarProducto(Guid id)
    {
        _logger.LogInformation("🗑️ DELETE /api/core/productos/{Id}", id);

        var command = new EliminarProductoCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Errors, "Error al eliminar producto", StatusCodes.Status404NotFound);
            return NotFound(errorResponse);
        }

        var response = ApiResponse<bool>.SuccessResponse(
            result.Value, "Producto eliminado exitosamente");
        return Ok(response);
    }
}
```

## 🔧 **Respuesta Estándar de la API**

Todas las respuestas utilizan el wrapper `ApiResponse<T>` para consistencia:

```csharp
// ApiResponse.cs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; }
    
    public static ApiResponse<T> SuccessResponse(T data, string message = "")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = 200
        };
    }
    
    public static ApiResponse<T> ErrorResponse(List<string> errors, string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors,
            Message = message,
            StatusCode = statusCode
        };
    }
}
```

## 🛡️ **Manejo de Excepciones**

Middleware centralizado que captura y formatea todas las excepciones:

```csharp
// ExceptionMiddleware.cs
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            Application.Common.Exceptions.ValidationException validationEx => 
                ApiResponse<object>.ErrorResponse(
                    validationEx.Errors.Select(e => e.ErrorMessage).ToList(), 
                    "Error de validación", 
                    StatusCodes.Status400BadRequest),
                    
            NotFoundException notFoundEx => 
                ApiResponse<object>.ErrorResponse(
                    new List<string> { notFoundEx.Message }, 
                    "Recurso no encontrado", 
                    StatusCodes.Status404NotFound),
                    
            UnauthorizedAccessException => 
                ApiResponse<object>.ErrorResponse(
                    new List<string> { "No autorizado" }, 
                    "Acceso denegado", 
                    StatusCodes.Status401Unauthorized),
                    
            _ => ApiResponse<object>.ErrorResponse(
                    new List<string> { "Ocurrió un error interno" }, 
                    "Error del servidor", 
                    StatusCodes.Status500InternalServerError)
        };
        
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

## ⚙️ **Configuración de la API**

```csharp
// Program.cs
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Agregar servicios al contenedor
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // Configuración específica para la API
        builder.Services.AddApiServices();
        
        // Agregar capas inferiores
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices(builder.Configuration);
        
        var app = builder.Build();
        
        // Configurar el pipeline de solicitudes HTTP
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        // Middleware global para manejo de excepciones
        app.UseMiddleware<ExceptionMiddleware>();
        
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        
        await app.RunAsync();
    }
}
```

## 🗂️ **Componentes Adicionales Planificados**

### **Models/ - Modelos Específicos de API**
Modelos que no pertenecen directamente a ningún contexto de dominio pero son útiles para la API:

```csharp
// Models/Requests/PaginationRequest.cs
public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
}

// Models/Responses/ErrorResponse.cs
public class ErrorResponse
{
    public string Code { get; set; }
    public string Message { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}

// Models/Validation/GuidNotEmptyAttribute.cs
public class GuidNotEmptyAttribute : ValidationAttribute
{
    // Validador personalizado para Guids
}
```

### **Common/ - Componentes Compartidos**
```csharp
// Common/PaginatedList.cs - PENDIENTE
public class PaginatedList<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

// Common/SortingOptions.cs - PENDIENTE
public class SortingOptions
{
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";
    public bool IsValidDirection => SortDirection?.ToLower() is "asc" or "desc";
}
```

### **Configuration/ - Configuraciones Avanzadas**
```csharp
// Configuration/SwaggerConfig.cs - PENDIENTE
public static class SwaggerConfig
{
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "RestaurantePro API", 
                Version = "v1",
                Description = "API para gestión integral de restaurantes"
            });
            
            // Configuración de autenticación JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
        });
    }
}

// Configuration/CorsConfig.cs - PENDIENTE
public static class CorsConfig
{
    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("http://localhost:3000", "https://restaurantepro.com")
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });
    }
}
```

## 🚀 **Guía de Desarrollo**

### **Crear un Nuevo Controlador**

1. **Ubicación**: Colocar en la carpeta del contexto correspondiente
2. **Convención de naming**: `{Entidad}Controller.cs`
3. **Estructura estándar**: Seguir el patrón del `ProductosController`
4. **Testing**: Crear tests de integración correspondientes

### **Convenciones de Rutas**
- **Core**: `/api/core/{entidad}`
- **Comercial**: `/api/comercial/{entidad}`
- **Operaciones**: `/api/operaciones/{entidad}`
- **Inventario**: `/api/inventario/{entidad}`
- **Proveedores**: `/api/proveedores/{entidad}`

### **Convenciones de Respuesta**
- **GET**: `200 OK` con datos o `404 Not Found`
- **POST**: `201 Created` con ubicación o `400 Bad Request`
- **PUT**: `200 OK` con datos o `404 Not Found`
- **DELETE**: `200 OK` con confirmación o `404 Not Found`

## 📈 **Métricas de Implementación**

### **Estado Actual**
- **Total Controladores Planificados**: 22
- **Implementados**: 17 (🔄 en corrección)
- **Completamente Funcionales**: 1 (✅ ProductosController)
- **Pendientes**: 4
- **Progreso General**: 77% implementado, 5% funcional

### **Próximos Pasos**
1. **Corregir errores de compilación** en controladores existentes (12 errores restantes)
2. **Implementar componentes Common** (PaginatedList, SortingOptions)
3. **Crear configuraciones avanzadas** (Swagger, CORS, Validación)
4. **Completar controladores pendientes** por contexto
5. **Implementar tests de integración** para controladores funcionales
6. **Optimizar rendimiento** y agregar caché
7. **Implementar autenticación y autorización** completa

### **Compatibilidad con Tests de Integración**

Esta estructura de API está diseñada para trabajar perfectamente con los tests de integración definidos en:
`tests/RestaurantePro.Api.IntegrationTests/README.md`

**Mapeo de Estructura API ↔ Tests:**
```
API Controllers/                    ←→ Tests Controllers/
├── Core/                          ←→ ├── Core/
│   └── ProductosController.cs     ←→ │   └── ProductosControllerTests.cs
├── Comercial/                     ←→ ├── Comercial/
│   └── ClientesController.cs      ←→ │   └── ClientesControllerTests.cs
├── Operaciones/                   ←→ ├── Operaciones/
│   └── ComandasController.cs      ←→ │   └── ComandasControllerTests.cs
├── Inventario/                    ←→ ├── Inventario/
│   └── IngredientesController.cs  ←→ │   └── IngredientesControllerTests.cs
└── Proveedores/                   ←→ └── Proveedores/
    └── ProveedoresController.cs   ←→     └── ProveedoresControllerTests.cs

API Middleware/                     ←→ Tests Middleware/
└── ExceptionMiddleware.cs         ←→ └── ExceptionMiddlewareTests.cs

API Common/                        ←→ Tests TestBase/
├── ApiResponse.cs                 ←→ ├── ApiIntegrationTestBase.cs
└── PaginatedList.cs               ←→ └── TestWebApplicationFactory.cs
```

**Ventajas de esta Estructura:**
- ✅ **Organización por Contextos**: Facilita el mantenimiento y testing
- ✅ **Separación de Responsabilidades**: Cada contexto tiene su lógica aislada
- ✅ **Testing Paralelo**: Tests organizados igual que los controladores
- ✅ **Escalabilidad**: Fácil agregar nuevos contextos y funcionalidades
- ✅ **Clean Architecture**: Respeta las capas y dependencias

## 🛠️ **Herramientas de Desarrollo**

### **Testing**
- **Tests de Integración**: `RestaurantePro.Api.IntegrationTests`
- **Cobertura**: Objetivo 90%+ en controladores
- **Herramientas**: xUnit, FluentAssertions, WebApplicationFactory

### **Documentación**
- **Swagger**: Configurado para desarrollo
- **XML Comments**: Documentación en código
- **Postman Collection**: Disponible para pruebas manuales

### **Calidad de Código**
- **Análisis Estático**: SonarQube
- **Linting**: EditorConfig + .NET Analyzers
- **Convenciones**: Clean Architecture + DDD

---

## 📞 **Soporte**

Para más información sobre:
- **Tests de API**: Ver `tests/RestaurantePro.Api.IntegrationTests/README.md`
- **Application Layer**: Ver `src/Backend/RestaurantePro.Application/README.md`
- **Domain Logic**: Ver `src/Backend/RestaurantePro.Domain/README.md`
- **Infrastructure**: Ver `src/Backend/RestaurantePro.Infrastructure/README.md` 