# RestaurantePro.Domain.UnitTests

Este proyecto contiene pruebas unitarias para el dominio de RestaurantePro.

## Estructura de pruebas

Las pruebas siguen la misma estructura que el código de dominio:

- `Core`: Pruebas para clases base y componentes compartidos
- `Comercial`: Pruebas para la gestión de clientes y fidelización
- `Operaciones`: Pruebas para comandas y reservaciones
- `Inventario`: Pruebas para la gestión de inventario y compras
- `Proveedores`: Pruebas para la gestión de proveedores

## Progreso en resolución de errores CS0854

Actualmente estamos trabajando en la resolución de errores CS0854 en varios archivos de prueba. Hemos avanzado significativamente pero aún quedan algunos errores por resolver.

### Archivos con correcciones parciales implementadas:

- `Inventario\Services\VerificadorStockTests.cs`
- `Inventario\Policies\StockBajoPolicyTests.cs`
- `Comercial\Services\ComercialServiceFacadeTests.cs`
- `Inventario\Services\GeneradorOrdenesCompraTests.cs`

### Archivos pendientes de corrección completa:

- `Inventario\Services\VerificadorStockTests.cs`
- `Inventario\Policies\StockBajoPolicyTests.cs`
- `Comercial\Services\ComercialServiceFacadeTests.cs`
- `Inventario\Services\GeneradorOrdenesCompraTests.cs`

## Plan de acción para resolver errores CS0854 restantes

### 1. Inventario\Services\VerificadorStockTests.cs
- Corregir líneas 257 y 262: Reemplazar el uso de Task.FromResult<T> en SetupProveedorPorId con lambda tipada explícita
- Corregir líneas 374-387: Crear método auxiliar SetupRequire similar al que ya existe para RequireNotNull
- Corregir líneas 398-411: Revisar y corregir SetupToResultGeneric con lambdas tipadas

### 2. Inventario\Policies\StockBajoPolicyTests.cs
- Corregir líneas 257-283: Crear método auxiliar para configurar AsignarIdEntidad que evite el uso de reflection o argumentos opcionales en lambda

### 3. Comercial\Services\ComercialServiceFacadeTests.cs
- Corregir líneas 73, 219, 241, 270: Métodos SetupRepositoryMethods y Verify con lambdas tipadas
- Corregir líneas 289-411: Métodos de configuración de policy y servicios con lambdas tipadas
- Corregir líneas 553-579: Métodos de configuración de ToResult para tipos específicos 

### 4. Inventario\Services\GeneradorOrdenesCompraTests.cs
- Corregir líneas 139-172: Métodos SetupRequire y SetupRequireNotNull con lambdas tipadas
- Corregir líneas 228-233: Métodos auxiliares para VerificarNoHayErrores y VerificarHayErrores

### Enfoque práctico paso a paso

1. **Comenzar por un archivo**: Elegir un archivo (por ejemplo, `VerificadorStockTests.cs`) y centrarse en resolver todos sus errores
2. **Identificar patrón común**: Analizar los errores para encontrar un patrón común
3. **Crear método auxiliar**: Desarrollar un método auxiliar que resuelva ese patrón
4. **Aplicar corrección**: Implementar la solución en todos los lugares con el mismo patrón
5. **Compilar y verificar**: Ejecutar `dotnet build` para verificar que los errores se han resuelto
6. **Documentar solución**: Actualizar este documento con la solución implementada
7. **Repetir proceso**: Continuar con el siguiente archivo o patrón de error

### Herramientas útiles

- **Comando para identificar errores**: `dotnet build | findstr CS0854`
- **Comando para verificar correcciones**: `dotnet build`
- **Editor con búsqueda y reemplazo en múltiples archivos**: Visual Studio o VS Code

## Sugerencias para resolver problemas con árboles de expresión

Para resolver los problemas con árboles de expresión en las pruebas, aquí hay algunas sugerencias:

1. **Usar It.Is<T>() en lugar de It.IsAny<T>()**: 
   ```csharp
   // En lugar de:
   _repo.Setup(r => r.MetodoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))

   // Usar:
   _repo.Setup(r => r.MetodoAsync(It.Is<Guid>(g => true), It.Is<CancellationToken>(t => true)))
   ```

2. **Extraer la lógica de setup a métodos auxiliares**:
   ```csharp
   private void SetupRepositorio(Guid id, Entidad returnValue)
   {
       var guid = id; // Variable local para capturar el valor
       _repo.Setup(r => r.MetodoAsync(It.Is<Guid>(g => g == guid), It.Is<CancellationToken>(t => true)))
           .ReturnsAsync(returnValue);
   }
   ```

3. **Usar Returns() con lambdas tipadas en lugar de ReturnsAsync()**:
   ```csharp
   // En lugar de:
   _repo.Setup(r => r.MetodoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(returnValue);

   // Usar:
   _repo.Setup(r => r.MetodoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
       .Returns((Guid id, CancellationToken token) => Task.FromResult(returnValue));
   ```

4. **Evitar usar Returns<T> con tipo genérico explícito**:
   ```csharp
   // En lugar de:
   .Returns<Cliente>(c => Result.Success<Cliente>(c));

   // Usar:
   .Returns((Cliente c) => Result.Success(c));
   ```

5. **Usar objetos dummy para evitar devolver el mock directamente**:
   ```csharp
   // En lugar de:
   .Returns(_notificationManagerMock.Object);

   // Usar:
   var dummyManager = new Mock<INotificationManager>().Object;
   .Returns((object obj, string message) => dummyManager);
   ```

## Ejemplos específicos de correcciones realizadas

### 1. Corregir SetupToResult

```csharp
private void SetupToResult()
{
    // Usar lambdas tipadas explícitamente
    _notificationManagerMock
        .Setup(m => m.ToResult(It.IsAny<ResultadoVerificacionStock>()))
        .Returns((ResultadoVerificacionStock r) => Result.Success(r));
        
    _notificationManagerMock
        .Setup(m => m.ToResult(It.IsAny<bool>()))
        .Returns((bool b) => Result.Success(b));
        
    // Más configuraciones similares...
}
```

### 2. Corregir SetupRequireNotNull

```csharp
private void SetupRequireNotNull()
{
    // Usar un objeto dummy para evitar devolver _notificationManagerMock.Object directamente
    var dummyManager = new Mock<INotificationManager>().Object;
    
    // Configurar sobrecargas básicas con lambda tipada
    _notificationManagerMock
        .Setup(m => m.RequireNotNull(It.IsAny<object>(), It.IsAny<string>()))
        .Returns((object obj, string message) => dummyManager);
        
    _notificationManagerMock
        .Setup(m => m.RequireNotNull(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns((object obj, string message, string paramName) => dummyManager);
}
```

### 3. Corregir ConfigurarNotificacionesStockBajo

```csharp
private void ConfigurarNotificacionesStockBajo()
{
    // Usar lambda tipada con parámetros explícitos
    _servicioNotificacionesMock
        .Setup(s => s.NotificarStockBajo(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>()))
        .Returns((Guid id, string nombre, decimal stockActual, decimal stockMinimo) => 
            Task.FromResult(Guid.NewGuid()));
}
```

### 4. Corregir SetupClientePorId

```csharp
private void SetupClientePorId(Guid clienteId, Cliente cliente)
{
    // Usar Returns con lambda tipada en lugar de ReturnsAsync
    if (cliente == null)
    {
        _clienteRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == clienteId), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken token) => Task.FromResult<Cliente>(null));
    }
    else
    {
        _clienteRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == clienteId), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken token) => Task.FromResult(cliente));
    }
}
```

## Metodología recomendada

Para resolver todos los errores CS0854 restantes, se recomienda seguir este enfoque:

1. Identificar patrones comunes de errores en cada archivo
2. Crear métodos auxiliares específicos que utilicen lambdas tipadas
3. Reemplazar las configuraciones de setup que causan errores por llamadas a estos métodos auxiliares
4. Para métodos que retornan Task<T>, usar Returns con lambda tipada en lugar de ReturnsAsync
5. Para métodos que retornan Result<T>, evitar especificar el tipo genérico en Result.Success

## Consejos generales para las pruebas

- Usar `FluentAssertions` para pruebas más legibles
- Incluir pruebas positivas y negativas para cada caso
- Mantener pruebas independientes y autónomas
- Evitar dependencias entre pruebas

## Tecnologías Utilizadas

- **xUnit**: Framework de pruebas unitarias
- **FluentAssertions**: Biblioteca para escribir aserciones más legibles
- **Moq**: Framework de mocking para simular dependencias

## Principios de Pruebas

1. **Independencia**: Cada prueba es independiente de las demás
2. **Aislamiento**: Se utilizan mocks para aislar la unidad bajo prueba
3. **Repetibilidad**: Las pruebas producen el mismo resultado en cada ejecución
4. **Legibilidad**: Uso del patrón AAA (Arrange-Act-Assert) para estructurar las pruebas
5. **Nomenclatura**: Convención [Método_Escenario_ResultadoEsperado] para nombrar pruebas

## Metodología TDD

Las pruebas implementan el ciclo de desarrollo TDD:

1. **Red**: Escribir una prueba que falle para una funcionalidad
2. **Green**: Implementar el código mínimo para que la prueba pase
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- **Escenarios positivos**: Flujo normal de operaciones
- **Escenarios negativos**: Manejo de condiciones de error
- **Casos límite**: Comportamiento en situaciones extremas
- **Invariantes de dominio**: Reglas de negocio que deben cumplirse
- **Eventos de dominio**: Verificación de eventos publicados

## Ejecución de Pruebas

Para ejecutar todas las pruebas:

```bash
dotnet test
```

Para ejecutar pruebas específicas por categoría:

```bash
dotnet test --filter "Category=Comandas"
```

Para ejecutar pruebas específicas por namespace:

```bash
dotnet test --filter "FullyQualifiedName~RestaurantePro.Domain.UnitTests.Operaciones"
```

## Convenciones de Pruebas

1. Cada clase de prueba tiene un nombre que coincide con la clase bajo prueba más el sufijo "Tests"
2. Las pruebas se agrupan por funcionalidad usando Facts y Theories
3. Los datos de prueba se proporcionan mediante:
   - InlineData para casos simples
   - ClassData para conjuntos de datos complejos
   - MemberData para datos generados dinámicamente
4. Se utilizan traits para categorizar las pruebas 

# Guía para resolver errores CS0854 en pruebas con Moq

## El problema CS0854

El error CS0854 "Un árbol de expresión no puede contener una llamada o invocación que use argumentos opcionales" ocurre cuando se utilizan argumentos opcionales en expresiones lambda dentro de configuraciones de Moq.

Este error se produce porque cuando Moq utiliza expresiones lambda para configurar el comportamiento de mocks, estas expresiones se convierten en árboles de expresión, y C# no permite que los árboles de expresión contengan llamadas a métodos con argumentos opcionales.

## Soluciones implementadas

En este proyecto, hemos adoptado varias técnicas para resolver este problema:

### 1. Dividir configuraciones complejas en métodos más pequeños

```csharp
// En lugar de tener muchas configuraciones en un solo método
public void ConfigurarTodo() {
    // Muchas configuraciones aquí...
}

// Dividirlo en métodos más pequeños:
public void ConfigurarTodo() {
    ConfigurarValidaciones();
    ConfigurarRepositorios();
    ConfigurarEventos();
}
```

### 2. Usar Returns con lambda tipada explícitamente en lugar de Returns<T>

```csharp
// Forma incorrecta (puede causar CS0854)
_notificationManagerMock
    .Setup(m => m.ToResult(It.IsAny<Cliente>()))
    .Returns<Cliente>(c => Result.Success(c));

// Forma correcta (evita CS0854)
_notificationManagerMock
    .Setup(m => m.ToResult(It.IsAny<Cliente>()))
    .Returns((Cliente c) => Result.Success(c));
```

### 3. Usar Returns con tipos explícitos en lugar de ReturnsAsync

```csharp
// Forma incorrecta (puede causar CS0854)
_repositoryMock
    .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(entidad);

// Forma correcta (evita CS0854)
_repositoryMock
    .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .Returns((Guid id, CancellationToken token) => Task.FromResult(entidad));
```

### 4. Usar It.IsAny<T>() en lugar de It.Is<T>(predicate) cuando sea posible

```csharp
// Forma que puede causar problemas
_notificationManagerMock
    .Verify(m => m.AddError(It.Is<string>(s => true), It.Is<string>(s => true)), Times.Never);

// Forma más segura
_notificationManagerMock
    .Verify(m => m.AddError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
```

### 5. Evitar usar parámetros opcionales en las lambdas

Cuando se trata de métodos con parámetros opcionales, es mejor especificar todos los parámetros explícitamente en la configuración.

## Archivos que necesitan ser corregidos

### Verificados y corregidos:
- ✅ `VerificadorStockTests.cs`

### Pendientes de corrección:
- `ComercialServiceFacadeTests.cs`
- `GeneradorOrdenesCompraTests.cs`
- `StockBajoPolicyTests.cs`

## Pasos para resolver errores CS0854

1. **Identifica los errores**: Usa `dotnet build` para ver dónde están los errores CS0854.
2. **Localiza el método problemático**: Busca en el archivo el método indicado en el error.
3. **Aplica la técnica adecuada**:
   - Si es un método de configuración con `.Returns<T>()`, cámbialo por `.Returns((T param) => ...)`
   - Si es un método `.ReturnsAsync()`, cámbialo por `.Returns((params) => Task.FromResult(...))`
   - Si hay demasiados parámetros, divide la configuración en métodos más pequeños

## Patrón general para configurar NotificationManager

```csharp
private void SetupNotificationManager()
{
    // Configurar CreateNewNotification para que simplemente retorne
    _notificationManagerMock.Setup(m => m.CreateNewNotification())
        .Verifiable();
        
    // Configurar HasErrors para que retorne false por defecto
    _notificationManagerMock.Setup(m => m.HasErrors)
        .Returns(false);
        
    // Dividir en métodos más pequeños para evitar errores CS0854
    SetupRequireNotNull();
    SetupToResult();
}

private void SetupRequireNotNull()
{
    // Evitar sobrecargas con argumentos opcionales
    _notificationManagerMock
        .Setup(m => m.RequireNotNull(It.IsAny<object>(), It.IsAny<string>()))
        .Returns((object obj, string message) => _notificationManagerMock.Object);
        
    _notificationManagerMock
        .Setup(m => m.RequireNotNull(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns((object obj, string message, string paramName) => _notificationManagerMock.Object);
        
    _notificationManagerMock
        .Setup(m => m.Require(It.IsAny<bool>(), It.IsAny<string>()))
        .Returns((bool condition, string message) => _notificationManagerMock.Object);
        
    _notificationManagerMock
        .Setup(m => m.Require(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns((bool condition, string message, string paramName) => _notificationManagerMock.Object);
}

private void SetupToResult()
{
    _notificationManagerMock
        .Setup(m => m.ToResult(It.IsAny<Cliente>()))
        .Returns((Cliente c) => Result.Success(c));
        
    _notificationManagerMock
        .Setup(m => m.ToResult(It.IsAny<bool>()))
        .Returns((bool b) => Result.Success(b));
        
    // Agregar más configuraciones específicas según sea necesario
}
```

## Patrón general para configurar repositorios

```csharp
private void SetupRepositoryMethods()
{
    // Setup para GuardarCambiosAsync
    _repositoryMock
        .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
        .Returns((CancellationToken token) => Task.FromResult(1));
        
    // Setup para AgregarAsync
    _repositoryMock
        .Setup(r => r.AgregarAsync(It.IsAny<Entidad>(), It.IsAny<CancellationToken>()))
        .Returns((Entidad e, CancellationToken token) => Task.CompletedTask);
        
    // Setup para ActualizarAsync
    _repositoryMock
        .Setup(r => r.ActualizarAsync(It.IsAny<Entidad>()))
        .Returns((Entidad e) => Task.CompletedTask);
}
```

## Consejos adicionales

- Cuando se corrigen errores de compilación, es mejor atacar primero los errores en archivos base o compartidos
- Asegúrate de seguir un patrón consistente en todos los archivos
- La prevención es mejor que la corrección: cuando escribas nuevos tests, utiliza estas técnicas desde el principio
- Para los métodos que utilizan Task.FromResult, recuerda especificar el tipo genérico exacto, especialmente cuando hay nulabilidad 