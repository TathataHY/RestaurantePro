# Sistema de Gestión de Inventario y Compras

## Visión General

El Sistema de Gestión de Inventario y Compras de RestaurantePro proporciona una solución completa para la administración de ingredientes, control de stock, relación con proveedores y proceso de órdenes de compra. 

## Estructura del Sistema

El sistema está organizado siguiendo el patrón CQRS (Command Query Responsibility Segregation) dentro de Clean Architecture:

### Capa de Dominio
- **Entidades:**
  - `Inventario`: Registro de ingredientes en stock
  - `MovimientoInventario`: Historial de cambios en el inventario
  - `Proveedor`: Datos de proveedores
  - `ProveedorIngrediente`: Relación entre proveedores e ingredientes
  - `OrdenCompra`: Órdenes de compra a proveedores
  - `DetalleOrdenCompra`: Elementos individuales en una orden

- **Enumeraciones:**
  - `TipoMovimiento`: Tipos de movimientos (Entrada, Salida, Ajuste, Merma)
  - `EstadoOrdenCompra`: Estados de órdenes (Pendiente, Parcial, Completada, Cancelada)

### Capa de Aplicación
- **DTOs:**
  - `InventarioDto`: Datos de visualización de inventario
  - `MovimientoDto`: Datos simplificados de movimientos
  - `ProveedorDto`: Datos de proveedores
  - `OrdenCompraDto` y `DetalleOrdenCompraDto`: Datos de órdenes de compra

- **Comandos:**
  - `AjustarInventario`: Ajusta la cantidad de un ingrediente en inventario
  - `RegistrarMovimiento`: Registra entradas, salidas o mermas de inventario
  - `CrearOrdenCompra`: Crea una nueva orden a un proveedor
  - `RecibirOrdenCompra`: Registra la recepción de productos

- **Consultas:**
  - `ObtenerInventario`: Lista el inventario con filtros
  - `ObtenerMovimientos`: Historial de movimientos con filtros
  - `ObtenerOrdenesCompra`: Lista de órdenes de compra

### Capa de API
- **Controladores:**
  - `InventarioController`: Endpoints de gestión de inventario
  - `ProveedoresController`: Endpoints de gestión de proveedores
  - `OrdenesCompraController`: Endpoints de gestión de órdenes

## Flujos Principales

### 1. Registro de Inventario
- Los registros de inventario se crean automáticamente al:
  - Registrar una entrada manual de ingredientes
  - Recibir productos de una orden de compra

### 2. Movimientos de Inventario
- **Tipos de movimientos:**
  - **Entrada**: Incrementa el stock (recepción de compras, ajustes positivos)
  - **Salida**: Reduce el stock (consumo por comandas)
  - **Merma**: Reduce el stock (pérdidas, caducidad)
  - **Ajuste**: Modifica el stock (inventarios físicos)

- Todos los movimientos quedan registrados con:
  - Usuario que realizó el movimiento
  - Fecha y hora
  - Cantidades antes y después
  - Motivo y referencias

### 3. Gestión de Proveedores
- Permite administrar proveedores con sus datos de contacto
- Vincula ingredientes específicos a cada proveedor
- Registra precios y condiciones por proveedor-ingrediente

### 4. Ciclo de Órdenes de Compra
1. **Creación de Orden**:
   - Selección de proveedor
   - Agregado de ingredientes, cantidades y precios
   - Cálculo automático de subtotales, impuestos y totales

2. **Seguimiento de Estado**:
   - Pendiente: Orden creada pero sin recepción
   - Parcial: Recepción parcial de productos
   - Completada: Todos los productos recibidos
   - Cancelada: Orden anulada

3. **Recepción de Productos**:
   - Registro de cantidades recibidas
   - Actualización automática del inventario
   - Registro de inconformidades
   - Actualización del estado de la orden

## Validaciones Implementadas

- Control de stock mínimo antes de salidas
- Recálculo de costo promedio ponderado en entradas
- Verificación de relación proveedor-ingrediente en órdenes
- Validaciones de cantidades positivas y datos requeridos

## Características de Seguridad

- Políticas de autorización basadas en roles:
  - `RequiereEmpleado`: Para consultas básicas
  - `RequiereGerente`: Para ajustes y operaciones críticas
  - `RequiereAdministrador`: Para eliminaciones

## Próximas Mejoras

- Dashboard con alertas de stock mínimo
- Planificación automática de compras basadas en consumo histórico
- Codificación por código de barras/QR para ingredientes
- Sistema de rotación FIFO/FEFO para control de caducidades 