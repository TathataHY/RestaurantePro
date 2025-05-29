# Configuración de Application Layer

Esta carpeta contiene toda la **configuración transversal** de la capa Application, incluyendo AutoMapper, Dependency Injection y validaciones.

## 🗺️ **AutoMapper Profiles**

### **CoreMappingProfile.cs**
Maneja todos los mapeos para el contexto **Core** (Productos, Usuarios, Notificaciones, Recetas).

```csharp
// Ejemplo de uso en un Handler
public class CrearProductoHandler : IRequestHandler<CrearProductoCommand, Result<ProductoDto>>
{
    private readonly IMapper _mapper;
    
    public async Task<Result<ProductoDto>> Handle(CrearProductoCommand request, CancellationToken cancellationToken)
    {
        // 1. Crear entidad usando Domain Builder
        var producto = _builder.ConNombre(request.Nombre).Construir().Value;
        
        // 2. Persistir
        await _repository.AgregarAsync(producto);
        
        // 3. ✨ AutoMapper hace la magia automáticamente
        var productoDto = _mapper.Map<ProductoDto>(producto);
        
        return Result.Success(productoDto);
    }
}
```

### **Mapeos Configurados:**

| Origen | Destino | Propósito |
|--------|---------|-----------|
| `Producto` → `ProductoDto` | Response completo | APIs de consulta |
| `Producto` → `ProductoSummaryDto` | Response resumido | Listas y grids |
| `ProductoCreateDto` → `CrearProductoCommand` | Input mapping | Desde API a Command |
| `ProductoUpdateDto` → `ActualizarProductoCommand` | Input mapping | Desde API a Command |

### **Mapeos Especiales:**
```csharp
// Mapeo de Value Objects
.ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Precio.Valor))

// Mapeo de propiedades calculadas
.ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo))

// Mapeo con valores por defecto
.ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.CategoriaNombre ?? "Sin categoría"))
```

## 🔧 **Dependency Injection**

### **ApplicationServiceCollection.cs**
Configuración **centralizada** de todos los servicios de Application.

```csharp
// En Program.cs o Startup.cs
services.AddApplicationServices();
```

### **Servicios Registrados:**

#### **🎯 MediatR (Vertical Slices)**
- ✅ Todos los Commands/Queries del assembly
- ✅ Pipeline behaviors (Validation, Logging)
- ✅ Request/Response handling

#### **🗺️ AutoMapper**
- ✅ Configuración manual para evitar conflictos de versiones
- ✅ CoreMappingProfile registrado
- ✅ Singleton pattern para performance

#### **✅ FluentValidation**
- ✅ Todos los validadores del assembly
- ✅ Configuración automática
- ✅ Integración con MediatR pipeline

#### **📦 Servicios por Contexto**
- ✅ Core services setup
- 🔄 Comercial services (pendiente)
- 🔄 Operaciones services (pendiente)
- 🔄 Inventario services (pendiente)
- 🔄 Proveedores services (pendiente)

## 🔄 **Pipeline de MediatR**

### **Orden de Ejecución:**
1. **LoggingBehavior** - Log inicio de request
2. **ValidationBehavior** - Validación automática con FluentValidation
3. **Handler** - Lógica de negocio
4. **LoggingBehavior** - Log fin de request con tiempo

### **Ejemplo de Pipeline:**
```csharp
// Request llega
🚀 Ejecutando CrearProductoCommand

// Validación automática
✅ Validación pasada

// Handler ejecuta
🍕 Iniciando creación de producto: Pizza Margherita
✅ Producto creado exitosamente: 123e4567-e89b-12d3-a456-426614174000

// Pipeline completo
✅ CrearProductoCommand completado en 245ms
```

## 🚀 **Cómo Agregar Nuevos Contextos**

### **1. Crear nuevo MappingProfile:**
```csharp
// Config/Mappings/ComercialMappingProfile.cs
public class ComercialMappingProfile : Profile
{
    public ComercialMappingProfile()
    {
        CreateMap<Cliente, ClienteDto>();
        CreateMap<Factura, FacturaDto>();
        // ... más mapeos
    }
}
```

### **2. Registrar en ApplicationServiceCollection:**
```csharp
var configuration = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<CoreMappingProfile>();
    cfg.AddProfile<ComercialMappingProfile>(); // ✨ Nuevo
});
```

### **3. Agregar servicios específicos:**
```csharp
private static IServiceCollection AddComercialServices(this IServiceCollection services)
{
    // Servicios específicos del contexto Comercial
    return services;
}
```

## ⚡ **Performance y Mejores Prácticas**

### **✅ AutoMapper Optimizado:**
- **Singleton**: Una sola instancia de IMapper
- **Configuración compilada**: MapperConfiguration se compila una vez
- **Mapeos explícitos**: No reflection en runtime

### **✅ MediatR Optimizado:**
- **Assembly scanning**: Una sola vez al startup
- **Pipeline behaviors**: Orden optimizado
- **Dependency injection**: Scoped lifetime para handlers

### **✅ FluentValidation Optimizado:**
- **Assembly scanning**: Registro automático
- **Pipeline integration**: Validación antes del handler
- **Error handling**: Excepciones tipadas

## 🧪 **Testing**

### **Unit Tests para Mapeos:**
```csharp
[Test]
public void CoreMappingProfile_ShouldBeValid()
{
    var configuration = new MapperConfiguration(cfg => cfg.AddProfile<CoreMappingProfile>());
    configuration.AssertConfigurationIsValid();
}

[Test]
public void Producto_To_ProductoDto_ShouldMapCorrectly()
{
    var producto = new ProductoBuilder().ConNombre("Test").Construir().Value;
    var dto = _mapper.Map<ProductoDto>(producto);
    
    Assert.That(dto.Nombre, Is.EqualTo("Test"));
}
```

### **Integration Tests para DI:**
```csharp
[Test]
public void ApplicationServices_ShouldRegisterCorrectly()
{
    var services = new ServiceCollection();
    services.AddApplicationServices();
    
    var provider = services.BuildServiceProvider();
    
    Assert.That(provider.GetService<IMapper>(), Is.Not.Null);
    Assert.That(provider.GetService<IMediator>(), Is.Not.Null);
}
```

---

**¡Configuración completa y optimizada para máximo rendimiento! 🚀** 