# Módulo Core/Productos - Domain

Este módulo contiene las entidades, value objects y eventos relacionados con el catálogo de productos del restaurante, siguiendo los principios de Domain-Driven Design (DDD).

## Estructura del Módulo

```
Productos/
├── Entities/                 # Entidades del dominio
│   ├── Producto.cs           # Entidad principal de producto
│   └── CategoriaProducto.cs  # Entidad para categorías de productos
│
├── ValueObjects/             # Value Objects específicos
│   ├── PrecioProducto.cs     # VO para precios de productos
│   └── DescripcionProducto.cs # VO para descripciones
│
└── Events/                   # Eventos de dominio
    ├── ProductoCreado.cs
    ├── ProductoActualizado.cs
    ├── ProductoActivado.cs
    └── ProductoDesactivado.cs
```

## Contexto del Módulo

Este módulo define el catálogo de productos que estarán disponibles para ser ordenados por los clientes. Forma parte del Core porque es un concepto compartido entre varios contextos (Operaciones, Inventario, etc.).

Incluye:

- Definición completa de productos con sus atributos
- Categorización de productos
- Precios y su comportamiento
- Gestión de estados (activo/inactivo)
- Eventos de dominio relacionados con cambios en productos

## Principios implementados

1. **Entidades**: Productos y categorías con identidad propia
2. **Value Objects**: Conceptos inmutables como precios y descripciones
3. **Eventos de dominio**: Notificaciones sobre cambios importantes
4. **Invariantes de dominio**: Reglas de negocio como precios válidos

## Casos de Uso Principales

- Creación de nuevos productos
- Actualización de información de productos
- Cambios de precio
- Activación/desactivación de productos
- Categorización de productos

## Relación con otros módulos

- Utilizado por **Operaciones/Comandas** para añadir productos a las órdenes
- Consultado por **Comercial** para presentar productos a los clientes
- Relacionado con **Inventario** para control de stock
- Utiliza conceptos del **SharedKernel** como objetos de valor monetarios 