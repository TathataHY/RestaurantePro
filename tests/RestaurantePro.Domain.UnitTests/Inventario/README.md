# Tests del Módulo de Inventario

## Descripción
Este directorio contiene todas las pruebas unitarias para el módulo de Inventario, verificando la correcta implementación del dominio según los requisitos y reglas de negocio establecidas. Estas pruebas siguen la metodología TDD (Test-Driven Development).

## Estructura del Módulo de Tests

```
Inventario/
├── Ingredientes/                # Tests para gestión de ingredientes
│   ├── Entities/                # Tests para entidades de ingredientes
│   └── Movimientos/             # Tests para movimientos de inventario
│       └── Entities/            # Tests para entidades de movimientos
├── Compras/                     # Tests para gestión de compras
│   └── OrdenesCompra/           # Tests para órdenes de compra
│       ├── Entities/            # Tests para entidades de órdenes
│       └── Repositories/        # Tests para repositorios de órdenes
└── README.md                    # Este archivo
```

## Submódulos Principales

### Ingredientes
Pruebas unitarias para la gestión de ingredientes y control de stock:
- Creación de ingredientes
- Operaciones de stock
- Movimientos de inventario (entradas/salidas)
- Validación de eventos de dominio

### Compras
Pruebas unitarias para la gestión de adquisiciones a proveedores:
- Creación y flujo de órdenes de compra
- Manejo de ítems en órdenes
- Interacción con repositorios
- Validación de eventos de dominio

## Enfoque TDD Aplicado
1. Escribir primero la prueba para el comportamiento esperado
2. Verificar que la prueba falla (por no estar implementado el código)
3. Implementar el código mínimo para pasar la prueba
4. Refactorizar el código manteniendo los tests exitosos

## Cobertura de Pruebas
El objetivo es mantener una cobertura superior al 90% para:
- Entidades del dominio
- Reglas de negocio y validaciones
- Generación de eventos de dominio
- Operaciones de repositorio
