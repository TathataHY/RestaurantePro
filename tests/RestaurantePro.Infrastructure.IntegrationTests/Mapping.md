# Mapa de Implementación de la Capa de Infraestructura

Este documento mapea las clases y componentes que deben implementarse en la capa de infraestructura de RestaurantePro, junto con su estado de implementación y pruebas.

## 📋 LEYENDA DE ESTADO DE IMPLEMENTACIÓN

En este documento, se utilizan marcas para indicar el estado de implementación de cada componente:

- **✅/✅**: El componente está implementado en el código fuente (src) Y tiene pruebas unitarias implementadas
- **✅/⬜**: El componente está implementado en el código fuente (src) pero NO tiene pruebas unitarias
- **⬜/⬜**: El componente NO está implementado aún (ni código ni pruebas)

El formato es `[Estado en Código]/[Estado en Pruebas]`

## Persistence

### Contexts
- `✅/✅` RestauranteProDbContext - Contexto principal de la base de datos
- `✅/⬜` CoreDbContext - Contexto para el dominio Core
- `✅/⬜` ComercialDbContext - Contexto para el dominio Comercial
- `✅/⬜` OperacionesDbContext - Contexto para el dominio Operaciones
- `✅/⬜` InventarioDbContext - Contexto para el dominio Inventario
- `✅/⬜` ProveedoresDbContext - Contexto para el dominio Proveedores

### Interceptors
- `✅/⬜` AuditableEntityInterceptor - Interceptor para auditoría de entidades
- `✅/⬜` DomainEventInterceptor - Interceptor para eventos de dominio
- `✅/⬜` SoftDeleteInterceptor - Interceptor para borrado lógico

### Configurations

#### Core
- `✅/❌` ProductoConfiguration - Configuración para la entidad Producto
- `✅/❌` UsuarioConfiguration - Configuración para la entidad Usuario
- `✅/❌` NotificacionConfiguration - Configuración para la entidad Notificación
- `✅/❌` RecetaConfiguration - Configuración para la entidad Receta

#### Comercial
- `✅/❌` ClienteConfiguration - Configuración para la entidad Cliente
- `✅/❌` FacturaConfiguration - Configuración para la entidad Factura
- `✅/❌` TarjetaFidelizacionConfiguration - Configuración para la entidad TarjetaFidelización

#### Operaciones
- `✅/❌` ComandaConfiguration - Configuración para la entidad Comanda
- `✅/❌` ReservacionConfiguration - Configuración para la entidad Reservación
- `✅/❌` MesaConfiguration - Configuración para la entidad Mesa
- `✅/❌` PreparacionDiariaConfiguration - Configuración para la entidad PreparaciónDiaria

#### Inventario
- `✅/❌` IngredienteConfiguration - Configuración para la entidad Ingrediente
- `✅/❌` MovimientoInventarioConfiguration - Configuración para la entidad MovimientoInventario
- `✅/❌` OrdenCompraConfiguration - Configuración para la entidad OrdenCompra

#### Proveedores
- `✅/❌` ProveedorConfiguration - Configuración para la entidad Proveedor
- `✅/❌` ContactoProveedorConfiguration - Configuración para la entidad ContactoProveedor

### Repositories

#### Base
- `✅/❌` Repository - Repositorio base genérico
- `✅/❌` UnitOfWork - Implementación de la unidad de trabajo

#### Core
- `✅/✅` ProductoRepository - Repositorio para la entidad Producto
- `✅/✅` UsuarioRepository - Repositorio para la entidad Usuario
- `✅/❌` NotificacionRepository - Repositorio para la entidad Notificación
- `✅/❌` RecetaRepository - Repositorio para la entidad Receta

#### Comercial
- `✅/❌` ClienteRepository - Repositorio para la entidad Cliente
- `✅/❌` FacturaRepository - Repositorio para la entidad Factura
- `✅/❌` TarjetaFidelizacionRepository - Repositorio para la entidad TarjetaFidelización

#### Operaciones
- `✅/❌` ComandaRepository - Repositorio para la entidad Comanda
- `✅/❌` ReservacionRepository - Repositorio para la entidad Reservación
- `✅/❌` MesaRepository - Repositorio para la entidad Mesa
- `✅/❌` PreparacionDiariaRepository - Repositorio para la entidad PreparaciónDiaria

#### Inventario
- `✅/❌` IngredienteRepository - Repositorio para la entidad Ingrediente
- `✅/❌` MovimientoInventarioRepository - Repositorio para la entidad MovimientoInventario
- `✅/❌` OrdenCompraRepository - Repositorio para la entidad OrdenCompra

#### Proveedores
- `✅/❌` ProveedorRepository - Repositorio para la entidad Proveedor
- `✅/❌` ContactoProveedorRepository - Repositorio para la entidad ContactoProveedor

## Identity

### Models
- `✅/❌` ApplicationUser - Modelo de usuario para Identity
- `✅/❌` ApplicationRole - Modelo de rol para Identity
- `✅/❌` ApplicationUserRole - Modelo de relación usuario-rol para Identity

### Services
- `✅/❌` IdentityService - Servicio para gestionar la identidad
- `✅/❌` JwtTokenService - Servicio para generar y validar tokens JWT
- `✅/❌` PermissionService - Servicio para gestionar permisos

### Configuration
- `✅/❌` IdentityConfiguration - Configuración de Identity
- `✅/❌` JwtConfiguration - Configuración de JWT

### Extensions
- `✅/❌` ClaimsPrincipalExtensions - Extensiones para ClaimsPrincipal
- `✅/❌` IdentityResultExtensions - Extensiones para IdentityResult

## ExternalServices

### Payment
- `✅/⬜` PayPalService - Servicio para pagos con PayPal (Estructura implementada pero comentada para uso futuro)
- `✅/⬜` StripeService - Servicio para pagos con Stripe (Estructura implementada pero comentada para uso futuro)

### Email
- `✅/⬜` EmailService - Servicio para envío de correos electrónicos
- `✅/⬜` SendGridService - Servicio para envío de correos electrónicos con SendGrid (Preparado para implementación futura)

### SMS
- `✅/⬜` TwilioService - Servicio para envío de SMS con Twilio (Estructura implementada pero comentada para uso futuro)

### FileStorage
- `✅/⬜` LocalFileService - Servicio para almacenamiento local de archivos
- `✅/⬜` AzureBlobService - Servicio para almacenamiento en Azure Blob Storage (Estructura implementada pero comentada para uso futuro)

## BackgroundTasks

### Jobs
- `✅/⬜` NotificationCleanupJob - Trabajo para limpieza de notificaciones
- `✅/⬜` UserInactivityJob - Trabajo para detectar usuarios inactivos
- `✅/⬜` LoyaltyPointsExpirationJob - Trabajo para expiración de puntos de fidelización
- `✅/⬜` InvoiceReminderJob - Trabajo para recordatorios de facturas
- `✅/⬜` TableCleanupJob - Trabajo para limpieza de mesas
- `✅/⬜` ReservationReminderJob - Trabajo para recordatorios de reservaciones
- `✅/⬜` LowStockAlertJob - Trabajo para alertas de bajo stock
- `✅/⬜` ExpirationCheckJob - Trabajo para verificación de expiración

### Workers
- `✅/⬜` EmailWorker - Worker para procesamiento de correos electrónicos
- `✅/⬜` NotificationWorker - Worker para procesamiento de notificaciones
- `✅/⬜` ReportGenerationWorker - Worker para generación de reportes

## Caching

### Services
- `✅/✅` MemoryCacheService - Servicio de caché en memoria
- `✅/✅` RedisCacheService - Servicio de caché con Redis

## Logging

### Providers
- `✅/⬜` SerilogProvider - Proveedor de logging con Serilog (Estructura básica implementada)
- `✅/⬜` ApplicationInsightsProvider - Proveedor de logging con Application Insights (Estructura básica implementada)

### Configuration
- `✅/⬜` LoggingConfiguration - Configuración para los servicios de logging
- `✅/⬜` LoggingPolicies - Políticas para los servicios de logging

### Enrichers
- `✅/⬜` UserEnricher - Enriquecedor de logs para incluir información del usuario
- `✅/⬜` CorrelationEnricher - Enriquecedor de logs para incluir información de correlación
- `✅/⬜` ContextEnricher - Enriquecedor de logs para incluir información del contexto

## Monitoring

### HealthChecks
- `✅/⬜` DatabaseHealthCheck - Health check para la base de datos
- `✅/⬜` ExternalServiceHealthCheck - Health check para servicios externos
- `✅/⬜` CacheHealthCheck - Health check para caché

## DependencyInjection
- `✅/⬜` InfrastructureSetup - Configuración general de infraestructura
- `✅/⬜` PersistenceSetup - Configuración de persistencia
- `✅/⬜` IdentitySetup - Configuración de identidad
- `✅/⬜` ExternalServicesSetup - Configuración de servicios externos
- `✅/⬜` CachingSetup - Configuración de caché
- `✅/⬜` LoggingSetup - Configuración de logging
- `✅/⬜` BackgroundTasksSetup - Configuración de tareas en segundo plano 