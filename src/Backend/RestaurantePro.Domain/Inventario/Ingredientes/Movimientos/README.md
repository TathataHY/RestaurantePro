# Módulo de Movimientos de Inventario

## Descripción
Este módulo gestiona los movimientos de inventario (entradas y salidas) de ingredientes en el restaurante. 
Registra todas las transacciones relacionadas con el stock, permitiendo mantener un historial 
detallado de todos los cambios y auditoría de inventario.

## Entidades Principales
- **MovimientoInventario**: Registra cada transacción de entrada o salida de un ingrediente.

## Enumerados
- **TipoMovimientoInventario**: Define los tipos de movimientos (Entrada, Salida).

## Eventos de Dominio
- **MovimientoRegistradoEvent**: Se dispara cuando se registra un nuevo movimiento.
- **MovimientoAplicadoEvent**: Se dispara cuando un movimiento se aplica al stock.

## Interfaces
- **IMovimientoInventarioRepository**: Repositorio para persistir y recuperar movimientos.

## Casos de Uso Principales
1. Registrar entrada de ingredientes por compra
2. Registrar salida de ingredientes por uso en comandas
3. Registrar ajustes de inventario (mermas, caducidades)
4. Consultar historial de movimientos por ingrediente
5. Generar reportes de movimientos por período 