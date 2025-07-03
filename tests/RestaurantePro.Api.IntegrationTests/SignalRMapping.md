# Mapa de Implementación de SignalR - RestaurantePro

Este documento mapea todos los componentes necesarios para implementar SignalR en RestaurantePro, incluyendo Hubs, servicios, configuración y tests. Sigue el mismo formato que los documentos de mapping existentes del proyecto.

## 📋 **LEYENDA DE ESTADO**
- **✅/✅**: El componente está implementado en el código fuente (src) Y tiene pruebas unitarias implementadas
- **✅/⬜**: El componente está implementado en el código fuente (src) pero NO tiene pruebas unitarias
- **⬜/⬜**: El componente NO está implementado aún (ni código ni pruebas)
- **🟡/🟡**: El componente está en desarrollo y en pruebas
- **🔄**: Implementación en proceso

El formato es `[Estado en Código]/[Estado en Pruebas]`

## 📊 **RESUMEN GENERAL**
- **Total Componentes SignalR**: 20
- **Componentes Implementados**: 1 ✅ (ISignalRService interface)
- **Componentes en Desarrollo**: 1 🟡 (ComandaHub)
- **Tests Implementados**: 0 ⬜
- **Estado**: 🚀 **FASE 1 - IMPLEMENTACIÓN BASE** - Empezando implementación
- **Próxima Fase**: 🚀 **FASE 2 - NOTIFICACIONES CORE** (Siguiente)

---

## 🏗️ **ARQUITECTURA SIGNALR**

### **Flujo de Comunicación en Tiempo Real**
```
Mesero (App Móvil) → ComandaHub → Cocina (App/Web)
     ↓
  API Controller → SignalRService → Hub → Clientes Conectados
     ↓
  Base de Datos → Event Handlers → Notificaciones Push
```

### **Casos de Uso Principales**
1. **Comandas en Tiempo Real**: Mesero crea comanda → Cocina recibe notificación inmediata
2. **Actualizaciones de Estado**: Cocina actualiza preparación → Mesero ve progreso
3. **Alertas de Inventario**: Stock bajo → Administradores reciben alerta
4. **Notificaciones Generales**: Anuncios del sistema → Todos los usuarios conectados

---

## 🏢 **API LAYER** - `/src/Backend/RestaurantePro.Api/`

### **Hubs**

#### **ComandaHub** - `/Hubs/ComandaHub.cs` 🟡/🟡
**Descripción**: Hub principal para comunicación de comandas entre meseros y cocina
**Responsabilidades**:
- Gestionar conexiones de meseros y cocineros
- Enviar nuevas comandas a la cocina
- Notificar cambios de estado de comandas
- Gestionar grupos por rol (Meseros, Cocina, Administradores)

**Métodos del Hub**:
- `NuevaComanda(ComandaDto comanda)` → Notifica nueva comanda a cocina
- `ActualizarEstadoComanda(Guid comandaId, string estado)` → Actualiza estado en tiempo real
- `JoinGroup(string groupName)` → Agregar usuario a grupo (Meseros/Cocina)
- `LeaveGroup(string groupName)` → Remover usuario de grupo
- `ComandaLista(Guid comandaId)` → Notifica que comanda está lista
- `ComandaEntregada(Guid comandaId)` → Confirma entrega de comanda

**Eventos del Cliente**:
- `RecibirNuevaComanda` → Cliente recibe nueva comanda
- `ComandaActualizada` → Cliente recibe actualización de estado
- `ComandaListaParaServir` → Cliente recibe notificación de comanda lista
- `ComandaEntregadaConfirmada` → Cliente recibe confirmación de entrega

#### **InventarioHub** - `/Hubs/InventarioHub.cs` ⬜/⬜
**Descripción**: Hub para alertas de inventario y stock
**Responsabilidades**:
- Notificar alertas de stock bajo
- Enviar actualizaciones de recepción de mercancía
- Alertas de productos próximos a vencer

**Métodos del Hub**:
- `AlertaStockBajo(Guid ingredienteId, string nombre, decimal stockActual)` → Alerta stock bajo
- `RecepcionMercancia(Guid ordenCompraId, List<ItemRecepcion> items)` → Notifica recepción
- `ProductoPorVencer(Guid ingredienteId, string nombre, DateTime fechaVencimiento)` → Alerta vencimiento

**Eventos del Cliente**:
- `RecibirAlertaStock` → Cliente recibe alerta de stock
- `RecibirActualizacionInventario` → Cliente recibe actualización de inventario
- `RecibirAlertaVencimiento` → Cliente recibe alerta de vencimiento

#### **NotificationHub** - `/Hubs/NotificationHub.cs` ⬜/⬜
**Descripción**: Hub general para notificaciones del sistema
**Responsabilidades**:
- Notificaciones generales del sistema
- Mensajes administrativos
- Alertas de seguridad

**Métodos del Hub**:
- `EnviarNotificacionGlobal(string titulo, string mensaje, string tipo)` → Notificación global
- `EnviarNotificacionARol(string rol, string titulo, string mensaje)` → Notificación por rol
- `EnviarNotificacionAUsuario(Guid usuarioId, string titulo, string mensaje)` → Notificación individual

**Eventos del Cliente**:
- `RecibirNotificacion` → Cliente recibe notificación
- `RecibirMensajeAdmin` → Cliente recibe mensaje administrativo
- `RecibirAlertaSistema` → Cliente recibe alerta del sistema

### **Configuración**

#### **Program.cs** - Configuración SignalR ⬜/⬜
**Modificaciones Necesarias**:
- Agregar `builder.Services.AddSignalR()` en ConfigureServices
- Agregar `app.MapHub<ComandaHub>("/hubs/comandas")` en Configure
- Agregar `app.MapHub<InventarioHub>("/hubs/inventario")` en Configure
- Agregar `app.MapHub<NotificationHub>("/hubs/notifications")` en Configure
- Configurar CORS para SignalR
- Configurar autenticación JWT para Hubs

---

## 🛠️ **INFRASTRUCTURE LAYER** - `/src/Backend/RestaurantePro.Infrastructure/`

### **Services**

#### **SignalRService** - `/Services/SignalRService.cs` ⬜/⬜
**Descripción**: Implementación de ISignalRService para envío de notificaciones
**Responsabilidades**:
- Implementar todos los métodos de ISignalRService
- Gestionar conexiones con los Hubs
- Manejar errores de conexión
- Logging de notificaciones enviadas

**Métodos a Implementar**:
- `EnviarNotificacionAUsuarioAsync(Guid usuarioId, string titulo, string mensaje, string tipo)`
- `EnviarNotificacionAUsuariosAsync(List<Guid> usuariosIds, string titulo, string mensaje, string tipo)`
- `EnviarNotificacionARolAsync(string rol, string titulo, string mensaje, string tipo)`
- `EnviarNotificacionGlobalAsync(string titulo, string mensaje, string tipo)`
- `ActualizarEstadoMesaAsync(Guid mesaId, string estado, object detalles)`
- `ActualizarEstadoComandaAsync(Guid comandaId, string estado, object detalles)`
- `EnviarAlertaInventarioAsync(Guid ingredienteId, string nombreIngrediente, decimal stockActual, decimal stockMinimo)`
- `ObtenerUsuariosConectadosAsync()`
- `UsuarioEstaConectadoAsync(Guid usuarioId)`

#### **HubConnectionManager** - `/Services/HubConnectionManager.cs` ⬜/⬜
**Descripción**: Gestor de conexiones de usuarios en los Hubs
**Responsabilidades**:
- Mapear usuarios a ConnectionIds
- Gestionar grupos de usuarios por rol
- Mantener estado de conexiones activas
- Cleanup de conexiones desconectadas

**Propiedades y Métodos**:
- `Dictionary<Guid, List<string>> UsuarioConexiones` → Mapeo usuario-conexiones
- `Dictionary<string, List<string>> GruposConexiones` → Mapeo grupo-conexiones
- `AgregarConexion(Guid usuarioId, string connectionId, string? grupo)`
- `RemoverConexion(string connectionId)`
- `ObtenerConexionesUsuario(Guid usuarioId)`
- `ObtenerConexionesGrupo(string grupo)`
- `UsuarioEstaConectado(Guid usuarioId)`

### **DTOs**

#### **ComandaSignalRDto** - `/DTOs/SignalR/ComandaSignalRDto.cs` ⬜/⬜
**Descripción**: DTO optimizado para envío por SignalR de comandas
**Propiedades**:
- `Guid Id` → ID de la comanda
- `string NumeroComanda` → Número visible de la comanda
- `Guid MesaId` → ID de la mesa
- `string NumeroMesa` → Número de la mesa
- `string Estado` → Estado actual de la comanda
- `List<ItemComandaSignalRDto> Items` → Items de la comanda
- `DateTime FechaCreacion` → Fecha de creación
- `DateTime? FechaEstimadaEntrega` → Tiempo estimado de entrega
- `string Observaciones` → Observaciones especiales
- `string NombreMesero` → Nombre del mesero responsable
- `string Prioridad` → Prioridad de la comanda (Normal, Alta, Urgente)

#### **NotificacionSignalRDto** - `/DTOs/SignalR/NotificacionSignalRDto.cs` ⬜/⬜
**Descripción**: DTO para notificaciones por SignalR
**Propiedades**:
- `string Titulo` → Título de la notificación
- `string Mensaje` → Mensaje de la notificación
- `string Tipo` → Tipo (info, success, warning, error)
- `DateTime FechaHora` → Timestamp de la notificación
- `Guid? DestinatarioId` → ID del usuario destinatario (null para notificaciones globales)
- `string? Rol` → Rol destinatario (null para notificaciones individuales)
- `object? Datos` → Datos adicionales específicos del tipo de notificación

#### **AlertaInventarioSignalRDto** - `/DTOs/SignalR/AlertaInventarioSignalRDto.cs` ⬜/⬜
**Descripción**: DTO para alertas de inventario
**Propiedades**:
- `Guid IngredienteId` → ID del ingrediente
- `string NombreIngrediente` → Nombre del ingrediente
- `decimal StockActual` → Stock actual
- `decimal StockMinimo` → Stock mínimo
- `string UnidadMedida` → Unidad de medida
- `string TipoAlerta` → Tipo de alerta (StockBajo, StockAgotado, PorVencer)
- `DateTime FechaAlerta` → Fecha de la alerta
- `string? FechaVencimiento` → Fecha de vencimiento (si aplica)

### **DependencyInjection**

#### **SignalRSetup** - `/DependencyInjection/SignalRSetup.cs` ⬜/⬜
**Descripción**: Configuración de dependencias para SignalR
**Responsabilidades**:
- Registrar SignalR en el contenedor de dependencias
- Configurar opciones de SignalR
- Registrar servicios relacionados
- Configurar logging para SignalR

**Métodos**:
- `AddSignalRServices(IServiceCollection services, IConfiguration configuration)`
- `ConfigureSignalROptions(SignalRSettings settings)`
- `RegisterSignalRServices(IServiceCollection services)`

---

## 🔧 **APPLICATION LAYER** - `/src/Backend/RestaurantePro.Application/`

### **Interfaces** ✅/⬜

#### **ISignalRService** - `/Common/Interfaces/ISignalRService.cs` ✅/⬜
**Estado**: YA EXISTE - Interface completamente definida
**Descripción**: Interfaz para el servicio de notificaciones en tiempo real con SignalR
**Todos los métodos están definidos**: 9 métodos para diferentes tipos de notificaciones

### **EventHandlers** - Integración con SignalR

#### **ComandaCreadaSignalRHandler** - `/Operaciones/Comandas/EventHandlers/ComandaCreada/ComandaCreadaSignalRHandler.cs` ⬜/⬜
**Descripción**: Handler para enviar notificación SignalR cuando se crea una comanda
**Evento**: `ComandaCreada`
**Acción**: Enviar comanda a la cocina vía SignalR

#### **ComandaActualizadaSignalRHandler** - `/Operaciones/Comandas/EventHandlers/ComandaActualizada/ComandaActualizadaSignalRHandler.cs` ⬜/⬜
**Descripción**: Handler para notificar cambios de estado de comandas
**Evento**: `ComandaActualizada`
**Acción**: Notificar nuevo estado a meseros y cocina

#### **StockBajoSignalRHandler** - `/Inventario/EventHandlers/StockBajo/StockBajoSignalRHandler.cs` ⬜/⬜
**Descripción**: Handler para alertas de stock bajo
**Evento**: `StockBajoDetectado`
**Acción**: Enviar alerta a administradores

---

## 🧪 **TESTS** - `/tests/`

### **Integration Tests** - `/RestaurantePro.Api.IntegrationTests/`

#### **Hubs Tests**

##### **ComandaHubIntegrationTests** - `/Hubs/ComandaHubIntegrationTests.cs` ⬜/⬜
**Descripción**: Tests de integración para ComandaHub
**Tests a Implementar**:
- `DeberiaEnviarNuevaComandaACocina()` → Test de envío de comandas
- `DeberiaActualizarEstadoComandaEnTiempoReal()` → Test de actualización de estado
- `DeberiaGestionarGruposCorrectamente()` → Test de gestión de grupos
- `DeberiaAutenticarUsuariosCorrectamente()` → Test de autenticación JWT
- `DeberiaRechazarConexionesNoAutenticadas()` → Test de seguridad

##### **InventarioHubIntegrationTests** - `/Hubs/InventarioHubIntegrationTests.cs` ⬜/⬜
**Descripción**: Tests de integración para InventarioHub
**Tests a Implementar**:
- `DeberiaEnviarAlertaStockBajo()` → Test de alertas de stock
- `DeberiaNotificarRecepcionMercancia()` → Test de notificación de recepción
- `DeberiaEnviarAlertaVencimiento()` → Test de alertas de vencimiento

##### **NotificationHubIntegrationTests** - `/Hubs/NotificationHubIntegrationTests.cs` ⬜/⬜
**Descripción**: Tests de integración para NotificationHub
**Tests a Implementar**:
- `DeberiaEnviarNotificacionGlobal()` → Test de notificaciones globales
- `DeberiaEnviarNotificacionPorRol()` → Test de notificaciones por rol
- `DeberiaEnviarNotificacionIndividual()` → Test de notificaciones individuales

#### **FlujosCompletos** - `/FlujosCompletos/`

##### **FlujoSignalRComandasTiempoRealTests** - `/FlujosCompletos/FlujoSignalRComandasTiempoRealTests.cs` ⬜/⬜
**Descripción**: Test del flujo completo de comandas en tiempo real
**Flujo**: Mesero crea comanda → Hub notifica cocina → Cocina actualiza estado → Hub notifica mesero
**Tests**:
- `FlujoCreadaComandaNotificacionCocina()` → Test flujo completo creación
- `FlujoActualizacionEstadoNotificacionMesero()` → Test flujo actualización
- `FlujoComandaListaNotificacionEntrega()` → Test flujo comanda lista

### **Unit Tests** - `/RestaurantePro.Infrastructure.IntegrationTests/`

#### **SignalRServiceTests** - `/Services/SignalRServiceTests.cs` ⬜/⬜
**Descripción**: Tests unitarios para SignalRService
**Tests a Implementar**:
- `DeberiaEnviarNotificacionAUsuario()` → Test envío individual
- `DeberiaEnviarNotificacionAMultiplesUsuarios()` → Test envío múltiple
- `DeberiaEnviarNotificacionPorRol()` → Test envío por rol
- `DeberiaManejatErroresDeConexion()` → Test manejo de errores
- `DeberiaValidarParametrosEntrada()` → Test validación de parámetros

#### **HubConnectionManagerTests** - `/Services/HubConnectionManagerTests.cs` ⬜/⬜
**Descripción**: Tests unitarios para HubConnectionManager
**Tests a Implementar**:
- `DeberiaAgregarConexionCorrectamente()` → Test agregar conexión
- `DeberiaRemoverConexionCorrectamente()` → Test remover conexión
- `DeberiaGestionarGruposCorrectamente()` → Test gestión de grupos
- `DeberiaDetectarUsuariosConectados()` → Test detección de usuarios conectados

---

## ⚙️ **CONFIGURACIÓN**

### **AppSettings** ✅/⬜

#### **SignalRSettings** - `AppSettings.cs` ✅/⬜
**Estado**: YA EXISTE - Configuración básica definida
**Configuraciones Existentes**:
- `Enabled` → Habilitado/Deshabilitado (actualmente false)
- `NotificationHubUrl` → URL del hub (actualmente "/notificationHub")
- `ConnectionTimeoutSeconds` → Timeout de conexión (30 segundos)

**Configuraciones Adicionales Necesarias**:
- `MaxConcurrentConnections` → Máximo de conexiones concurrentes
- `EnableDetailedErrors` → Habilitar errores detallados en desarrollo
- `BackplaneRedisConnectionString` → Redis para escalabilidad (opcional)
- `KeepAliveInterval` → Intervalo de keep-alive
- `ClientTimeoutInterval` → Timeout del cliente

### **CORS Configuration** ⬜/⬜
**Descripción**: Configurar CORS para permitir conexiones SignalR desde apps móviles
**Configuraciones**:
- Permitir conexiones WebSocket
- Configurar origins permitidos para SignalR
- Headers específicos para SignalR

---

## 📊 **ESTADÍSTICAS DE IMPLEMENTACIÓN**

### **Distribución por Capa**
| Capa | Total Componentes | ✅ Implementados | ⬜ Pendientes | 🧪 Tests |
|------|------------------|------------------|---------------|----------|
| **API** | 4 | 0 (0%) | 4 (100%) | 0/8 |
| **Infrastructure** | 7 | 0 (0%) | 7 (100%) | 0/12 |
| **Application** | 1 | 1 (100%) | 0 (0%) | 0/4 |
| **Tests** | 8 | 0 (0%) | 8 (100%) | 0/25 |
| **TOTAL** | **20** | **1 (5%)** | **19 (95%)** | **0/49** |

### **Métricas de Código Estimadas**
- **Líneas de código total**: ~1,200-1,500 líneas
- **Hubs**: ~300 líneas
- **Services**: ~400 líneas
- **DTOs**: ~200 líneas
- **Event Handlers**: ~300 líneas
- **Tests**: ~500 líneas

### **Dependencias Necesarias**
- **Microsoft.AspNetCore.SignalR**: Ya agregado ✅
- **Microsoft.AspNetCore.SignalR.Client**: Para tests (pendiente)
- **StackExchange.Redis**: Para escalabilidad (opcional)

---

## 🚀 **PLAN DE IMPLEMENTACIÓN SUGERIDO**

### **Fase 1: Base SignalR** (Estimado: 4-6 horas)
1. **ComandaHub básico** → Implementar métodos core
2. **SignalRService básico** → Implementar ISignalRService
3. **Program.cs** → Configurar SignalR
4. **Tests básicos** → 2-3 tests de integración

### **Fase 2: Notificaciones Core** (Estimado: 3-4 horas)
1. **NotificationHub** → Hub general de notificaciones
2. **Event Handlers** → Integrar con eventos de dominio
3. **Tests de notificaciones** → Tests completos

### **Fase 3: Inventario y Alertas** (Estimado: 2-3 horas)
1. **InventarioHub** → Hub de alertas de inventario
2. **HubConnectionManager** → Gestor de conexiones
3. **Tests de inventario** → Tests de alertas

### **Fase 4: Optimización y Producción** (Estimado: 2-3 horas)
1. **Configuración avanzada** → CORS, autenticación, logging
2. **Tests de flujos completos** → Tests end-to-end
3. **Documentación** → Actualizar documentación API

---

## 📋 **CHECKLIST DE IMPLEMENTACIÓN**

### **Pre-requisitos**
- [ ] Paquete SignalR agregado al proyecto API ✅
- [ ] Estructura de carpetas creada
- [ ] Configuración base en AppSettings ✅

### **Implementación Core**
- [ ] ComandaHub implementado
- [ ] InventarioHub implementado  
- [ ] NotificationHub implementado
- [ ] SignalRService implementado
- [ ] HubConnectionManager implementado
- [ ] Program.cs configurado
- [ ] DTOs de SignalR creados

### **Event Handlers**
- [ ] ComandaCreadaSignalRHandler
- [ ] ComandaActualizadaSignalRHandler
- [ ] StockBajoSignalRHandler

### **Tests**
- [ ] Tests de integración de Hubs (8 tests mínimo)
- [ ] Tests unitarios de servicios (10 tests mínimo)
- [ ] Tests de flujos completos (3 tests mínimo)

### **Configuración Producción**
- [ ] CORS configurado para SignalR
- [ ] Autenticación JWT en Hubs
- [ ] Logging configurado
- [ ] Configuración `Enabled = true` en AppSettings

---

## 🎯 **BENEFICIOS ESPERADOS POST-IMPLEMENTACIÓN**

### **Operacionales**
- ⚡ **Tiempo real**: Cocina ve comandas instantáneamente
- 📱 **Sincronización**: Apps móviles sincronizadas en tiempo real
- 🔔 **Alertas proactivas**: Notificaciones automáticas de stock e inventario
- 🎯 **Eficiencia**: Reducción del tiempo de comunicación entre staff

### **Técnicos**
- 🏗️ **Escalabilidad**: Arquitectura preparada para múltiples restaurantes
- 🛡️ **Seguridad**: Autenticación JWT en todas las conexiones
- 📊 **Observabilidad**: Logging completo de todas las comunicaciones
- 🧪 **Calidad**: 100% test coverage en funcionalidades críticas

### **Experiencia Usuario**
- 👨‍🍳 **Cocina**: Dashboard en tiempo real de comandas pendientes
- 👨‍💼 **Meseros**: Notificaciones push de comandas listas
- 👨‍💻 **Administradores**: Alertas proactivas de inventario y operaciones
- 📱 **Móvil**: Sincronización perfecta entre dispositivos

---

**📝 Nota**: Este documento será actualizado conforme se implemente cada componente, manteniendo el estado real de la implementación. 
