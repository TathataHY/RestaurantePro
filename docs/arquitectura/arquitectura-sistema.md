# Arquitectura del Sistema RestaurantePro

## Visión General

RestaurantePro es un sistema completo para la gestión de restaurantes que utiliza tecnologías modernas de .NET. Está compuesto por una aplicación móvil para la gestión de comandas y una aplicación web para análisis y administración.

## Componentes Principales

### 1. Aplicación Móvil (.NET MAUI)

Aplicación multiplataforma para la gestión de comandas, mesas y pedidos por parte del personal del restaurante.

**Características técnicas:**
- Framework: .NET MAUI
- Patrón: MVVM con CommunityToolkit.Mvvm
- Almacenamiento local: SQLite para trabajo offline
- Notificaciones en tiempo real: SignalR

### 2. Aplicación Web (Blazor WebAssembly)

Portal web para la visualización de analítica, administración del sistema y gestión de inventario.

**Características técnicas:**
- Framework: Blazor WebAssembly
- Componentes UI: MudBlazor
- Gráficos: ChartJS.Blazor
- Single Page Application (SPA)

### 3. Backend API (ASP.NET Core 8)

API RESTful centralizada que sirve a ambas aplicaciones cliente.

**Características técnicas:**
- Framework: ASP.NET Core 8 Web API
- Autenticación: JWT + Identity
- Documentación: Swagger/OpenAPI
- Comunicación en tiempo real: SignalR
- Patrones: CQRS + MediatR

### 4. Capa de Persistencia

Almacenamiento de datos y acceso mediante Entity Framework Core.

**Características técnicas:**
- ORM: Entity Framework Core 8
- Base de datos: SQL Server o PostgreSQL
- Migraciones para control de versiones
- Queries optimizadas con Dapper para lecturas

## Arquitectura de Capas

El sistema seguirá una arquitectura limpia (Clean Architecture) con las siguientes capas:

### Domain Layer

Contiene entidades de negocio, reglas de dominio, interfaces y lógica central.

- Entidades
- Interfaces
- Eventos de dominio
- Excepciones personalizadas
- Value Objects

### Application Layer

Implementa casos de uso que orquestan el flujo de datos y la lógica de negocio.

- Comandos y Queries (CQRS)
- Validaciones
- DTOs
- Interfaces de servicios
- Eventos de aplicación

### Infrastructure Layer

Proporciona implementaciones concretas para interfaces definidas en capas superiores.

- Repositorios
- Servicios externos
- Persistencia (EF Core)
- Implementaciones de infraestructura
- Seguridad

### Presentation Layer

Interfaces de usuario y APIs.

- API Controllers
- Middleware
- Filtros
- Configuración

## Flujo de Datos

```
+-------------+     +----------------+     +-----------------+
|             |     |                |     |                 |
|   Cliente   +---->+  Presentation  +---->+   Application   |
|   (UI/API)  |     |     Layer      |     |      Layer      |
|             |     |                |     |                 |
+-------------+     +----------------+     +-----------------+
                                                   |
                                                   v
                    +----------------+     +-----------------+
                    |                |     |                 |
                    | Infrastructure |<----+     Domain      |
                    |     Layer      |     |      Layer      |
                    |                |     |                 |
                    +----------------+     +-----------------+
```

## Comunicación en Tiempo Real

Para la comunicación en tiempo real entre diferentes componentes (cocina, meseros), se utilizará SignalR:

- Notificaciones de nuevas comandas
- Actualización del estado de los platos
- Alertas de mesas esperando atención
- Comunicación entre meseros y cocina

## Seguridad

El sistema implementará:

- Autenticación basada en JWT
- Autorización basada en roles (Admin, Mesero, Cocinero)
- Almacenamiento seguro de contraseñas con Identity
- HTTPS para toda comunicación
- Validación de entradas para prevenir inyecciones

## Escalabilidad

La arquitectura permitirá escalar el sistema en el futuro mediante:

- Diseño modular con bajo acoplamiento
- Separación clara de responsabilidades
- Uso de patrones que facilitan la extensión (CQRS, Repository)
- API bien documentada para posibles integraciones 