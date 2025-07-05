# Tests de Integración de API - RestaurantePro

Este proyecto contiene las pruebas de integración para la API REST de RestaurantePro, organizadas por contextos de dominio y siguiendo las mejores prácticas de testing.

## 🏗️ **Estructura Esperada del Proyecto**

```
RestaurantePro.Api.IntegrationTests/
├── TestBase/                           # Infraestructura base para tests
│   ├── ApiIntegrationTestBase.cs       # Clase base para todos los tests de API
│   ├── TestWebApplicationFactory.cs    # Factory personalizada para tests
│   └── TestDataBuilders/               # Builders para crear datos de prueba
│       ├── ProductoTestDataBuilder.cs
│       ├── ClienteTestDataBuilder.cs
│       ├── ComandaTestDataBuilder.cs
│       └── ...
│
├── Controllers/                        # Tests organizados por contextos
│   ├── Core/                          # Tests para controladores del contexto Core
│   │   ├── ProductosControllerTests.cs
│   │   ├── UsuariosControllerTests.cs
│   │   ├── NotificacionesControllerTests.cs
│   │   └── RecetasControllerTests.cs
│   ├── Comercial/                     # Tests para controladores del contexto Comercial
│   │   ├── ClientesControllerTests.cs
│   │   ├── FacturasControllerTests.cs
│   │   ├── TarjetasFidelizacionControllerTests.cs
│   │   ├── PromocionesControllerTests.cs
│   │   └── ReportesComercialControllerTests.cs
│   ├── Operaciones/                   # Tests para controladores del contexto Operaciones
│   │   ├── ComandasControllerTests.cs
│   │   ├── ReservacionesControllerTests.cs
│   │   ├── MesasControllerTests.cs
│   │   ├── PreparacionesControllerTests.cs
│   │   └── ReportesOperacionesControllerTests.cs
│   ├── Inventario/                    # Tests para controladores del contexto Inventario
│   │   ├── IngredientesControllerTests.cs
│   │   ├── OrdenesCompraControllerTests.cs
│   │   ├── MovimientosInventarioControllerTests.cs
│   │   └── ReportesInventarioControllerTests.cs
│   └── Proveedores/                   # Tests para controladores del contexto Proveedores
│       ├── ProveedoresControllerTests.cs
│       ├── ContactosProveedorControllerTests.cs
│       └── EvaluacionesProveedorControllerTests.cs
│
├── Middleware/                        # Tests para middleware personalizado
│   ├── ExceptionMiddlewareTests.cs
│   ├── AuthenticationMiddlewareTests.cs
│   └── ValidationMiddlewareTests.cs
│
├── Authentication/                    # Tests de autenticación y autorización
│   ├── JwtAuthenticationTests.cs
│   ├── RoleBasedAuthorizationTests.cs
│   └── PermissionBasedAuthorizationTests.cs
│
├── Filters/                          # Tests para filtros personalizados
│   ├── ApiExceptionFilterTests.cs
│   ├── ValidationFilterTests.cs
│   └── CacheFilterTests.cs
│
├── Integration/                      # Tests de integración entre contextos
│   ├── CrossContextOperationsTests.cs
│   ├── TransactionalOperationsTests.cs
│   └── EventHandlingTests.cs
│
├── Performance/                      # Tests de rendimiento
│   ├── LoadTestingTests.cs
│   ├── ConcurrencyTests.cs
│   └── CachePerformanceTests.cs
│
├── Security/                         # Tests de seguridad
│   ├── InputValidationTests.cs
│   ├── SqlInjectionTests.cs
│   └── XssProtectionTests.cs
│
├── GlobalUsings.cs                   # Importaciones globales
├── README.md                         # Este archivo
├── Mapping.md                        # Mapeo de implementación y testing
└── RestaurantePro.Api.IntegrationTests.csproj
```

## 🔧 **Configuración del Proyecto**

### **Paquetes NuGet Requeridos**
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.6" />
<PackageReference Include="FluentAssertions" Version="8.3.0" />
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="Testcontainers.MsSql" Version="4.6.0" />
<PackageReference Include="Bogus" Version="35.6.1" />
<PackageReference Include="Respawn" Version="6.2.1" />
```

### **Referencias de Proyecto**
```xml
<ProjectReference Include="..\..\src\Backend\RestaurantePro.Api\RestaurantePro.Api.csproj" />
<ProjectReference Include="..\..\src\Backend\RestaurantePro.Application\RestaurantePro.Application.csproj" />
<ProjectReference Include="..\..\src\Backend\RestaurantePro.Infrastructure\RestaurantePro.Infrastructure.csproj" />
```

## 🧪 **Tipos de Tests Implementados**

### **1. Tests de Controladores (CRUD)**
- **GET**: Obtener recursos (individuales y colecciones)
- **POST**: Crear nuevos recursos
- **PUT**: Actualizar recursos existentes
- **DELETE**: Eliminar recursos
- **PATCH**: Actualizaciones parciales

### **2. Tests de Validación**
- Validación de modelos de entrada
- Validación de reglas de negocio
- Manejo de errores y excepciones
- Respuestas HTTP apropiadas

### **3. Tests de Autenticación y Autorización**
- Acceso sin autenticación
- Acceso con roles incorrectos
- Validación de tokens JWT
- Permisos específicos por endpoint

### **4. Tests de Integración**
- Operaciones que afectan múltiples contextos
- Transacciones distribuidas
- Consistencia de datos
- Eventos de dominio

### **5. Tests de Rendimiento**
- Carga de trabajo pesada
- Operaciones concurrentes
- Optimización de consultas
- Uso de caché

## 🚀 **Cómo Ejecutar los Tests**

### **Ejecutar Todos los Tests**
```bash
cd tests/RestaurantePro.Api.IntegrationTests
dotnet test
```

### **Ejecutar Tests por Contexto**
```bash
# Tests del contexto Core
dotnet test --filter "FullyQualifiedName~Core"

# Tests del contexto Comercial
dotnet test --filter "FullyQualifiedName~Comercial"

# Tests del contexto Operaciones
dotnet test --filter "FullyQualifiedName~Operaciones"

# Tests del contexto Inventario
dotnet test --filter "FullyQualifiedName~Inventario"

# Tests del contexto Proveedores
dotnet test --filter "FullyQualifiedName~Proveedores"
```

### **Ejecutar Tests Específicos**
```bash
# Tests de un controlador específico
dotnet test --filter "FullyQualifiedName~ProductosControllerTests"

# Test específico
dotnet test --filter "DisplayName~GetProductos_SinProductos_DebeRetornarListaVacia"
```

### **Ejecutar con Cobertura de Código**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 **Patrones de Testing Utilizados**

### **AAA Pattern (Arrange-Act-Assert)**
```csharp
[Fact]
public async Task GetProducto_ConIdExistente_DebeRetornarProducto()
{
    // Arrange
    var producto = await CrearProductoEnBD();
    
    // Act
    var response = await _httpClient.GetAsync($"/api/productos/{producto.Id}");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var productoResponse = await DeserializarRespuesta<ProductoDto>(response);
    productoResponse.Should().NotBeNull();
    productoResponse.Id.Should().Be(producto.Id);
}
```

### **Test Data Builders**
```csharp
public class ProductoTestDataBuilder
{
    private string _nombre = "Producto Test";
    private decimal _precio = 10.50m;
    private ProductoCategoria _categoria = ProductoCategoria.PlatoPrincipal;
    
    public ProductoTestDataBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }
    
    public ProductoTestDataBuilder ConPrecio(decimal precio)
    {
        _precio = precio;
        return this;
    }
    
    public CrearProductoCommand Build() => new()
    {
        Nombre = _nombre,
        Precio = _precio,
        Categoria = _categoria
    };
}
```

### **Factory Pattern para Tests**
```csharp
public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Reemplazar base de datos real con InMemory
            RemoveService<DbContextOptions<RestauranteProDbContext>>(services);
            services.AddDbContext<RestauranteProDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));
        });
        
        builder.UseEnvironment("Testing");
    }
}
```

## 🔍 **Convenciones de Nomenclatura**

### **Nombres de Clases de Test**
- `{Controller}ControllerTests` para tests de controladores
- `{Middleware}MiddlewareTests` para tests de middleware
- `{Feature}IntegrationTests` para tests de integración

### **Nombres de Métodos de Test**
```
{Método}_{Escenario}_{ResultadoEsperado}
```

**Ejemplos:**
- `GetProductos_SinProductos_DebeRetornarListaVacia`
- `PostProducto_ConDatosInvalidos_DebeRetornar400`
- `DeleteProducto_SinAutenticacion_DebeRetornar401`

## 📈 **Métricas y Reportes**

### **Cobertura de Código Objetivo**
- **Controladores**: 90%+ de cobertura
- **Middleware**: 85%+ de cobertura
- **Filtros**: 80%+ de cobertura
- **Servicios**: 95%+ de cobertura

### **Tipos de Reportes Generados**
- Cobertura de código por contexto
- Tiempo de ejecución de tests
- Tests fallidos y razones
- Métricas de rendimiento

## 🛠️ **Herramientas de Desarrollo**

### **Debugging de Tests**
```bash
# Ejecutar con información detallada
dotnet test --logger "console;verbosity=detailed"

# Ejecutar tests específicos en modo debug
dotnet test --filter "DisplayName~NombreDelTest" --logger "console;verbosity=detailed"
```

### **Generación de Datos de Prueba**
- **Bogus**: Para generar datos falsos realistas
- **Test Data Builders**: Para construir objetos de prueba complejos
- **Seeders de Testing**: Para datos específicos de integración

## 🔄 **Integración Continua**

### **Pipeline de CI/CD**
1. **Compilación**: Verificar que el código compila
2. **Tests Unitarios**: Ejecutar tests rápidos
3. **Tests de Integración**: Ejecutar tests de API
4. **Análisis de Código**: SonarQube/CodeQL
5. **Cobertura**: Generar reportes de cobertura
6. **Deployment**: Si todos los tests pasan

### **Configuración de GitHub Actions**
```yaml
name: API Integration Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Run API Integration Tests
        run: |
          cd tests/RestaurantePro.Api.IntegrationTests
          dotnet test --logger trx --collect:"XPlat Code Coverage"
```

## 📝 **Mejores Prácticas**

### **✅ Hacer**
- Usar nombres descriptivos para tests
- Limpiar la base de datos entre tests
- Usar datos de prueba realistas
- Verificar tanto el código de estado como el contenido
- Probar casos edge y errores
- Mantener tests independientes entre sí

### **❌ No Hacer**
- Depender del orden de ejecución de tests
- Usar datos de producción en tests
- Crear tests que modifiquen estado global
- Ignorar tests fallidos
- Crear tests demasiado complejos
- Duplicar lógica de negocio en tests

## 🎯 **Objetivos del Proyecto**

1. **Cobertura Completa**: Todos los endpoints de la API deben tener tests
2. **Calidad Alta**: Tests que realmente validen la funcionalidad
3. **Mantenibilidad**: Código de tests fácil de entender y mantener
4. **Velocidad**: Tests que ejecuten rápidamente
5. **Confiabilidad**: Tests estables que no fallen aleatoriamente

## 📞 **Soporte y Documentación**

Para más información sobre:
- **Arquitectura del Sistema**: Ver `docs/arquitectura/`
- **Casos de Uso**: Ver `docs/casos-uso/`
- **API Documentation**: Ver swagger en `/swagger/index.html`
- **Domain Logic**: Ver `src/Backend/RestaurantePro.Domain/README.md`
- **Application Layer**: Ver `src/Backend/RestaurantePro.Application/README.md` 