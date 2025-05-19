# Seguimiento del Desarrollo TDD - Capa de Dominio

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

### Comercial

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Cliente | ✅ Completo | ✅ Completas | Gestión de clientes |
| ClienteNombre | ✅ Completo | ✅ Completas | Value Object para nombres |
| TarjetaFidelizacion | ✅ Completo | ✅ Completas | Programa de fidelización |
| HistorialPuntos | ✅ Completo | ✅ Completas | Registro de puntos de fidelización |
| ServicioFidelizacion | ✅ Completo | ✅ Completas | Servicios de fidelización y descuentos |
| ClientesFrecuentesPolicy | ✅ Completo | ✅ Completas | Política para clientes frecuentes |

### Operaciones

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Comanda | ✅ Completo | ✅ Completas | Gestión de órdenes |
| ItemComanda | ✅ Completo | ✅ Completas | Elementos de una comanda |
| Reservacion | ✅ Completo | ✅ Completas | Reservación de mesas |
| Mesa | ✅ Completo | ✅ Completas | Gestión de mesas |

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

### Proveedores

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Proveedor | ✅ Completo | ✅ Completas | Gestión de proveedores |
| ContactoProveedor | ✅ Completo | ✅ Completas | Contactos de proveedores |

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

### Inventario ↔ Proveedores
- OrdenCompra se genera para un Proveedor específico
- Proveedor suministra Ingredientes

## Próximos pasos (TDD)

1. **Servicios de integración entre contextos**:
   - ✅ Implementar `ServicioGeneracionOrdenesCompra`: Servicio que analiza niveles de inventario y genera órdenes de compra automáticas
   - ✅ Implementar `ServicioNotificaciones`: Para manejar notificaciones entre contextos (ej. notificar stock bajo a administradores)
   
2. **Políticas de dominio**:
   - ✅ Implementar política `StockBajoPolicy`: Para manejar automáticamente acciones cuando el stock está por debajo del mínimo
   - ✅ Implementar política `ClientesFrecuentesPolicy`: Para analizar patrones de consumo y ofrecer beneficios

3. **Mejoras en eventos de dominio**:
   - Implementar un sistema de suscripción a eventos entre contextos delimitados
   - Mejorar el sistema de registro de eventos para facilitar la auditoría

4. **Capa de Aplicación**:
   - Definir DTOs para la comunicación con capas externas
   - Implementar validaciones a nivel de aplicación
   - Crear servicios de aplicación para orquestar casos de uso

## Plan de Mejoras Dominio Febrero 2024

Tras completar la implementación base del dominio, se han identificado las siguientes áreas de mejora para refinar la capa de dominio:

### 1. Mejora del sistema de eventos de dominio

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Sistema de suscripción entre agregados | Implementar un mecanismo más robusto que permita la suscripción a eventos entre diferentes agregados dentro y fuera de contextos | Alta | Pendiente |
| Registro centralizado de eventos | Crear un servicio que almacene todos los eventos de dominio para auditoría y reconstrucción del estado | Media | ✅ Completado |
| Manejadores de eventos configurables | Permitir la configuración declarativa de manejadores de eventos sin acoplamiento directo | Media | ✅ Completado |

### 2. Refinamiento de políticas de dominio

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Ampliar StockBajoPolicy | Incluir más reglas de negocio como priorización de ingredientes por rotación y temporada | Alta | ✅ Completado |
| Mejorar ClientesFrecuentesPolicy | Añadir segmentación de clientes por comportamiento y campañas personalizadas | Media | ✅ Completado |
| Nueva política: ProductoRecomendadoPolicy | Crear política para recomendar productos basados en historial y tendencias | Baja | Pendiente |

### 3. Validaciones de dominio robustas

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Invariantes en OrdenCompra | Reforzar las reglas de negocio que deben cumplirse en órdenes de compra | Alta | Pendiente |
| Invariantes en Comanda | Mejorar validaciones para garantizar la integridad de las comandas | Media | Pendiente |
| Validaciones en ValueObjects | Introducir validaciones más específicas para objetos como Email, Teléfono, etc. | Media | Pendiente |

### 4. Patrón de especificación

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Implementar patrón base | Crear las interfaces y clases base para implementar el patrón Specification | Alta | ✅ Completado |
| ProductoDisponibleSpecification | Especificación para verificar disponibilidad de productos | Media | ✅ Completado |
| ProveedorActivoSpecification | Especificación para validar proveedores activos para órdenes | Media | ✅ Completado |
| ReservacionValidaSpecification | Especificación para verificar disponibilidad y validez de reservaciones | Media | ✅ Completado |
| IngredienteDisponibleSpecification | Especificación para verificar disponibilidad de ingredientes | Media | ✅ Completado |
| ClienteFrecuenteSpecification | Especificación para identificar clientes frecuentes según criterios de visitas y gastos | Media | ✅ Completado |

### 5. Corrección de pruebas unitarias

| Tarea | Descripción | Prioridad | Estado |
|-------|-------------|-----------|--------|
| Corregir ServicioNotificacionesInventarioTests | Resolver errores de compilación en las pruebas | Alta | Pendiente |
| Ajustar mocks con problemas de expresiones | Modificar setup de pruebas con problemas de árboles de expresión | Alta | Pendiente |
| Aplicar #nullable context | Aplicar contexto de nulabilidad en pruebas para eliminar advertencias | Media | Pendiente |

## Registro de ciclos TDD completados

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

## Plan de integración con otras capas

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

## Mejoras recientes en la arquitectura

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

Esta refactorización demuestra nuestro compromiso con los principios de diseño de dominio, donde identificamos conceptos transversales y los colocamos en un nivel apropiado de la arquitectura, facilitando su reutilización y separando claramente las responsabilidades.

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

5. **Actualización de interfaces**:
   - Nuevo método en `IClienteRepository` para obtener clientes con historial de visitas
   - Ampliación de `ResultadoClientesFrecuentesPolicy` para incluir información sobre segmentación

Esta mejora permite a los restaurantes comprender mejor el comportamiento de sus clientes y adaptar sus estrategias comerciales según los diferentes segmentos, lo que facilitará la creación de campañas personalizadas y acciones específicas para cada grupo.

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

4. **Nueva funcionalidad en la política**:
   - Método para priorizar ingredientes para reposición
   - Integración con notificaciones y generación de órdenes considerando prioridades

5. **Pruebas unitarias**:
   - Verificación del ordenamiento por prioridad considerando temporada
   - Prueba del comportamiento sin considerar temporada
   - Validación de notificaciones y órdenes en orden de prioridad

Esta mejora permite optimizar las compras priorizando ingredientes de alta rotación, en temporada actual, con stock más crítico y considerando costos, lo que resulta en mejor aprovechamiento del presupuesto y reducción de desabastecimientos en productos clave para el negocio.
