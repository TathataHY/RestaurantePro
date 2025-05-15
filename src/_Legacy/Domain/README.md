# RestaurantePro.Domain - Estructura Modular por Categorías

Este directorio contiene la capa de dominio del sistema RestaurantePro, organizada siguiendo los principios de Screaming Architecture (Arquitectura Gritante) junto con Clean Architecture, agrupada en categorías funcionales.

## Estructura de carpetas

La estructura está organizada por categorías principales que agrupan módulos de negocio relacionados:

```
Domain/
├── Core/                         # Componentes esenciales del sistema
│   ├── Base/                     # Componentes base compartidos
│   ├── Productos/                # Catálogo de productos/platillos
│   └── Usuarios/                 # Usuarios del sistema
│
├── Operaciones/                  # Operaciones diarias del restaurante
│   ├── Comandas/                 # Gestión de comandas y pedidos
│   └── Reservaciones/            # Gestión de reservaciones y mesas
│
├── Inventario/                   # Gestión de stock y suministros
│   ├── Inventario/               # Control de inventario
│   ├── Ingredientes/             # Gestión de ingredientes
│   └── Compras/                  # Órdenes de compra
│
├── Comercial/                    # Aspectos comerciales y financieros
│   ├── Clientes/                 # Gestión de clientes y fidelización
│   ├── Pagos/                    # Sistema de pagos
│   └── Promociones/              # Sistema de promociones
│
└── Proveedores/                  # Gestión de proveedores
```

Cada módulo mantiene su estructura interna con:
- Entities/: Entidades de dominio
- Enums/: Enumeraciones
- Interfaces/: Interfaces específicas del módulo

## Beneficios de esta estructura

1. **Organización conceptual clara**: La estructura refleja las áreas funcionales del negocio
2. **Mayor cohesión**: Los componentes relacionados están agrupados en diferentes niveles
3. **Límites contextuales definidos**: Cada módulo principal representa un contexto del negocio
4. **Escalabilidad**: Facilita agregar nuevas funcionalidades en el contexto adecuado
5. **Facilita TDD**: Estructura ideal para aplicar desarrollo guiado por pruebas por contextos
