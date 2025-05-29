# Capa de Aplicación - RestaurantePro

Esta capa organiza la lógica de aplicación siguiendo la misma estructura de contextos de dominio para mantener la consistencia arquitectónica.

## 🏗️ **Estructura de Contextos (Alineada con Domain)**

```
Application/
├── Core/                     # Contexto Core - Componentes base
│   ├── Productos/            # Gestión del catálogo de productos
│   │   ├── Commands/         # Comandos para productos
│   │   ├── Queries/          # Consultas de productos
│   │   ├── DTOs/             # DTOs de productos
│   │   └── Validators/       # Validadores
│   ├── Usuarios/             # Gestión de usuarios del sistema
│   │   ├── Commands/         # Comandos para usuarios
│   │   ├── Queries/          # Consultas de usuarios
│   │   ├── DTOs/             # DTOs de usuarios
│   │   └── Validators/       # Validadores
│   ├── Notificaciones/       # Sistema central de notificaciones
│   │   ├── Commands/         # Comandos para notificaciones
│   │   ├── Queries/          # Consultas de notificaciones
│   │   ├── DTOs/             # DTOs de notificaciones
│   │   └── Services/         # Servicios de aplicación
│   └── Recetas/              # Gestión de recetas e ingredientes
│       ├── Commands/         # Comandos para recetas
│       ├── Queries/          # Consultas de recetas
│       ├── DTOs/             # DTOs de recetas
│       └── Validators/       # Validadores
│
├── Comercial/                # Contexto Comercial - Clientes y ventas
│   ├── Clientes/             # Gestión de clientes
│   │   ├── Commands/         # Crear, actualizar, desactivar clientes
│   │   ├── Queries/          # Consultas de clientes
│   │   ├── DTOs/             # DTOs de clientes
│   │   └── Validators/       # Validadores de clientes
│   ├── Fidelizacion/         # Programa de fidelización
│   │   ├── Commands/         # Comandos de puntos y descuentos
│   │   ├── Queries/          # Consultas de fidelización
│   │   ├── DTOs/             # DTOs de fidelización
│   │   └── Services/         # Servicios de aplicación
│   └── Facturacion/          # Sistema de facturación
│       ├── Commands/         # Comandos de facturación
│       ├── Queries/          # Consultas de facturas
│       ├── DTOs/             # DTOs de facturación
│       └── Validators/       # Validadores de facturas
│
├── Operaciones/              # Contexto Operaciones - Restaurante diario
│   ├── Comandas/             # Gestión de comandas y pedidos
│   │   ├── Commands/         # Crear, modificar, finalizar comandas
│   │   ├── Queries/          # Consultas de comandas
│   │   ├── DTOs/             # DTOs de comandas
│   │   └── Validators/       # Validadores de comandas
│   ├── Reservaciones/        # Sistema de reservaciones
│   │   ├── Commands/         # Comandos de reservaciones
│   │   ├── Queries/          # Consultas de reservaciones
│   │   ├── DTOs/             # DTOs de reservaciones
│   │   └── Validators/       # Validadores de reservaciones
│   ├── Mesas/                # Gestión de mesas
│   │   ├── Commands/         # Comandos de mesas
│   │   ├── Queries/          # Consultas de mesas
│   │   ├── DTOs/             # DTOs de mesas
│   │   └── Validators/       # Validadores
│   └── Preparaciones/        # 🆕 Preparaciones diarias
│       ├── Commands/         # Comandos de preparaciones
│       ├── Queries/          # Consultas de preparaciones
│       ├── DTOs/             # DTOs de preparaciones
│       └── Services/         # Servicios de aplicación
│
├── Inventario/               # Contexto Inventario - Gestión de stock
│   ├── Ingredientes/         # Gestión de ingredientes
│   │   ├── Commands/         # Comandos de ingredientes
│   │   ├── Queries/          # Consultas de ingredientes
│   │   ├── DTOs/             # DTOs de ingredientes
│   │   └── Validators/       # Validadores
│   ├── MovimientosInventario/ # Movimientos de inventario
│   │   ├── Commands/         # Comandos de movimientos
│   │   ├── Queries/          # Consultas de movimientos
│   │   ├── DTOs/             # DTOs de movimientos
│   │   └── Validators/       # Validadores
│   └── OrdenesCompra/        # Órdenes de compra
│       ├── Commands/         # Comandos de órdenes
│       ├── Queries/          # Consultas de órdenes
│       ├── DTOs/             # DTOs de órdenes
│       └── Validators/       # Validadores
│
├── Proveedores/              # Contexto Proveedores - Gestión de proveedores
│   ├── Proveedores/          # Gestión de proveedores
│   │   ├── Commands/         # Comandos de proveedores
│   │   ├── Queries/          # Consultas de proveedores
│   │   ├── DTOs/             # DTOs de proveedores
│   │   └── Validators/       # Validadores
│   └── ContactosProveedor/   # Contactos de proveedores
│       ├── Commands/         # Comandos de contactos
│       ├── Queries/          # Consultas de contactos
│       ├── DTOs/             # DTOs de contactos
│       └── Validators/       # Validadores
│
├── Common/                   # Componentes compartidos entre contextos
│   ├── Interfaces/           # Interfaces comunes
│   │   ├── IApplicationService.cs
│   │   ├── IQueryHandler.cs
│   │   └── ICommandHandler.cs
│   ├── DTOs/                 # DTOs base y compartidos
│   │   ├── PaginatedList.cs
│   │   ├── FilterRequest.cs
│   │   └── BaseDto.cs
│   ├── Behaviors/            # Comportamientos de MediatR
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   └── CachingBehavior.cs
│   ├── Exceptions/           # Excepciones de aplicación
│   │   ├── ApplicationException.cs
│   │   ├── ValidationException.cs
│   │   └── NotFoundException.cs
│   └── Extensions/           # Extensiones útiles
│       ├── MediatorExtensions.cs
│       └── QueryableExtensions.cs
│
└── Config/                   # Configuración de la aplicación
    ├── Mappings/             # AutoMapper profiles por contexto
    │   ├── CoreMappingProfile.cs
    │   ├── ComercialMappingProfile.cs
    │   ├── OperacionesMappingProfile.cs
    │   ├── InventarioMappingProfile.cs
    │   └── ProveedoresMappingProfile.cs
    ├── DependencyInjection/  # Registro de servicios por contexto
    │   ├── CoreServiceSetup.cs
    │   ├── ComercialServiceSetup.cs
    │   ├── OperacionesServiceSetup.cs
    │   ├── InventarioServiceSetup.cs
    │   └── ProveedoresServiceSetup.cs
    └── Validation/           # Validadores de FluentValidation
        ├── AbstractValidators/
        └── ValidationExtensions.cs
```

## 🔧 **Patrón CQRS con MediatR**

Cada contexto sigue el patrón Command Query Responsibility Segregation:

### **📝 Commands (Operaciones que modifican estado)**
```csharp
// Ejemplo: CrearClienteCommand
public class CrearClienteCommand : IRequest<Result<ClienteDto>>
{
    public string Nombre { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
}

public class CrearClienteCommandHandler : IRequestHandler<CrearClienteCommand, Result<ClienteDto>>
{
    private readonly IClienteRepository _repository;
    private readonly IMapper _mapper;
    
    public async Task<Result<ClienteDto>> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
    {
        // Lógica de creación...
    }
}
```

### **📊 Queries (Operaciones de consulta)**
```csharp
// Ejemplo: ObtenerClientePorIdQuery
public class ObtenerClientePorIdQuery : IRequest<Result<ClienteDto>>
{
    public Guid ClienteId { get; set; }
}

public class ObtenerClientePorIdQueryHandler : IRequestHandler<ObtenerClientePorIdQuery, Result<ClienteDto>>
{
    private readonly IClienteRepository _repository;
    private readonly IMapper _mapper;
    
    public async Task<Result<ClienteDto>> Handle(ObtenerClientePorIdQuery request, CancellationToken cancellationToken)
    {
        // Lógica de consulta...
    }
}
```

## 🧩 **Integración con Patrones de Domain**

### **🏗️ Uso de Builders desde Application**
```csharp
public class CrearComandaCommandHandler : IRequestHandler<CrearComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _repository;
    private readonly ComandaBuilder _comandaBuilder;
    
    public async Task<Result<ComandaDto>> Handle(CrearComandaCommand request, CancellationToken cancellationToken)
    {
        // Usar el builder del dominio
        var resultadoComanda = _comandaBuilder
            .ConMesero(request.MeseroId)
            .ConCliente(request.ClienteId)
            .EnMesa(request.MesaId)
            .ConObservaciones(request.Observaciones)
            .Construir();
            
        if (!resultadoComanda.Succeeded)
            return Result<ComandaDto>.Failure(resultadoComanda.ErrorMessage);
            
        await _repository.AddAsync(resultadoComanda.Value);
        return Result<ComandaDto>.Success(_mapper.Map<ComandaDto>(resultadoComanda.Value));
    }
}
```

### **🏭 Uso de Factories desde Application**
```csharp
public class CrearClienteCommandHandler : IRequestHandler<CrearClienteCommand, Result<ClienteDto>>
{
    private readonly IClienteRepository _repository;
    private readonly ClienteFactory _clienteFactory;
    
    public async Task<Result<ClienteDto>> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
    {
        // Usar el factory del dominio
        var resultadoCliente = _clienteFactory.CrearCliente(
            request.Nombre,
            request.Email,
            request.Telefono);
            
        if (!resultadoCliente.Succeeded)
            return Result<ClienteDto>.Failure(resultadoCliente.ErrorMessage);
            
        await _repository.AddAsync(resultadoCliente.Value);
        return Result<ComandaDto>.Success(_mapper.Map<ClienteDto>(resultadoCliente.Value));
    }
}
```

## 🔄 **Flujo de Trabajo por Contexto**

### **📋 Ejemplo: Operaciones - Crear Comanda**
1. **Controller** → `CrearComandaCommand`
2. **MediatR** → `CrearComandaCommandHandler`
3. **Handler** → Domain `ComandaBuilder`
4. **Builder** → Entidad `Comanda` validada
5. **Repository** → Persistencia en BD
6. **Mapper** → `ComandaDto` de respuesta

### **🎯 Beneficios de esta Arquitectura**
- ✅ **Consistencia** con la estructura de Domain
- ✅ **Separación clara** de responsabilidades por contexto
- ✅ **CQRS** para separar lecturas de escrituras
- ✅ **Validaciones** en múltiples niveles
- ✅ **Mapeo automático** con AutoMapper
- ✅ **Manejo de errores** centralizado con Result pattern

## 📦 **Registro de Dependencias por Contexto**

```csharp
// ApplicationServiceCollectionExtensions.cs
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(Assembly.GetExecutingAssembly());
        
        // AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        // Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        
        // Contextos específicos
        services.AddCoreApplicationServices();
        services.AddComercialApplicationServices();
        services.AddOperacionesApplicationServices();
        services.AddInventarioApplicationServices();
        services.AddProveedoresApplicationServices();
        
        return services;
    }
}
```

## 🚀 **Próximos Pasos de Implementación**

1. **📁 Reestructurar carpetas** para alinear con Domain
2. **📝 Implementar Commands/Queries** básicos por contexto
3. **🗺️ Configurar AutoMapper** profiles
4. **✅ Implementar validadores** con FluentValidation
5. **🔧 Configurar MediatR** behaviors
6. **📊 Desarrollar DTOs** específicos por contexto

## Ejemplo de uso

Para crear un nuevo cliente:

```csharp
// Desde un controlador o API endpoint
await mediator.Send(new CreateClienteCommand 
{
    Nombre = "Juan",
    Apellido = "Pérez",
    Email = "juan@ejemplo.com",
    Telefono = "555-1234"
});
``` 