# Historias de Usuario - RestaurantePro

## Sistema de Comandas (App Móvil)

### HU-001: Gestión de Mesas
**Como** mesero  
**Quiero** visualizar el estado de todas las mesas del restaurante  
**Para** gestionar de forma eficiente el servicio a los clientes

**Criterios de aceptación:**
- Se muestran todas las mesas con su número y estado (Libre, Ocupada, Reservada)
- Se permite cambiar el estado de la mesa
- Se visualiza información de la comanda activa en cada mesa ocupada
- Se pueden filtrar mesas por estado

### HU-002: Toma de Comandas
**Como** mesero  
**Quiero** registrar los pedidos de los clientes en una mesa  
**Para** enviarlos a cocina y gestionar el servicio

**Criterios de aceptación:**
- Se puede crear una nueva comanda asociada a una mesa
- Se pueden agregar varios platos a la comanda con sus cantidades
- Se pueden agregar notas especiales por plato (ej. "sin cebolla")
- Se puede enviar la comanda a cocina
- Se puede editar una comanda antes de ser procesada por cocina

### HU-003: Gestión de Productos
**Como** administrador  
**Quiero** gestionar el catálogo de productos/platos  
**Para** mantener actualizada la carta del restaurante

**Criterios de aceptación:**
- Se pueden agregar nuevos platos con nombre, descripción, precio y categoría
- Se pueden editar platos existentes
- Se pueden desactivar platos temporalmente
- Se pueden categorizar los platos
- Se puede subir una imagen para cada plato

### HU-004: Seguimiento de Comandas
**Como** mesero/cocinero  
**Quiero** ver el estado de las comandas en tiempo real  
**Para** coordinar el servicio entre salón y cocina

**Criterios de aceptación:**
- Los cocineros ven las comandas nuevas en orden de llegada
- Los cocineros pueden marcar platos como "en preparación" y "listos"
- Los meseros reciben notificaciones cuando los platos están listos
- Se visualiza el tiempo transcurrido desde la creación de cada comanda

## Sistema de Analítica (Web)

### HU-005: Dashboard de Ventas
**Como** propietario/gerente  
**Quiero** visualizar un resumen de las ventas  
**Para** analizar el rendimiento del negocio

**Criterios de aceptación:**
- Se muestra el total de ventas por día/semana/mes/año
- Se visualizan gráficos comparativos entre periodos
- Se puede filtrar por rangos de fechas
- Se muestran indicadores clave (ticket promedio, clientes atendidos)

### HU-006: Análisis de Productos
**Como** propietario/gerente  
**Quiero** saber qué productos se venden más  
**Para** optimizar el menú y el inventario

**Criterios de aceptación:**
- Se muestra ranking de productos más vendidos
- Se visualiza el margen de ganancia por producto
- Se identifica qué productos suelen combinarse en los pedidos
- Se generan reportes exportables

### HU-007: Gestión de Usuarios
**Como** administrador  
**Quiero** gestionar los usuarios del sistema  
**Para** controlar el acceso según roles

**Criterios de aceptación:**
- Se pueden crear usuarios con diferentes roles (Admin, Mesero, Cocinero)
- Se pueden modificar permisos por usuario
- Se puede desactivar/reactivar usuarios
- Se mantiene un registro de actividad por usuario

### HU-008: Control de Inventario
**Como** gerente  
**Quiero** llevar un control del inventario  
**Para** gestionar compras y minimizar pérdidas

**Criterios de aceptación:**
- Se registran ingredientes con su stock
- Se actualiza automáticamente el inventario con cada comanda
- Se generan alertas de stock bajo
- Se puede registrar entrada de nuevos productos 