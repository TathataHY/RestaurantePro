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
- [x] Tests de endpoints críticos ✅ (COMPLETADO)

### Fase 4: Tests de Flujos de Negocio ✅ (COMPLETADO)
- [x] CRUD de entidades principales ✅ (COMPLETADO)
- [x] Flujos de operaciones completas ✅ (COMPLETADO)
- [x] Validación de reglas de negocio ✅ (COMPLETADO)

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

## 📊 Estado Actual del Proyecto

### ✅ COMPLETADO
- **Fase 1: Tests de Seeders Individuales** - Todos los seeders críticos validados
- **Fase 2: Tests de Integración** - Validación E2E de todos los seeders
- **Fase 3: Tests de API** - Autenticación, autorización y endpoints críticos validados
- **Fase 4: Tests de Flujos de Negocio** - Validación de procesos completos ✅ (COMPLETADO)

### 🎯 PRÓXIMOS PASOS
- **Fase 5: Tests de Rendimiento** - Validación de performance con datos reales

## 📈 Resumen de Tests AuthController (Fase 3)
- **Total de tests:** 13/13 ✅ PASANDO
- **Tests de autenticación:** 6/6 ✅
- **Tests de autorización:** 4/4 ✅
- **Tests de endpoints críticos:** 3/3 ✅

### Tests Implementados:
1. ✅ Login con credenciales válidas
2. ✅ Login con credenciales inválidas (401)
3. ✅ Registro de usuarios
4. ✅ Registro con email duplicado (400)
5. ✅ Obtener perfil sin autenticación (401)
6. ✅ Obtener perfil con autenticación
7. ✅ Login con usuario admin sembrado
8. ✅ Autorización: Admin puede acceder a endpoint protegido
9. ✅ Autorización: Cajero NO puede acceder a endpoint de admin (403)
10. ✅ Consulta de usuarios sembrados
11. ✅ Consulta de perfil de usuario sembrado
12. ✅ Cambio de contraseña
13. ✅ Logout

## 🚀 Resumen de Tests Flujos de Negocio (Fase 4) ✅ COMPLETADO
- **Total de tests:** 88/88 ✅ PASANDO (100% ÉXITO)
- **Tiempo de ejecución:** 51.5 segundos
- **Flujos críticos validados:** 18 flujos completos

### Flujos de Negocio Implementados:
1. ✅ **Gestión de Proveedores Completa** - CRUD completo con evaluaciones
2. ✅ **Facturación Completa** - Comandas → Facturas → Pagos
3. ✅ **Gestión de Inventario Inteligente** - Stock bajo → Orden de compra → Recepción
4. ✅ **Sistema de Fidelización** - Puntos, descuentos, tarjetas
5. ✅ **Reservaciones Inteligentes** - Gestión de mesas y horarios
6. ✅ **Promociones Dinámicas** - Descuentos automáticos
7. ✅ **SignalR en Tiempo Real** - Notificaciones de comandas
8. ✅ **Transacciones Distribuidas** - Consistencia de datos
9. ✅ **Eventos de Dominio** - Automatización de procesos
10. ✅ **Cache Inteligente** - Optimización de rendimiento
11. ✅ **Monitoreo y Alertas** - Sistema de notificaciones
12. ✅ **Backup y Recuperación** - Respaldo de datos
13. ✅ **Business Intelligence** - Reportes y analytics
14. ✅ **Configuración del Sistema** - Gestión de configuraciones
15. ✅ **Atención al Cliente** - Flujos de servicio completo
16. ✅ **Analytics de Inventario** - Reportes de stock
17. ✅ **Registro y Autenticación Segura** - Flujos de seguridad
18. ✅ **Reportes Operativos** - Métricas en tiempo real

### Métricas de Éxito:
- **Cobertura de flujos:** 100% de flujos críticos validados
- **Tiempo de respuesta:** Todos los flujos completan en < 1 minuto
- **Consistencia de datos:** Transacciones distribuidas funcionando correctamente
- **Eventos de dominio:** Automatización de procesos operativa
- **SignalR:** Notificaciones en tiempo real funcionando
- **Cache:** Optimización de rendimiento validada

---

**Última actualización:** 8 de Julio 2025
**Responsable:** Equipo de Desarrollo RestaurantePro 