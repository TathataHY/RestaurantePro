# Capa de API - RestaurantePro

Esta capa expone las funcionalidades del sistema a través de una Web API RESTful organizada por módulos de negocio.

## Estructura de Módulos Principales

```
Api/
├── Controllers/               # Controladores organizados por módulos
│   ├── Comercial/             # Controladores del módulo Comercial
│   │   ├── ClientesController.cs
│   │   ├── PromocionesController.cs
│   │   └── FidelizacionController.cs
│   │
│   ├── Operaciones/           # Controladores del módulo Operaciones
│   │   ├── ComandasController.cs
│   │   ├── MesasController.cs
│   │   └── ReservacionesController.cs
│   │
│   ├── Inventario/            # Controladores del módulo Inventario
│   │   ├── ProductosController.cs
│   │   ├── MovimientosController.cs
│   │   └── ComprasController.cs
│   │
│   ├── Catalogo/              # Controladores del módulo Catálogo
│   │   ├── ProductosController.cs
│   │   ├── CategoriasController.cs
│   │   └── IngredientesController.cs
│   │
│   └── Finanzas/              # Controladores del módulo Finanzas
│       ├── PagosController.cs
│       ├── FacturacionController.cs
│       └── ContabilidadController.cs
│
├── Middleware/                # Middleware personalizado
│   ├── ExceptionMiddleware.cs # Manejo global de excepciones
│   ├── RequestLoggingMiddleware.cs
│   └── ApiKeyMiddleware.cs
│
├── Filters/                   # Filtros de acción y autorización
│   ├── ApiExceptionFilterAttribute.cs
│   ├── ValidationFilterAttribute.cs
│   └── ApiKeyAuthAttribute.cs
│
├── Models/                    # Modelos específicos de la API
│   ├── Requests/              # Modelos de solicitud
│   ├── Responses/             # Modelos de respuesta
│   └── Validation/            # Validadores de modelos
│
├── Extensions/                # Extensiones para configuración
│   ├── ApiServicesExtensions.cs
│   ├── SwaggerExtensions.cs
│   └── MiddlewareExtensions.cs
│
├── Common/                    # Componentes comunes de API
│   ├── ApiResponse.cs         # Envoltura de respuesta estándar
│   ├── PaginatedList.cs       # Modelo para paginación
│   └── SortingOptions.cs      # Opciones de ordenamiento
│
└── Configuration/             # Configuración de la API
    ├── AuthorizationConfig.cs
    ├── CorsConfig.cs
    └── SwaggerConfig.cs
```

## Controladores API

Los controladores siguen la estructura modular, mapeando a los mismos módulos de negocio que las capas de aplicación e infraestructura:

```csharp
// ClientesController.cs
[ApiController]
[Route("api/comercial/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClienteDto>>> GetAll()
    {
        var query = new GetClientesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id)
    {
        var query = new GetClienteByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateClienteCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateClienteCommand command)
    {
        if (id != command.Id)
            return BadRequest();
            
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteClienteCommand { Id = id });
        return NoContent();
    }
}
```

## Respuesta Estándar

Todas las respuestas de la API utilizan un formato estándar para consistencia:

```csharp
// ApiResponse.cs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; }
    
    // Constructores para éxito y error
    public static ApiResponse<T> SuccessResponse(T data, string message = null)
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

## Manejo de Excepciones

Un middleware centralizado maneja todas las excepciones, proporcionando respuestas consistentes:

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
            _logger.LogError(ex, "Error no manejado");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ValidationException validationEx => 
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

## Configuración de Startup

La configuración mantiene una estructura modular, donde cada componente es configurado de manera independiente:

```csharp
// Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Agregar servicios al contenedor
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // Configuración específica para la API
        builder.Services.AddApiServices();
        builder.Services.AddSwaggerServices();
        
        // Agregar capas inferiores
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);
        
        var app = builder.Build();
        
        // Configurar el pipeline de solicitudes HTTP
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        
        app.Run();
    }
}
``` 