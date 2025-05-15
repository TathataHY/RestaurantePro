# Matriz de Requisitos - RestaurantePro

## Índice
1. [Introducción](#introducción)
2. [Convenciones](#convenciones)
3. [Requisitos Funcionales](#requisitos-funcionales)
4. [Requisitos No Funcionales](#requisitos-no-funcionales)
5. [Matriz de Trazabilidad](#matriz-de-trazabilidad)

## Introducción

Este documento presenta la matriz de requisitos para el sistema RestaurantePro. Los requisitos están clasificados por tipo (funcional o no funcional), módulo, prioridad y complejidad estimada.

## Convenciones

### Prioridad
- **Alta**: Esencial para el funcionamiento del sistema. Debe implementarse en la primera iteración.
- **Media**: Importante pero puede implementarse en iteraciones posteriores.
- **Baja**: Deseable pero no crítico para el funcionamiento básico del sistema.

### Complejidad
- **Alta**: Requiere significativo esfuerzo de desarrollo (>40 horas)
- **Media**: Esfuerzo moderado de desarrollo (16-40 horas)
- **Baja**: Esfuerzo relativamente pequeño (<16 horas)

### Identificadores
- **RF-**: Requisito Funcional
- **RNF-**: Requisito No Funcional

## Requisitos Funcionales

### Módulo: Autenticación y Seguridad

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RF-A01 | El sistema debe permitir a los usuarios iniciar sesión con email y contraseña | Alta | Baja | Pendiente |
| RF-A02 | El sistema debe gestionar roles de usuario (Mesero, Cocinero, Gerente, Administrador) | Alta | Media | Pendiente |
| RF-A03 | El sistema debe permitir cerrar sesión | Alta | Baja | Pendiente |
| RF-A04 | El sistema debe permitir recuperar contraseña mediante email | Media | Baja | Pendiente |
| RF-A05 | El sistema debe bloquear cuentas después de 5 intentos fallidos | Media | Baja | Pendiente |
| RF-A06 | El sistema debe permitir al administrador gestionar usuarios y permisos | Alta | Media | Pendiente |
| RF-A07 | El sistema debe implementar autenticación de doble factor para roles administrativos | Baja | Media | Pendiente |
| RF-A08 | El sistema debe registrar y mostrar la actividad de los usuarios | Media | Media | Pendiente |

### Módulo: Gestión de Mesas

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RF-M01 | El sistema debe mostrar un plano visual de las mesas del restaurante | Alta | Media | Pendiente |
| RF-M02 | El sistema debe permitir gestionar el estado de las mesas (Libre, Ocupada, Reservada, Mantenimiento) | Alta | Baja | Pendiente |
| RF-M03 | El sistema debe permitir ver información detallada de cada mesa | Alta | Baja | Pendiente |
| RF-M04 | El sistema debe permitir filtrar mesas por estado, capacidad o ubicación | Media | Baja | Pendiente |
| RF-M05 | El sistema debe permitir al administrador configurar el plano de mesas | Alta | Alta | Pendiente |
| RF-M06 | El sistema debe mostrar el historial de uso de cada mesa | Baja | Media | Pendiente |
| RF-M07 | El sistema debe actualizar el estado de las mesas en tiempo real | Alta | Media | Pendiente |

### Módulo: Gestión de Productos y Menú

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RF-P01 | El sistema debe permitir crear, editar y desactivar productos | Alta | Media | Pendiente |
| RF-P02 | El sistema debe permitir categorizar productos | Alta | Baja | Pendiente |
| RF-P03 | El sistema debe permitir definir precios, descripciones e imágenes para productos | Alta | Baja | Pendiente |
| RF-P04 | El sistema debe permitir definir ingredientes para cada producto | Alta | Media | Pendiente |
| RF-P05 | El sistema debe permitir marcar productos como personalizables | Media | Baja | Pendiente |
| RF-P06 | El sistema debe permitir importar/exportar catálogos de productos | Baja | Media | Pendiente |
| RF-P07 | El sistema debe permitir gestionar la disponibilidad de productos | Alta | Baja | Pendiente |
| RF-P08 | El sistema debe permitir definir tiempos estimados de preparación | Media | Baja | Pendiente |

### Módulo: Toma de Comandas

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RF-C01 | El sistema debe permitir crear nuevas comandas asociadas a mesas | Alta | Media | Pendiente |
| RF-C02 | El sistema debe permitir agregar productos a una comanda | Alta | Media | Pendiente |
| RF-C03 | El sistema debe permitir personalizar productos | Alta | Media | Pendiente |
| RF-C04 | El sistema debe permitir agregar notas especiales a productos y comandas | Media | Baja | Pendiente |
| RF-C05 | El sistema debe calcular automáticamente subtotales y totales | Alta | Baja | Pendiente |
| RF-C06 | El sistema debe enviar comandas a cocina en tiempo real | Alta | Media | Pendiente |
| RF-C07 | El sistema debe permitir visualizar el estado de las comandas | Alta | Baja | Pendiente |
| RF-C08 | El sistema debe permitir modificar o cancelar comandas pendientes | Alta | Media | Pendiente |
| RF-C09 | El sistema debe funcionar offline y sincronizar cuando se recupere la conexión | Media | Alta | Pendiente |

### Módulo: Cocina

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RF-K01 | El sistema debe mostrar cola de comandas pendientes en cocina | Alta | Media | Pendiente |
| RF-K02 | El sistema debe permitir actualizar el estado de las comandas y productos | Alta | Media | Pendiente |
| RF-K03 | El sistema debe notificar a meseros cuando las comandas estén listas | Alta | Media | Pendiente |
| RF-K04 | El sistema debe mostrar tiempos estimados de preparación | Media | Baja | Pendiente |
| RF-K05 | El sistema debe permitir priorizar comandas | Media | Baja | Pendiente |
| RF-K06 | El sistema debe permitir marcar ingredientes o productos como no disponibles | Media | Baja | Pendiente |
| RF-K07 | El sistema debe mostrar historial de comandas completadas | Baja | Baja | Pendiente |

### Módulo: Pagos

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RF-G01 | El sistema debe permitir generar cuentas para mesas | Alta | Media | Pendiente |
| RF-G02 | El sistema debe soportar diferentes métodos de pago (efectivo, tarjeta) | Alta | Media | Pendiente |
| RF-G03 | El sistema debe permitir dividir cuentas | Alta | Alta | Pendiente |
| RF-G04 | El sistema debe generar comprobantes de pago | Alta | Media | Pendiente |
| RF-G05 | El sistema debe permitir aplicar descuentos | Media | Baja | Pendiente |
| RF-G06 | El sistema debe soportar pagos parciales | Media | Media | Pendiente |
| RF-G07 | El sistema debe registrar propinas | Baja | Baja | Pendiente |
| RF-G08 | El sistema debe funcionar offline para procesar pagos en efectivo | Media | Media | Pendiente |

### Módulo: Inventario

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RF-I01 | El sistema debe permitir gestionar ingredientes en inventario | Alta | Media | Pendiente |
| RF-I02 | El sistema debe registrar movimientos de inventario (entradas/salidas) | Alta | Media | Pendiente |
| RF-I03 | El sistema debe alertar sobre niveles bajos de stock | Alta | Baja | Pendiente |
| RF-I04 | El sistema debe permitir ajustes de inventario | Media | Baja | Pendiente |
| RF-I05 | El sistema debe descontar automáticamente ingredientes al preparar productos | Media | Alta | Pendiente |
| RF-I06 | El sistema debe permitir generar reportes de consumo de inventario | Media | Media | Pendiente |
| RF-I07 | El sistema debe soportar lectura de códigos de barras para inventario | Baja | Media | Pendiente |

### Módulo: Reservaciones

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RF-R01 | El sistema debe permitir crear, modificar y cancelar reservaciones | Media | Media | Pendiente |
| RF-R02 | El sistema debe verificar disponibilidad de mesas para reservaciones | Media | Media | Pendiente |
| RF-R03 | El sistema debe enviar confirmaciones y recordatorios por email/SMS | Baja | Media | Pendiente |
| RF-R04 | El sistema debe mostrar calendario visual de reservaciones | Media | Media | Pendiente |
| RF-R05 | El sistema debe permitir gestionar la llegada de clientes con reservación | Media | Baja | Pendiente |
| RF-R06 | El sistema debe bloquear mesas reservadas en el horario correspondiente | Media | Baja | Pendiente |

### Módulo: Reportes y Analítica

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RF-RA01 | El sistema debe generar reportes de ventas | Alta | Media | Pendiente |
| RF-RA02 | El sistema debe generar reportes de inventario | Media | Media | Pendiente |
| RF-RA03 | El sistema debe generar reportes de productividad | Media | Media | Pendiente |
| RF-RA04 | El sistema debe permitir filtrar reportes por fecha, categoría, etc. | Alta | Baja | Pendiente |
| RF-RA05 | El sistema debe mostrar gráficos de rendimiento | Media | Media | Pendiente |
| RF-RA06 | El sistema debe permitir exportar reportes en diferentes formatos | Media | Baja | Pendiente |
| RF-RA07 | El sistema debe permitir programar generación automática de reportes | Baja | Media | Pendiente |
| RF-RA08 | El sistema debe mostrar un dashboard con KPIs principales | Alta | Alta | Pendiente |

## Requisitos No Funcionales

### Rendimiento

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RNF-P01 | El tiempo de respuesta para operaciones comunes debe ser menor a 2 segundos | Alta | Media | Pendiente |
| RNF-P02 | El sistema debe soportar al menos 50 usuarios concurrentes | Alta | Media | Pendiente |
| RNF-P03 | Las actualizaciones en tiempo real deben tener una latencia máxima de 3 segundos | Alta | Media | Pendiente |
| RNF-P04 | El sistema debe escalar horizontalmente para soportar múltiples restaurantes | Baja | Alta | Pendiente |

### Disponibilidad y Fiabilidad

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RNF-D01 | El sistema debe estar disponible 99.9% del tiempo | Alta | Alta | Pendiente |
| RNF-D02 | El sistema debe funcionar offline con capacidades limitadas | Alta | Alta | Pendiente |
| RNF-D03 | El sistema debe sincronizar datos automáticamente al restaurar conexión | Alta | Alta | Pendiente |
| RNF-D04 | El sistema debe realizar copias de seguridad automáticas diarias | Alta | Media | Pendiente |
| RNF-D05 | El tiempo de recuperación ante fallos debe ser menor a 1 hora | Media | Alta | Pendiente |

### Seguridad

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RNF-S01 | Todas las contraseñas deben almacenarse con hash y salt | Alta | Baja | Pendiente |
| RNF-S02 | Todas las comunicaciones deben ser cifradas (HTTPS) | Alta | Baja | Pendiente |
| RNF-S03 | El sistema debe implementar protección contra ataques comunes (XSS, CSRF, inyección SQL) | Alta | Media | Pendiente |
| RNF-S04 | El sistema debe registrar todos los intentos de inicio de sesión | Alta | Baja | Pendiente |
| RNF-S05 | El sistema debe cumplir con normativas de protección de datos (GDPR, etc.) | Media | Alta | Pendiente |
| RNF-S06 | Los datos sensibles de pago deben estar protegidos según estándares PCI DSS | Alta | Alta | Pendiente |

### Usabilidad

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RNF-U01 | La interfaz debe ser intuitiva y requerir mínima capacitación | Alta | Media | Pendiente |
| RNF-U02 | La aplicación móvil debe optimizarse para uso con una sola mano | Media | Media | Pendiente |
| RNF-U03 | El sistema debe proporcionar retroalimentación clara para todas las acciones | Alta | Baja | Pendiente |
| RNF-U04 | La interfaz debe ser accesible según WCAG 2.1 nivel AA | Baja | Alta | Pendiente |
| RNF-U05 | El sistema debe soportar múltiples idiomas | Baja | Media | Pendiente |

### Mantenibilidad y Portabilidad

| ID | Descripción | Prioridad | Complejidad | Estado |
|----|-------------|-----------|-------------|--------|
| RNF-M01 | El código debe seguir principios SOLID y patrones de diseño | Alta | Media | Pendiente |
| RNF-M02 | El sistema debe seguir una arquitectura limpia con separación de capas | Alta | Media | Pendiente |
| RNF-M03 | El sistema debe tener cobertura de pruebas unitarias mínima del 80% | Media | Alta | Pendiente |
| RNF-M04 | La aplicación móvil debe ejecutarse en Android 8.0+ e iOS 13.0+ | Alta | Media | Pendiente |
| RNF-M05 | El portal web debe ser compatible con los navegadores principales (Chrome, Firefox, Safari, Edge) | Alta | Media | Pendiente |
| RNF-M06 | La documentación técnica debe estar actualizada y ser comprensible | Media | Baja | Pendiente |

## Matriz de Trazabilidad

Esta sección mapea los requisitos funcionales con los casos de uso documentados.

| ID Requisito | Casos de Uso Relacionados |
|--------------|---------------------------|
| RF-A01, RF-A03, RF-A04, RF-A05 | CU-001: Iniciar Sesión |
| RF-M01, RF-M02, RF-M03, RF-M04, RF-M07 | CU-002: Gestionar Mesas |
| RF-C01, RF-C02, RF-C03, RF-C04, RF-C05, RF-C06, RF-C08 | CU-003: Tomar Comanda |
| RF-K01, RF-K02, RF-K03, RF-K04, RF-K05, RF-K06 | CU-004: Preparar Comanda en Cocina |
| RF-G01, RF-G02, RF-G03, RF-G04, RF-G05, RF-G06, RF-G07 | CU-005: Procesar Pago |
| RF-P01, RF-P02, RF-P03, RF-P04, RF-P05, RF-P07, RF-P08 | CU-006: Gestionar Productos |
| RF-I01, RF-I02, RF-I03, RF-I04, RF-I05 | CU-007: Gestionar Inventario |
| RF-RA01, RF-RA02, RF-RA03, RF-RA04, RF-RA05, RF-RA06, RF-RA07, RF-RA08 | CU-008: Generar Reportes |
| RF-R01, RF-R02, RF-R03, RF-R04, RF-R05, RF-R06 | CU-009: Gestionar Reservaciones |
| RF-A02, RF-A06, RF-A07, RF-A08 | CU-010: Administrar Usuarios | 