# Módulo de Ingredientes

## Descripción
Este módulo gestiona los ingredientes utilizados en el restaurante, permitiendo el control de stock, 
unidades de medida y gestión de umbrales mínimos para reabastecimiento.

## Entidades Principales
- **Ingrediente**: Representa una materia prima utilizada en la preparación de productos.

## Enumerados
- **UnidadMedida**: Define las diferentes unidades de medida para los ingredientes (Kilogramo, Gramo, Litro, etc.).

## Eventos de Dominio
- **IngredienteCreadoEvent**: Se dispara cuando se crea un nuevo ingrediente.
- **IngredienteActivadoEvent**: Se dispara cuando se activa un ingrediente.
- **IngredienteDesactivadoEvent**: Se dispara cuando se desactiva un ingrediente.
- **StockActualizadoEvent**: Se dispara cuando cambia el stock de un ingrediente.
- **StockBajoMinimoEvent**: Se dispara cuando el stock cae por debajo del umbral mínimo.

## Interfaces
- **IIngredienteRepository**: Repositorio para persistir y recuperar ingredientes.

## Submódulos
- **Movimientos**: Gestiona las entradas y salidas de ingredientes del inventario.

## Casos de Uso Principales
1. Crear y gestionar nuevos ingredientes
2. Activar o desactivar ingredientes
3. Monitorear niveles de stock
4. Generar alertas de stock bajo
5. Definir y modificar umbrales mínimos de stock 