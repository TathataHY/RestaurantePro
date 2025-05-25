# Seguimiento del Desarrollo TDD - Capa de Dominio

## Índice de Contenidos
1. [Propósito de este documento](#propósito-de-este-documento)
2. [Estructura General del Dominio](#estructura-general-del-dominio)
3. [Estado de Implementación por Contexto](#estado-de-implementación)
   - [Core](#core)
   - [Comercial](#comercial)
   - [Operaciones](#operaciones)
   - [Inventario](#inventario)
   - [Proveedores](#proveedores)
4. [Estandarización de Eventos de Dominio](#estandarización-de-eventos-de-dominio)
5. [Relaciones entre Contextos](#relaciones-entre-contextos)
6. [Registro Cronológico de Ciclos TDD](#registro-de-ciclos-tdd-completados)
7. [Mejoras y Refactorizaciones](#mejoras-recientes-en-la-arquitectura)
8. [Decisiones de Diseño](#decisiones-de-diseño)
9. [Plan de Integración con Otras Capas](#plan-de-integración-con-otras-capas)
10. [Estado Actual y Tareas Pendientes](#próximos-pasos-prioritarios-noviembre-2024)

## Propósito de este documento

Este documento sirve como guía y registro del desarrollo de la capa de dominio utilizando Test-Driven Development (TDD). Aquí encontrarás:

- El estado actual de implementación de cada módulo
- Las relaciones entre los diferentes contextos y agregados
- El seguimiento del proceso TDD aplicado
- La planificación de próximos desarrollos

## Estructura General del Dominio

```
RestaurantePro.Domain/
├── Core/                 # Componentes base y compartidos
│   ├── Base/             # Clases base (Entity, ValueObject, etc.)
│   ├── BoundedContexts/  # Definición de contextos delimitados
│   ├── Notificaciones/   # Sistema central de notificaciones
│   ├── Productos/        # Catálogo de productos
│   └── SharedKernel/     # Componentes compartidos entre contextos
├── Comercial/            # Gestión de clientes y fidelización
├── Operaciones/          # Comandas y reservaciones
├── Inventario/           # Gestión de inventario y compras
└── Proveedores/          # Gestión de proveedores
```

## Estado de Implementación

### Core

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| EntityBase | ✅ Completo | ✅ Completas | Base para todas las entidades |
| ValueObject | ✅ Completo | ✅ Completas | Base para objetos de valor |
| DomainEvent | ✅ Completo | ✅ Completas | Eventos de dominio |
| Productos | ✅ Completo | ✅ Completas | Catálogo de productos |
| IDateTimeService | ✅ Completo | ✅ Completas | Servicio de fecha/hora |
| Notificaciones | ✅ Completo | ✅ Completas | Sistema central de notificaciones |
| DomainServiceCollection | ✅ Completo | ✅ Completas | Extensiones para registro de servicios |
| Email ValueObject | ✅ Completo | ✅ Completas | Con validaciones específicas para Chile |
| PhoneNumber ValueObject | ✅ Completo | ✅ Completas | Con validaciones específicas para Chile |
| Specification | ✅ Completo | ✅ Completas | Patrón de especificación refactorizado |
| Usuarios | ✅ Completo | ✅ Completas | Gestión de usuarios y servicios relacionados |
| Receta | ✅ Completo | ✅ Completas | Recetas para elaboración de productos |
| IngredienteReceta | ✅ Completo | ✅ Completas | Value Object para ingredientes de recetas |
| RecetaService | ✅ Completo | ✅ Completas | Gestión de recetas e ingredientes |

### Comercial

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Cliente | ✅ Completo | ✅ Completas | Gestión de clientes |
| ClienteNombre | ✅ Completo | ✅ Completas | Value Object para nombres |
| TarjetaFidelizacion | ✅ Completo | ✅ Completas | Programa de fidelización |
| HistorialPuntos | ✅ Completo | ✅ Completas | Registro de puntos de fidelización |
| ServicioFidelizacion | ✅ Completo | ✅ Completas | Servicios de fidelización y descuentos |
| ClientesFrecuentesPolicy | ✅ Completo | ✅ Completas | Política para clientes frecuentes |
| Factura | ✅ Completo | ✅ Completas | Gestión de facturas |
| DetalleFactura | ✅ Completo | ✅ Completas | Líneas de detalle de facturas |
| ServicioFacturacion | ✅ Completo | ✅ Completas | Servicio para gestionar facturación |
| ClienteFrecuenteSpecification | ✅ Completo | ✅ Completas | Especificación para identificar clientes frecuentes |
| PromocionActivaSpecification | ✅ Completo | ✅ Completas | Especificación para verificar promociones activas |
| PromocionElegibleSpecification | ✅ Completo | ✅ Completas | Especificación para determinar elegibilidad |

### Operaciones

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Comanda | ✅ Completo | ✅ Completas | Gestión de órdenes |
| ItemComanda | ✅ Completo | ✅ Completas | Elementos de una comanda |
| Reservacion | ✅ Completo | ✅ Completas | Reservación de mesas |
| Mesa | ✅ Completo | ✅ Completas | Gestión de mesas |
| ReservacionValidaSpecification | ✅ Completo | ✅ Completas | Validación de reservaciones |
| PersonalizacionItem | ✅ Completo | ✅ Completas | Personalización de ítems de comanda |

### Inventario

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Ingrediente | ✅ Completo | ✅ Completas | Materias primas |
| MovimientoInventario | ✅ Completo | ✅ Completas | Registro de movimientos |
| OrdenCompra | ✅ Completo | ✅ Completas | Órdenes a proveedores |
| VerificadorStock | ✅ Completo | ✅ Completas | Verificación y generación de órdenes |
| GeneradorOrdenesCompra | ✅ Completo | ✅ Completas | Generación de órdenes automáticas |
| ServicioNotificacionesInventario | ✅ Completo | ✅ Completas | Adaptador de notificaciones para inventario |
| StockBajoPolicy | ✅ Completo | ✅ Completas | Política para stock bajo |
| IngredienteDisponibleSpecification | ✅ Completo | ✅ Completas | Especificación de disponibilidad |
| IngredienteRotacionAltaSpecification | ✅ Completo | ✅ Completas | Especificación por rotación |

### Proveedores

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Proveedor | ✅ Completo | ✅ Completas | Gestión de proveedores |
| ContactoProveedor | ✅ Completo | ✅ Completas | Contactos de proveedores |
| ProveedorActivoSpecification | ✅ Completo | ✅ Completas | Validación de proveedores activos |
| ProveedorPorCategoriaSpecification | ✅ Completo | ✅ Completas | Filtro por categoría |
| ProveedorCategoria | ✅ Completo | ✅ Completas | Value object para categorías |

## Estandarización de Eventos de Dominio

Se ha completado la estandarización de eventos de dominio siguiendo estas reglas:

1. Los nombres de eventos de dominio no llevan el sufijo "Event"
2. Las propiedades de eventos no deben tener el mismo nombre que la clase
3. Cada evento refleja una acción específica en pasado (ej. ClienteCreado, PuntosAgregados)

| Contexto | Eventos Estandarizados | Estado |
|----------|------------------------|--------|
| Comercial | ClienteCreado, ClienteDesactivado, PuntosAgregados, etc. | ✅ Completo |
| Inventario | IngredienteCreado, StockActualizado, MovimientoRegistrado, etc. | ✅ Completo |
| Operaciones | ComandaCreada, ReservacionConfirmada, etc. | ✅ Completo |
| Proveedores | ProveedorRegistrado, ContactoProveedorAgregado, etc. | ✅ Completo |

## Relaciones entre Contextos

### Comercial ↔ Operaciones
- Cliente puede hacer Reservaciones
- Cliente acumula puntos por Comandas
- TarjetaFidelizacion aplicable a Comandas
- ServicioFidelizacion calcula descuentos para Comandas

### Operaciones ↔ Core
- Comanda contiene Productos
- Reservacion asigna Mesas

### Operaciones ↔ Inventario
- Comanda reduce stock de Ingredientes
- ItemComanda verifica disponibilidad de Ingredientes

### Core ↔ Inventario
- Recetas de Productos utilizan Ingredientes del inventario
- RecetaService verifica disponibilidad de Ingredientes para Productos

### Inventario ↔ Proveedores
- OrdenCompra se genera para un Proveedor específico
- Proveedor suministra Ingredientes

## Mejoras Implementadas

### 1. Mejora del sistema de eventos de dominio

| Tarea | Descripción | Estado |
|-------|-------------|--------|
| Sistema de suscripción entre agregados | Implementación de mecanismo para suscripción a eventos entre diferentes agregados dentro y fuera de contextos | ✅ Completado |
| Registro centralizado de eventos | Servicio para almacenar eventos de dominio para auditoría y reconstrucción del estado | ✅ Completado |
| Manejadores de eventos configurables | Configuración declarativa de manejadores sin acoplamiento directo | ✅ Completado |

### 2. Refinamiento de políticas de dominio

| Tarea | Descripción | Estado |
|-------|-------------|--------|
| Ampliar StockBajoPolicy | Incluye reglas para priorización de ingredientes por rotación y temporada | ✅ Completado |
| Mejorar ClientesFrecuentesPolicy | Segmentación de clientes por comportamiento y campañas personalizadas | ✅ Completado |
| Implementar ProductoRecomendadoPolicy | Recomendación de productos basados en historial y tendencias | ✅ Completado |

### 3. Validaciones de dominio robustas

| Tarea | Descripción | Estado |
|-------|-------------|--------|
| Invariantes en OrdenCompra | Reglas de negocio para órdenes de compra | ✅ Completado |
| Invariantes en Comanda | Validaciones para integridad de comandas | ✅ Completado |
| Validaciones en ValueObjects | Validaciones específicas para Email, Teléfono, etc. | ✅ Completado |
| Mejora validaciones Email | Optimización de validaciones con reglas más inteligentes y detección de patrones repetitivos | ✅ Completado |

### 4. Patrón de especificación

| Tarea | Descripción | Estado |
|-------|-------------|--------|
| Implementar patrón base | Interfaces y clases base para el patrón Specification | ✅ Completado |
| Refactorizar especificaciones | Cambio de SpecificationBase a Specification | ✅ Completado |
| Implementar especificaciones específicas | Conjunto completo de especificaciones para cada contexto | ✅ Completado |

## Registro de Ciclos TDD Completados

| Fecha | Componente | Test → Implementación → Refactor |
|-------|------------|----------------------------------|
| 2023-10-15 | Ingrediente | Pruebas → Implementación → Refactor |
| 2023-10-20 | MovimientoInventario | Pruebas → Implementación → Refactor |
| 2023-10-25 | OrdenCompra | Pruebas → Implementación → Refactor |
| 2023-11-05 | Proveedor | Pruebas → Implementación → Refactor |
| 2023-11-10 | IDateTimeService | Pruebas → Implementación → Refactor |
| 2023-11-12 | ServicioFidelizacion | Pruebas → Implementación → Refactor |
| 2023-11-20 | Estandarización de Eventos | Refactorización → Pruebas → Validación |
| 2023-12-05 | VerificadorStock | Pruebas → Implementación → Refactor |
| 2023-12-10 | ServicioNotificaciones | Pruebas → Implementación → Refactor |
| 2023-12-15 | GeneradorOrdenesCompra | Pruebas → Implementación → Refactor |
| 2023-12-20 | StockBajoPolicy | Pruebas → Implementación → Refactor |
| 2023-12-25 | ClientesFrecuentesPolicy | Pruebas → Implementación → Refactor |
| 2024-03-15 | ClienteFrecuenteSpecification | Pruebas → Implementación → Refactor |
| 2024-03-20 | Segmentación de Clientes | Diseño → Pruebas → Implementación → Refactor |
| 2024-03-25 | Priorización en StockBajoPolicy | Diseño → Pruebas → Implementación → Refactor |
| 2024-03-30 | Sistema de Suscripción entre Agregados | Diseño → Pruebas → Implementación → Refactor |
| 2024-04-05 | Validaciones Robustas OrdenCompra | Pruebas → Implementación → Refactor |
| 2024-04-05 | Corrección ServicioNotificacionesInventarioTests | Pruebas → Implementación → Refactor |
| 2024-04-10 | ProductoRecomendadoPolicy | Pruebas → Implementación → Refactor |
| 2024-04-10 | ProductoRecomendableSpecification | Pruebas → Implementación → Refactor |
| 2024-04-10 | DomainServiceCollectionExtensions | Diseño → Pruebas → Implementación |
| 2024-04-15 | Corrección ClientesFrecuentesPolicyTests | Pruebas → Implementación → Refactor |
| 2024-04-20 | Validaciones Robustas Comanda | Pruebas → Implementación → Refactor |
| 2024-05-10 | ValueObject Email con validaciones Chile | Diseño → Pruebas → Implementación → Refactor |
| 2024-05-10 | ValueObject PhoneNumber con validaciones Chile | Diseño → Pruebas → Implementación → Refactor |
| 2024-05-15 | Corrección Mocks con Expresiones Lambda | Pruebas → Implementación → Refactor |
| 2024-05-20 | Aplicación #nullable context en pruebas | Pruebas → Implementación → Refactor |
| 2024-05-25 | Implementación de Personalización en Ítems | Diseño → Pruebas → Implementación → Refactor |
| 2024-06-05 | Refactorización de Specifications | Refactor → Pruebas → Validación |
| 2024-08-01 | Corrección de nombres en Specifications | Refactor → Pruebas → Validación |
| 2024-09-10 | Factura | Diseño → Pruebas → Implementación → Refactor |
| 2024-09-10 | DetalleFactura | Diseño → Pruebas → Implementación → Refactor |
| 2024-09-10 | ServicioFacturacion | Diseño → Pruebas → Implementación → Refactor |
| 2024-10-15 | Homogeneización IRepository | Diseño → Pruebas → Implementación → Refactor |
| 2024-10-15 | Implementación ObtenerPorSpecAsync | Diseño → Pruebas → Implementación → Refactor |
| 2024-10-15 | ServicioNotificacionesExtensions | Diseño → Pruebas → Implementación → Refactor |
| 2024-10-15 | ValueObjects Empty | Diseño → Pruebas → Implementación → Refactor |
| 2024-10-20 | Cliente con ValueObjects | Diseño → Pruebas → Implementación → Refactor |
| 2024-10-20 | MesaFueraDeServicio | Diseño → Implementación → Refactor |
| 2024-10-20 | Interfaces de repositorio en Application | Análisis → Diseño → Implementación → Refactor |
| 2024-10-21 | IUsuarioActualService consistente | Análisis → Diseño → Implementación |
| 2024-10-21 | Standardización de interfaces en Application | Análisis → Diseño → Implementación |
| 2024-10-23 | Organización Pruebas de Integración | Análisis → Refactor |
| 2024-10-25 | UsuarioCreado_AsignacionRolesTests | Pruebas → Implementación |
| 2024-10-26 | ComandaModificada_ActualizacionInventarioTests | Pruebas → Implementación → Refactor |
| 2024-11-05 | RecetaService | Diseño → Implementación → Refactor |
| 2024-11-05 | Validación de disponibilidad de ingredientes | Diseño → Implementación → Refactor |
| 2024-11-10 | Corrección RecetaServiceTests | Pruebas → Implementación → Refactor |
| 2024-11-10 | Optimización ClientesFrecuentesPolicyTests | Pruebas → Implementación → Refactor |
| 2024-11-12 | Mejora de validaciones en Email ValueObject | Pruebas → Implementación → Refactor |
| 2024-11-14 | CoreServiceFacade | Diseño → Implementación → Pruebas → Refactor |

## Mejoras Recientes en la Arquitectura

### Implementación de CoreServiceFacade (En progreso)

Siguiendo el patrón de fachada de servicios aplicado en otros contextos (como ComercialServiceFacade, InventarioServiceFacade, etc.), se ha iniciado la implementación de una fachada de servicios para el módulo Core. Esta fachada proporcionará una interfaz unificada para acceder a todas las funcionalidades principales del módulo Core, que incluyen:

1. **Gestión de Productos**: Operaciones CRUD para productos y categorías.
2. **Gestión de Recetas**: Funcionalidades para registrar recetas de productos y verificar disponibilidad de ingredientes.
3. **Gestión de Usuarios**: Operaciones para administrar usuarios y roles.
4. **Sistema de Notificaciones**: Funcionalidades para enviar y gestionar notificaciones.

El diseño sigue los principios de:
- Delegación a servicios específicos de dominio
- Encapsulación de lógica compleja
- Validaciones centralizadas
- Operaciones transaccionales

La implementación está casi completa, con la interfaz y la implementación adaptadas a las clases e interfaces reales del dominio. Se han ajustado los métodos para trabajar con las estructuras existentes y seguir las convenciones establecidas. Algunos de los ajustes realizados incluyen:

- Uso de constructores directos en lugar de métodos factory cuando corresponde
- Adaptación a los métodos disponibles en los repositorios
- Gestión adecuada de las referencias y relaciones entre entidades
- Uso de reflection en casos donde los métodos específicos no están disponibles

Queda pendiente completar los tests unitarios para verificar el funcionamiento correcto de la implementación.

### Centralización del sistema de notificaciones

Una de las mejoras importantes realizadas recientemente ha sido la centralización del sistema de notificaciones, que anteriormente estaba ubicado en el contexto de Inventario. Al tratarse de una funcionalidad transversal utilizada por varios módulos de la aplicación, se ha decidido moverlo al módulo Core para facilitar su reutilización.

#### Cambios realizados

1. **Creación de estructura en Core**:
   - Creación de la estructura de directorios en `Core/Notificaciones`
   - Organización en capas: Entities, Enums, Events, Interfaces, Services

2. **Implementación del servicio genérico**:
   - Desarrollo de `IServicioNotificaciones` como interfaz principal
   - Implementación de `ServicioNotificaciones` para gestionar notificaciones de cualquier tipo
   - Adición de métodos genéricos para envío masivo y gestión de notificaciones

3. **Adaptador específico por dominio**:
   - Creación de `ServicioNotificacionesInventario` como adaptador para el contexto de Inventario
   - Mantenimiento de la interfaz `IServicioNotificaciones` específica para Inventario
   - Implementación delegando al servicio principal en Core

4. **Actualización de referencias**:
   - Modificación de `GlobalUsings.cs` para utilizar los nuevos namespaces
   - Actualización de las pruebas unitarias para reflejar los cambios

#### Beneficios

- **Reutilización**: El servicio de notificaciones ahora puede ser utilizado por cualquier contexto sin duplicar código
- **Consistencia**: Todas las notificaciones siguen la misma estructura y comportamiento
- **Extensibilidad**: Fácil adición de nuevos tipos de notificaciones o canales de entrega
- **Mantenimiento**: Centralización de la lógica de notificaciones en un solo lugar

### Mejora de ClientesFrecuentesPolicy con segmentación

Se ha mejorado la política `ClientesFrecuentesPolicy` para incluir una funcionalidad de segmentación de clientes basada en su comportamiento de consumo. Esta mejora permite categorizar a los clientes en varios segmentos para facilitar campañas de marketing personalizadas y estrategias de fidelización más efectivas.

#### Cambios realizados

1. **Creación de nuevo enum**:
   - Definición de `SegmentoCliente` con categorías como: FrecuenciaAlta, TicketAlto, Premium, Creciente, Decreciente, Inactivo
   - Documentación completa de cada segmento y su significado

2. **Ampliación de la entidad Cliente**:
   - Adición de la propiedad `Segmento` para almacenar la clasificación
   - Implementación del método `ActualizarSegmento` para cambiar la clasificación y emitir eventos

3. **Nuevo evento de dominio**:
   - Creación de `SegmentoClienteActualizado` que se dispara cuando cambia la clasificación de un cliente
   - Mantenimiento de estado anterior y nuevo para análisis de tendencias

4. **Implementación de algoritmo de segmentación**:
   - Método `DeterminarSegmentoCliente` que analiza el comportamiento del cliente
   - Criterios para cada segmento basados en frecuencia, gasto y tendencias

### Mejora de StockBajoPolicy con priorización inteligente

Se ha ampliado la política `StockBajoPolicy` para incorporar un sistema inteligente de priorización de ingredientes basado en múltiples factores. Esta mejora permite una gestión más eficiente del inventario y optimiza el proceso de reposición de stock.

#### Cambios realizados

1. **Nuevos enums para clasificación de ingredientes**:
   - Creación de `RotacionIngrediente` con niveles: Baja, Media, Alta, Crítica
   - Implementación de `TemporadaIngrediente` para clasificar ingredientes según estacionalidad

2. **Ampliación de la entidad Ingrediente**:
   - Adición de propiedades para rotación, temporada, control de calidad y costo promedio
   - Implementación de métodos para actualizar estos atributos
   - Creación de eventos de dominio para cada cambio

3. **Algoritmo de priorización**:
   - Ponderación de factores múltiples: rotación (40%), temporada (30%), nivel de stock (20%) y costo (10%)
   - Cálculo de puntuación por temporada considerando la estación actual
   - Ordenamiento inteligente de ingredientes según prioridad calculada

### Sistema de Suscripción entre Agregados

Se ha implementado un sistema de suscripción a eventos que permite a los agregados de diferentes contextos delimitados suscribirse a eventos específicos sin crear dependencias directas entre ellos. Este sistema mejora significativamente la modularidad y desacoplamiento de la arquitectura.

#### Componentes principales

1. **Nuevas clases e interfaces para suscripciones**:
   - `IEventSubscriptionManager` como interfaz principal
   - `EventSubscriptionManager` para gestionar suscripciones
   - `EventSubscriptionCriteria` para filtrar eventos por tipo, entidad y contexto

2. **Ampliación del sistema de eventos**:
   - Mejora de `DomainEventDispatcher` para notificar a suscriptores
   - Métodos de suscripción tipados y con criterios específicos
   - Manejo seguro de excepciones para evitar que errores en un manejador afecten a otros

### Refactorización del Patrón Specification

Se ha llevado a cabo una importante refactorización del patrón Specification, simplificando su implementación y mejorando su integración con LINQ y Entity Framework:

#### Aspectos clave

1. **Eliminación de SpecificationBase**:
   - Se consolidó toda la funcionalidad en la clase `Specification<T>`

2. **Mejora de la implementación**:
   - La interfaz `ISpecification<T>` enfocada en expresiones LINQ
   - El método `ToExpression()` retorna una expresión LINQ compatible con EF Core
   - El método `IsSatisfiedBy(T entity)` derivado automáticamente de `ToExpression()`

3. **Operadores de composición**:
   - Los operadores `And`, `Or` y `Not` retornan `Specification<T>`
   - Mejor encadenamiento de llamadas con tipo fuerte

## Decisiones de Diseño

- Las entidades usan Factory Methods (Crear) en lugar de constructores públicos
- Se utiliza encapsulación estricta con propiedades privadas (set privado)
- Los cambios de estado se realizan mediante métodos específicos
- Cada cambio de estado genera eventos de dominio
- Se priorizan objetos inmutables para valores
- Se separan interfaces de repositorio por contexto
- Los servicios de dominio implementan lógica que involucra múltiples agregados
- Interfaces y clases de implementación se separan en archivos diferentes
- Los eventos de dominio se nombran sin sufijo "Event" y en tiempo pasado
- Las políticas de dominio encapsulan reglas de negocio complejas que implican múltiples entidades y servicios
- El sistema de notificaciones se ha centralizado en el módulo Core para permitir su uso por todos los contextos
- Se utilizan adaptadores específicos para cada contexto que requiere enviar notificaciones

## Plan de Integración con Otras Capas

1. **Capa de Infraestructura**:
   - Implementar repositorios con Entity Framework Core
   - Configurar inyección de dependencias
   - Implementar servicios de persistencia de eventos de dominio

2. **Capa de Aplicación**:
   - Desarrollar servicios de aplicación que orquesten los casos de uso
   - Implementar DTOs y mapeos desde/hacia entidades de dominio
   - Agregar validaciones a nivel de aplicación

3. **Capa de Presentación/API**:
   - Desarrollar controladores API para exponer funcionalidades
   - Implementar autenticación y autorización
   - Configurar middleware para manejo de errores y logging

## Próximos Pasos Prioritarios (Octubre 2024)

### 1. Homogeneización de interfaces de repositorio

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Estandarización de nombres de métodos | Resolver inconsistencia entre `GetByIdAsync` (legacy) y `ObtenerPorIdAsync` (nuevo) | Alta | ✅ Completado |
| Consistencia de CancellationToken | Asegurar que todos los métodos asíncronos acepten y propaguen CancellationToken | Alta | ✅ Completado |
| Interfaces de Application | Revisar y actualizar las interfaces de repositorio en la capa de Application | Media | ✅ Completado |
| Soporte para Especificaciones | Agregar métodos para trabajar con especificaciones en IRepository | Alta | ✅ Completado |

### 2. Utilización consistente de ValueObjects

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Refactorizar Cliente (Comercial) | Implementar Email y PhoneNumber ValueObjects | Alta | ✅ Completado |

### 3. Limpieza y formato

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Comentarios inconsistentes | Eliminar o implementar comentarios sin funcionalidad correspondiente | Baja | ✅ Completado |
| Formato de eventos | Corregir indentación en eventos de Reservaciones | Baja | ✅ Completado |

### 4. Mejoras en documentación

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Guías de uso | Crear guías de uso para los principales componentes del dominio | Baja | ⏳ Pendiente |
| Documentación de Usuarios | Documentar el módulo de usuarios y servicios relacionados | Media | ⏳ Pendiente |

### 5. Mejoras en interfaces de servicio

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Estandarización de IUsuarioActualService | Actualizar a convenciones y tipos consistentes | Media | ✅ Completado |
| Revisión de interfaces de servicio | Actualizar y añadir documentación a interfaces de servicio en Application | Media | ✅ Completado |

### 6. Organización de Pruebas de Integración

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Estructurar pruebas por contexto | Reorganizar pruebas de integración en carpetas por contexto | Media | ✅ Completado |
| Implementar test Core-Usuarios | Implementar prueba UsuarioCreado_AsignacionRolesTests | Alta | ✅ Completado |

## Próximos Pasos Prioritarios (Noviembre 2024)

### 1. Integración con capas superiores

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Puente con Application | Desarrollar servicios puente entre Domain y Application | Alta | ⏳ Pendiente |
| Mapeo con DTOs | Implementar perfil de AutoMapper para entidades y DTOs | Media | ⏳ Pendiente |
| CoreServiceFacade | Implementar fachada de servicio para el módulo Core | Alta | ✅ Completado |

### 2. Pruebas de integración

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Pruebas Core-Comercial | Probar integración entre contextos Core y Comercial | Alta | ⏳ Pendiente |
| Pruebas Operaciones-Inventario | Probar integración entre contextos Operaciones e Inventario | Alta | ✅ Completado |
| Implementar test Operaciones-Comandas | Implementar pruebas en Integration/Operaciones/Comandas | Media | ✅ Completado |

### 3. Servicios de dominio

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Implementar RecetaService | Implementar servicio de recetas para productos | Alta | ✅ Completado |
| Validación de disponibilidad | Implementar validación de disponibilidad de ingredientes | Media | ✅ Completado |
| Mejoras en validación de ValueObjects | Optimizar las validaciones en Email y otros ValueObjects | Baja | ✅ Completado |

### 4. Mejoras en rendimiento y optimización

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Análisis de performance | Identificar cuellos de botella en el dominio | Media | ⏳ Pendiente |
| Implementación de caché | Estrategia de caché para servicios de dominio frecuentes | Baja | ⏳ Pendiente |
| Optimización de consultas | Mejorar las interfaces de repositorio para consultas optimizadas | Media | ⏳ Pendiente |
