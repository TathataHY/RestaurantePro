# Módulo de Órdenes de Compra

## Descripción
Este módulo gestiona el proceso de pedidos a proveedores, desde la creación de órdenes de compra
hasta su recepción y cierre. Permite realizar un seguimiento completo del ciclo de vida de cada pedido.

## Entidades Principales
- **OrdenCompra**: Representa un pedido a un proveedor.
- **ItemOrdenCompra**: Representa un producto específico dentro de una orden de compra.

## Enumerados
- **EstadoOrdenCompra**: Define los diferentes estados de una orden (Creada, Enviada, Recibida, Cancelada).

## Eventos de Dominio
- **OrdenCompraCreada**: Se dispara cuando se crea una nueva orden de compra.
- **OrdenCompraEnviada**: Se dispara cuando una orden se envía al proveedor.
- **OrdenCompraRecibida**: Se dispara cuando se recibe una orden de compra.
- **OrdenCompraCancelada**: Se dispara cuando se cancela una orden de compra.
- **ItemOrdenCompraAgregado**: Se dispara cuando se agrega un ítem a la orden.
- **ItemOrdenCompraEliminado**: Se dispara cuando se elimina un ítem de la orden.

## Interfaces
- **IOrdenCompraRepository**: Repositorio para persistir y recuperar órdenes de compra.

## Casos de Uso Principales
1. Crear nuevas órdenes de compra
2. Agregar o eliminar ítems a una orden
3. Enviar órdenes a proveedores
4. Recibir y procesar órdenes
5. Cancelar órdenes
6. Seguimiento del estado de órdenes pendientes
7. Generar reportes de órdenes por período y proveedor 