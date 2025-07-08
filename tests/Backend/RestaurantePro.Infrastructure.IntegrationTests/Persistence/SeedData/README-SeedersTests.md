# Documento de Pruebas de Seeders - RestaurantePro

## 📋 Propósito
Este documento define la estrategia de pruebas para los seeders de datos críticos del sistema RestaurantePro, asegurando que los datos fundamentales estén disponibles y correctos para el funcionamiento de la API y el frontend.

## 🎯 Objetivos
- **Validar que los seeders críticos funcionan correctamente**
- **Asegurar que los datos críticos existen y están bien formados**
- **Prevenir errores de inyección de dependencias**
- **Garantizar que la API tiene los datos necesarios para funcionar**
- **Facilitar el desarrollo del frontend con datos consistentes**

## 📊 Seeders Críticos del Sistema

### 1. **RolesSeeder** ✅ (COMPLETADO)
- **Propósito:** Crear roles del sistema (ADMINISTRADOR, GERENTE, CAJERO, etc.)
- **Datos críticos:** Roles de usuario para autorización
- **Dependencias:** Ninguna
- **Tests:** ✅ Implementados y funcionando (8/8 tests pasando)
- **Estado:** ✅ COMPLETADO - Todos los tests pasan correctamente

### 2. **IdentityUsersSeeder** ✅ (Implementado)
- **Propósito:** Sincronizar usuarios del dominio con Identity
- **Datos críticos:** Usuarios del sistema con roles asignados
- **Dependencias:** RolesSeeder (debe ejecutarse primero)
- **Tests:** ✅ Implementados
- **Estado:** Funcionando

### 3. **PermisosSeeder** ❌ (Pendiente)
- **Propósito:** Crear permisos del sistema
- **Datos críticos:** Permisos para autorización granular
- **Dependencias:** RolesSeeder
- **Tests:** ❌ Pendiente
- **Estado:** Por implementar

### 4. **ConfiguracionSeeder** ❌ (Pendiente)
- **Propósito:** Crear configuraciones del sistema
- **Datos críticos:** Configuraciones de negocio, parámetros del sistema
- **Dependencias:** Ninguna
- **Tests:** ❌ Pendiente
- **Estado:** Por implementar

### 5. **EstadosSeeder** ❌ (Pendiente)
- **Propósito:** Crear estados del sistema (activo, inactivo, etc.)
- **Datos críticos:** Estados para entidades del dominio
- **Dependencias:** Ninguna
- **Tests:** ❌ Pendiente
- **Estado:** Por implementar

### 6. **UnidadesMedidaSeeder** ❌ (Pendiente)
- **Propósito:** Crear unidades de medida para ingredientes/productos
- **Datos críticos:** Unidades de medida estándar
- **Dependencias:** Ninguna
- **Tests:** ❌ Pendiente
- **Estado:** Por implementar

### 7. **UsuarioAdminSeeder** ❌ (Pendiente)
- **Propósito:** Crear usuario administrador del sistema
- **Datos críticos:** Usuario admin con todos los permisos
- **Dependencias:** RolesSeeder, PermisosSeeder
- **Tests:** ❌ Pendiente
- **Estado:** Por implementar

## 🔄 Orden de Ejecución de Seeders
```
1. RolesSeeder
2. PermisosSeeder
3. EstadosSeeder
4. UnidadesMedidaSeeder
5. ConfiguracionSeeder
6. UsuarioAdminSeeder
7. IdentityUsersSeeder
```

## 🧪 Estrategia de Pruebas

### Tests de Integración para Seeders
Cada seeder debe tener tests que validen:

1. **Creación correcta de datos**
   - Los datos se crean con los valores esperados
   - No hay errores de validación o constraint

2. **Idempotencia**
   - Ejecutar el seeder múltiples veces no duplica datos
   - Los datos existentes se actualizan correctamente

3. **Dependencias**
   - El seeder falla apropiadamente si faltan dependencias
   - El seeder funciona cuando las dependencias están presentes

4. **Validación de datos**
   - Los datos creados son consistentes con el dominio
   - Las relaciones entre entidades son correctas

### Tests de Integración para API
Una vez que los seeders funcionan, validar que la API:

1. **Puede usar los datos sembrados**
   - Autenticación funciona con usuarios sembrados
   - Autorización funciona con roles/permisos sembrados
   - Consultas devuelven datos consistentes

2. **Endpoints críticos funcionan**
   - Login/autenticación
   - Consulta de roles/permisos
   - Operaciones que dependen de datos críticos

## 📁 Estructura de Tests

```
tests/Backend/RestaurantePro.Infrastructure.IntegrationTests/
└── Persistence/
    └── SeedData/
        ├── README-SeedersTests.md (este documento)
        ├── IdentityUsersSeederTests.cs ✅
        ├── RolesSeederTests.cs ❌ (pendiente)
        ├── PermisosSeederTests.cs ❌ (pendiente)
        ├── ConfiguracionSeederTests.cs ❌ (pendiente)
        ├── EstadosSeederTests.cs ❌ (pendiente)
        ├── UnidadesMedidaSeederTests.cs ❌ (pendiente)
        ├── UsuarioAdminSeederTests.cs ❌ (pendiente)
        └── SeedersIntegrationTests.cs ❌ (pendiente - tests E2E)
```

## 🚀 Plan de Implementación

### Fase 1: Tests de Seeders Individuales
- [x] IdentityUsersSeederTests
- [ ] RolesSeederTests
- [ ] PermisosSeederTests
- [ ] ConfiguracionSeederTests
- [ ] EstadosSeederTests
- [ ] UnidadesMedidaSeederTests
- [ ] UsuarioAdminSeederTests

### Fase 2: Tests de Integración
- [ ] SeedersIntegrationTests (validar orden y dependencias)
- [ ] Validar que todos los seeders críticos se ejecutan correctamente

### Fase 3: Tests de API con Datos Sembrados
- [ ] Tests de autenticación con usuarios sembrados
- [ ] Tests de autorización con roles/permisos sembrados
- [ ] Tests de endpoints críticos

## 🎯 Criterios de Éxito

### Para Seeders
- ✅ Todos los seeders críticos tienen tests de integración
- ✅ Los tests pasan consistentemente
- ✅ Los datos se crean correctamente y no se duplican
- ✅ Las dependencias se manejan apropiadamente

### Para API
- ✅ La API puede autenticarse con usuarios sembrados
- ✅ La autorización funciona con roles/permisos sembrados
- ✅ Los endpoints críticos responden correctamente
- ✅ Los datos devueltos son consistentes

### Para Frontend
- ✅ El frontend tiene acceso a todos los datos críticos necesarios
- ✅ No hay errores por falta de datos en la base de datos
- ✅ La experiencia de usuario es consistente

## 🔧 Comandos Útiles

### Ejecutar tests de seeders específicos
```bash
# Test de un seeder específico
dotnet test --filter "FullyQualifiedName~IdentityUsersSeederTests"

# Todos los tests de seeders
dotnet test --filter "FullyQualifiedName~SeedData"
```

### Ejecutar seeders en desarrollo
```bash
# Desde la API
dotnet run --project src/Backend/RestaurantePro.Api
# Los seeders se ejecutan automáticamente al iniciar
```

## 📝 Notas de Desarrollo

### Patrón de Test para Seeders
```csharp
public class [NombreSeeder]Tests : IntegrationTestBase
{
    [Fact]
    public async Task SeedAsync_DeberiaCrearDatosCorrectamente()
    {
        // Arrange
        // Act
        // Assert
    }

    [Fact]
    public async Task SeedAsync_NoDeberiaDuplicarDatos()
    {
        // Arrange
        // Act
        // Assert
    }

    [Fact]
    public async Task ExistsAsync_DeberiaRetornarTrue_CuandoExistenDatos()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

### Consideraciones Importantes
1. **Orden de ejecución:** Los seeders deben ejecutarse en el orden correcto
2. **Dependencias:** Asegurar que las dependencias estén disponibles antes de ejecutar cada seeder
3. **Idempotencia:** Los seeders deben poder ejecutarse múltiples veces sin efectos secundarios
4. **Validación:** Los datos creados deben ser válidos según las reglas del dominio

---

**Última actualización:** [Fecha actual]
**Responsable:** Equipo de Desarrollo RestaurantePro 