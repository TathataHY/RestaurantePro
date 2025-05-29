# RestaurantePro

Sistema de gestión completo para restaurantes, desarrollado con tecnologías modernas .NET.

## Visión General

RestaurantePro es un conjunto de aplicaciones diseñadas para la gestión integral de un restaurante:

- **Aplicación Móvil:** Para la gestión de comandas, mesas y pedidos
- **Portal Web:** Para análisis, reportes y administración

## Tecnologías Utilizadas

- **App Móvil:** .NET MAUI
- **Web Analítica:** Blazor WebAssembly
- **Backend API:** ASP.NET Core 8
- **Base de datos:** SQL Server / PostgreSQL
- **ORM:** Entity Framework Core 8
- **Comunicación en tiempo real:** SignalR
- **Arquitectura:** Clean Architecture con CQRS y MediatR

## Estructura del Proyecto

```
RestaurantePro/
├── src/                       # Código fuente
│   ├── RestaurantePro.Api/    # Backend API
│   │   └── Controllers/       # Controladores REST
│   ├── RestaurantePro.Application/ # Lógica de aplicación
│   │   ├── Common/            # 🔧 Componentes compartidos entre contextos
│   │   │   ├── Interfaces/    # Interfaces específicas de Application
│   │   │   │   └── IUsuarioActualService.cs    ✅ IMPLEMENTADO
│   │   │   └── Models/        # Modelos comunes
│   │   └── Features/          # Funcionalidades del sistema
│   │       ├── Categorias/    # Gestión de categorías
│   │       ├── Clientes/      # Gestión de clientes y fidelización
│   │       ├── Comandas/      # Gestión de comandas
│   │       ├── Inventario/    # Sistema de inventario
│   │       │   ├── Commands/  # Comandos (escritura)
│   │       │   ├── Queries/   # Consultas (lectura)
│   │       │   └── Dtos/      # Objetos de transferencia de datos
│   │       ├── Mesas/         # Gestión de mesas
│   │       ├── OrdenesCompra/ # Gestión de órdenes de compra
│   │       ├── Pagos/         # Gestión de pagos
│   │       ├── Productos/     # Gestión de productos/platillos
│   │       ├── Promociones/   # Gestión de promociones
│   │       ├── Reportes/      # Generación de reportes
│   │       ├── Reservaciones/ # Sistema de reservaciones
│   │       └── Usuarios/      # Gestión de usuarios
│   ├── RestaurantePro.Mobile/ # App móvil (.NET MAUI)
│   ├── RestaurantePro.Web/    # Portal web (Blazor)
│   ├── RestaurantePro.Domain/ # Entidades y lógica de dominio
│   │   ├── Entities/         # Entidades de dominio
│   │   ├── Enums/            # Enumeraciones
│   │   ├── Events/           # Eventos de dominio
│   │   └── Exceptions/       # Excepciones de dominio 
│   └── RestaurantePro.Infrastructure/ # Persistencia y servicios
│       ├── Identity/          # Autenticación y autorización
│       ├── Persistence/       # Configuración de DbContext
│       └── Services/          # Implementación de servicios
├── tests/                     # Pruebas
├── tools/                     # Scripts y herramientas
└── docs/                      # Documentación
    ├── analisis-diseno/       # Análisis y diseño
    ├── arquitectura/          # Arquitectura del sistema
    ├── requisitos/            # Requisitos y casos de uso
    ├── diagramas/             # Diagramas del sistema
    ├── prototipos/            # Diseños de UI
    └── manuales/              # Guías y manuales
```

## Módulos Implementados

### Sistema de Gestión de Inventario
- **Características:**
  - Gestión de ingredientes y productos
  - Registro de movimientos de inventario (entradas, salidas, mermas)
  - Ajustes de inventario con trazabilidad
  - Reportes de estado de inventario y alertas de stock mínimo
  - Cálculo automático de costos promedios ponderados

### Sistema de Proveedores
- **Características:**
  - Catálogo de proveedores
  - Categorización de proveedores
  - Vinculación de ingredientes con proveedores y precios
  - Historial de pedidos por proveedor

### Sistema de Órdenes de Compra
- **Características:**
  - Creación de órdenes de compra a proveedores
  - Seguimiento del estado de órdenes (pendiente, parcial, completada)
  - Recepción parcial o total de órdenes
  - Actualización automática del inventario al recibir productos
  - Registro de inconformidades en la recepción

### Sistema de Clientes y Fidelización
- **Características:**
  - Registro y gestión de clientes
  - Programa de lealtad con tarjetas de puntos
  - Acumulación de puntos por consumo
  - Canje de puntos por promociones o descuentos
  - Historial de transacciones por cliente

### Sistema de Reservaciones
- **Características:**
  - Gestión de reservaciones de mesas
  - Diferentes estados de reservación (pendiente, confirmada, completada, etc.)
  - Asignación de mesas según capacidad y disponibilidad
  - Historial de reservaciones por cliente

## API Endpoints

El sistema expone los siguientes endpoints principales:

### Inventario
- `GET /api/inventario` - Lista de inventario con filtros
- `GET /api/inventario/{id}` - Detalle de un ítem de inventario
- `PUT /api/inventario/{id}/actualizar` - Actualizar un ítem de inventario
- `POST /api/inventario/ajustar` - Realizar ajuste de inventario
- `POST /api/inventario/movimiento` - Registrar movimiento (entrada/salida)
- `GET /api/inventario/movimientos` - Historial de movimientos
- `GET /api/inventario/alertas` - Ítems con stock bajo

### Proveedores
- `GET /api/proveedores` - Lista de proveedores
- `GET /api/proveedores/{id}` - Detalle de un proveedor
- `POST /api/proveedores` - Crear nuevo proveedor
- `PUT /api/proveedores/{id}` - Actualizar proveedor
- `DELETE /api/proveedores/{id}` - Eliminar proveedor
- `GET /api/proveedores/{id}/ingredientes` - Ingredientes de un proveedor

### Órdenes de Compra
- `GET /api/ordenescompra` - Lista de órdenes de compra
- `GET /api/ordenescompra/{id}` - Detalle de una orden
- `POST /api/ordenescompra` - Crear nueva orden
- `PUT /api/ordenescompra/{id}` - Actualizar orden
- `DELETE /api/ordenescompra/{id}` - Eliminar orden
- `POST /api/ordenescompra/{id}/recibir` - Registrar recepción de productos
- `GET /api/ordenescompra/pendientes` - Órdenes pendientes
- `GET /api/ordenescompra/proveedor/{proveedorId}` - Órdenes por proveedor

## Próximos Desarrollos

- Sistema de comandas con asignación automática a cocina
- Integración con periféricos (impresoras térmicas, PDV)
- Aplicación móvil para seguimiento de pedidos
- Dashboard en tiempo real de operaciones
- Sistema de notificaciones

## Instalación y Configuración

*Instrucciones detalladas estarán disponibles cuando el proyecto avance en desarrollo.*

## Equipo de Desarrollo

*En desarrollo*

## Licencia

Este proyecto está bajo licencia privada.

## Contacto

*En desarrollo*

## Common/Interfaces

```
RestaurantePro/
├── Common/                   # 🔧 Componentes compartidos entre contextos
│   ├── Interfaces/           # Interfaces específicas de Application
│   │   └── IUsuarioActualService.cs    ✅ IMPLEMENTADO
│   │   # 📝 Nota: Las demás interfaces están en Domain:
│   │   # - IRepository<T>, IUnitOfWork (SharedKernel/Interfaces)
│   │   # - IDateTimeService (Base/Services)  
│   │   # - IProductoRepository, IClienteRepository, etc. (por contexto)
│   │
│   ├── DTOs/                 # DTOs base y compartidos
│   │   ├── PaginatedList.cs
│   │   ├── FilterRequest.cs
│   │   ├── BaseDto.cs
│   │   └── PagedResult.cs
│   │
│   ├── Behaviors/            # Comportamientos de MediatR
│   │   ├── ValidationBehavior.cs      ✅ IMPLEMENTADO
│   │   ├── LoggingBehavior.cs         ✅ IMPLEMENTADO
│   │   ├── CachingBehavior.cs         🔄 PENDIENTE
│   │   └── PerformanceBehavior.cs     🔄 PENDIENTE
``` 