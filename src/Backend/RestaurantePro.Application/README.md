# Capa de Aplicación - RestaurantePro

Esta capa organiza la lógica de aplicación en módulos principales que reflejan las áreas funcionales del negocio.

## Estructura de Módulos Principales

```
Application/
├── Comercial/                # Gestión comercial y de clientes
│   ├── Clientes/             # Gestión de clientes
│   ├── Promociones/          # Gestión de promociones
│   └── Fidelizacion/         # Programa de fidelización
│
├── Operaciones/              # Operaciones del restaurante
│   ├── Comandas/             # Gestión de comandas y pedidos
│   ├── Mesas/                # Gestión de mesas
│   └── Reservaciones/        # Sistema de reservaciones
│
├── Inventario/               # Gestión de inventario
│   ├── Productos/            # Productos en inventario
│   ├── Movimientos/          # Entradas y salidas
│   └── Compras/              # Órdenes de compra
│
├── Catalogo/                 # Catálogo de productos
│   ├── Productos/            # Productos y platillos
│   ├── Categorias/           # Categorización de productos
│   └── Ingredientes/         # Ingredientes base
│
├── Finanzas/                 # Gestión financiera
│   ├── Pagos/                # Procesamiento de pagos
│   ├── Facturacion/          # Emisión de facturas
│   └── Contabilidad/         # Registros contables
│
├── Common/                   # Componentes compartidos
│   ├── Interfaces/           # Interfaces comunes
│   ├── DTOs/                 # Objetos de transferencia de datos
│   ├── Behaviors/            # Comportamientos reutilizables
│   └── Exceptions/           # Excepciones de aplicación
│
└── Config/                   # Configuración de la aplicación
    ├── Mappings/             # Configuración de mapeos
    ├── DependencyInjection/  # Registro de servicios
    └── Validation/           # Validadores
```

## Organización interna de cada módulo

Cada módulo sigue una organización consistente:

```
ModuloPrincipal/
├── SubModulo/
│   ├── Commands/             # Operaciones que modifican estado
│   │   ├── Create/           # Comandos de creación
│   │   ├── Update/           # Comandos de actualización
│   │   └── Delete/           # Comandos de eliminación
│   │
│   ├── Queries/              # Operaciones de consulta 
│   │   ├── GetById/          # Consulta por ID
│   │   ├── GetList/          # Consulta de listas
│   │   └── GetReport/        # Consultas para reportes
│   │
│   ├── DTOs/                 # Objetos de transferencia de datos
│   ├── Validators/           # Validadores de comandos y consultas
│   └── Events/               # Manejadores de eventos
```

## Ejemplo de uso

Para crear un nuevo cliente:

```csharp
// Desde un controlador o API endpoint
await mediator.Send(new CreateClienteCommand 
{
    Nombre = "Juan",
    Apellido = "Pérez",
    Email = "juan@ejemplo.com",
    Telefono = "555-1234"
});
``` 