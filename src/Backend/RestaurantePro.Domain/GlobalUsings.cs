// Common .NET imports
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

// Domain core imports - Base
global using RestaurantePro.Domain.Core.Base;
global using RestaurantePro.Domain.Core.Base.Interfaces;
// global using RestaurantePro.Domain.Core.Base.Exceptions;

// Domain core imports - SharedKernel
global using RestaurantePro.Domain.Core.SharedKernel;
global using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
global using RestaurantePro.Domain.Core.SharedKernel.Interfaces;

// Domain core imports - BoundedContexts
global using RestaurantePro.Domain.Core.BoundedContexts;

// Domain core imports - Productos
global using RestaurantePro.Domain.Core.Productos.Entities;
global using RestaurantePro.Domain.Core.Productos.ValueObjects;
// global using RestaurantePro.Domain.Core.Productos.Enums;
global using RestaurantePro.Domain.Core.Productos.Events;
global using RestaurantePro.Domain.Core.Productos.Events.Producto;

// Domain Operaciones - Comandas
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;
global using RestaurantePro.Domain.Operaciones.Comandas.Enums;
global using RestaurantePro.Domain.Operaciones.Comandas.Events;
global using RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda;
global using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;

// Domain Operaciones - Reservaciones
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
// global using RestaurantePro.Domain.Operaciones.Reservaciones.ValueObjects;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Events;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events;

// Domain Comercial - Clientes
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
global using RestaurantePro.Domain.Comercial.Clientes.Enums;
global using RestaurantePro.Domain.Comercial.Clientes.Events;
global using RestaurantePro.Domain.Comercial.Clientes.Events.Cliente;
global using RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion;
global using RestaurantePro.Domain.Comercial.Clientes.Interfaces;

// Domain Inventario - Ingredientes
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
global using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
global using RestaurantePro.Domain.Inventario.Ingredientes.Events;

// Domain Inventario - Ingredientes - Movimientos
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Events;

// Domain Inventario - Compras - OrdenesCompra
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events;

// Domain Proveedores
global using RestaurantePro.Domain.Proveedores.Entidades;
