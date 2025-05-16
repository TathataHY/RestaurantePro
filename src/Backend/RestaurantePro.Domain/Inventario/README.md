# Módulo de Inventario

## Descripción
Este módulo gestiona todo lo relacionado con el control de existencias e insumos del restaurante,
desde el registro y seguimiento de ingredientes hasta la gestión de compras y movimientos de stock.
Permite mantener un control preciso de las materias primas disponibles para la operación del negocio.

## Submódulos
- **Ingredientes**: Gestión de materias primas y sus niveles de stock.
  - **Movimientos**: Control de entradas y salidas de ingredientes.
- **Compras**: Gestión de procesos de adquisición.
  - **OrdenesCompra**: Ciclo de vida de pedidos a proveedores.

## Contexto en DDD
Este módulo representa el Bounded Context de "Inventario" según el Context Map del sistema,
con integraciones hacia los contextos de Catálogo y Proveedores.

## Valor de Negocio
- Control preciso de costos de insumos
- Prevención de pérdidas por mermas o caducidades
- Optimización de procesos de compra
- Garantía de disponibilidad de insumos para la operación
- Trazabilidad completa de movimientos de inventario

## Reglas de Negocio Principales
1. Todo movimiento de inventario debe registrarse con motivo y responsable
2. El stock nunca puede ser negativo
3. Se deben generar alertas cuando los niveles caen bajo mínimos
4. Las órdenes de compra siguen un flujo de estados predefinido
5. Toda recepción de mercancía debe estar asociada a una orden de compra
