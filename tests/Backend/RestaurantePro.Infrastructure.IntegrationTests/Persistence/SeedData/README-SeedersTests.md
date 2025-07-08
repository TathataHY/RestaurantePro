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

### 3. **PermisosSeeder** ✅ (COMPLETADO)
- **Propósito:** Validar permisos del sistema (NO crea datos, solo valida)
- **Datos críticos:** Validación de consistencia de permisos
- **Dependencias:** RolesSeeder
- **Tests:** ✅ Implementados y funcionando (9/9 tests pasando)
- **Estado:** ✅ COMPLETADO - Validación de 53 permisos, 5 peligrosos identificados

### 4. **ConfiguracionSeeder** ✅ (COMPLETADO)
- **Propósito:** Validar configuraciones del sistema (NO crea datos, solo valida)
- **Datos críticos:** Validación de consistencia de configuraciones
- **Dependencias:** Ninguna
- **Tests:** ✅ Implementados y funcionando (11/11 tests pasando)
- **Estado:** ✅ COMPLETADO - Validación de configuraciones del sistema

### 5. **EstadosSeeder** ✅ (COMPLETADO)
- **Propósito:** Validar estados del sistema (NO crea datos, solo valida)
- **Datos críticos:** Validación de consistencia de estados en enums
- **Dependencias:** Ninguna
- **Tests:** ✅ Implementados y funcionando (10/10 tests pasando)
- **Estado:** ✅ COMPLETADO - Validación de 50+ estados del sistema

### 6. **UnidadesMedidaSeeder** ✅ (COMPLETADO)
- **Propósito:** Validar unidades de medida del sistema (NO crea datos, solo valida)
- **Datos críticos:** Validación de consistencia de unidades en enum
- **Dependencias:** Ninguna
- **Tests:** ✅ Implementados y funcionando (10/10 tests pasando)
- **Estado:** ✅ COMPLETADO - Validación de 15 unidades de medida

### 7. **UsuarioAdminSeeder** ✅ (COMPLETADO)
- **Propósito:** Crear usuario administrador del sistema
- **Datos críticos:** Usuario admin con todos los permisos
- **Dependencias:** RolesSeeder, PermisosSeeder
- **Tests:** ✅ Implementados y funcionando (10/10 tests pasando)
- **Estado:** ✅ COMPLETADO - Usuario admin creado correctamente

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
        ├── RolesSeederTests.cs ✅ (COMPLETADO)
        ├── PermisosSeederTests.cs ✅ (COMPLETADO)
        ├── ConfiguracionSeederTests.cs ✅ (COMPLETADO)
        ├── EstadosSeederTests.cs ✅ (COMPLETADO)
        ├── UnidadesMedidaSeederTests.cs ✅ (COMPLETADO)
        ├── UsuarioAdminSeederTests.cs ✅ (COMPLETADO)
        └── SeedersIntegrationTests.cs ✅ (COMPLETADO)
```

## 🚀 Plan de Implementación

### Fase 1: Tests de Seeders Individuales
- [x] IdentityUsersSeederTests
- [x] RolesSeederTests ✅ (COMPLETADO)
- [x] PermisosSeederTests ✅ (COMPLETADO)
- [x] ConfiguracionSeederTests ✅ (COMPLETADO)
- [x] EstadosSeederTests ✅ (COMPLETADO)
- [x] UnidadesMedidaSeederTests ✅ (COMPLETADO)
- [x] UsuarioAdminSeederTests ✅ (COMPLETADO)

### Fase 2: Tests de Integración
- [x] SeedersIntegrationTests (validar orden y dependencias) ✅ (COMPLETADO)
- [x] Validar que todos los seeders críticos se ejecutan correctamente

### Fase 3: Tests de API con Datos Sembrados
- [x] Tests de autenticación con usuarios sembrados ✅ (COMPLETADO)
- [x] Tests de autorización con roles/permisos sembrados ✅ (COMPLETADO)
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
- [ ] Los endpoints críticos responden correctamente
- [ ] Los datos devueltos son consistentes

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