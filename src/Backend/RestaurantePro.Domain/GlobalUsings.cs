// .NET Base
global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Diagnostics;
global using System.Globalization;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;

// Domain Base
global using RestaurantePro.Domain.Core.Base;
global using RestaurantePro.Domain.Core.Base.Interfaces;
global using RestaurantePro.Domain.Core.Base.Services;

// Domain Base - Events
global using RestaurantePro.Domain.Core.Base.Events;
global using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
global using RestaurantePro.Domain.Core.Base.Events.Handlers;
global using RestaurantePro.Domain.Core.Base.Events.Registry;
global using RestaurantePro.Domain.Core.Base.Events.Extensions;
global using RestaurantePro.Domain.Core.Base.Events.Subscription;

// Domain core imports - SharedKernel
global using RestaurantePro.Domain.Core.SharedKernel;
global using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
global using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
global using RestaurantePro.Domain.Core.SharedKernel.Services;

// Domain core imports - SharedKernel Results
global using RestaurantePro.Domain.Core.SharedKernel.Results;

// Domain core imports - SharedKernel Validation
global using RestaurantePro.Domain.Core.SharedKernel.Validation;

// Domain core imports - SharedKernel Services - Cache
global using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
global using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Decorators;
global using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Telemetry;
global using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Strategy;
global using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Invalidation;

// Domain core imports - SharedKernel Services
global using RestaurantePro.Domain.Core.SharedKernel.Services;
global using RestaurantePro.Domain.Core.SharedKernel.Services.Notification;

// Domain core imports - Services
global using RestaurantePro.Domain.Core.Services;

// Domain core imports - BoundedContexts
global using RestaurantePro.Domain.Core.BoundedContexts;

// Domain core imports - Notificaciones
global using RestaurantePro.Domain.Core.Notificaciones.Entities;
global using RestaurantePro.Domain.Core.Notificaciones.Enums;
global using RestaurantePro.Domain.Core.Notificaciones.Events;
global using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
global using RestaurantePro.Domain.Core.Notificaciones.Services;

// Domain core imports - Usuarios
global using RestaurantePro.Domain.Core.Usuarios.Entities;
global using RestaurantePro.Domain.Core.Usuarios.Enums;
global using RestaurantePro.Domain.Core.Usuarios.Events;
global using RestaurantePro.Domain.Core.Usuarios.Events.Usuario;
global using RestaurantePro.Domain.Core.Usuarios.Events.Rol;
global using RestaurantePro.Domain.Core.Usuarios.Interfaces;
global using RestaurantePro.Domain.Core.Usuarios.Services;
global using RestaurantePro.Domain.Core.Usuarios.EventHandlers;

// Domain core imports - Productos
global using RestaurantePro.Domain.Core.Productos.Entities;
global using RestaurantePro.Domain.Core.Productos.ValueObjects;
global using RestaurantePro.Domain.Core.Productos.Interfaces;
global using RestaurantePro.Domain.Core.Productos.Events;
global using RestaurantePro.Domain.Core.Productos.Events.Producto;
global using RestaurantePro.Domain.Core.Productos.Events.ProductoCategoria;
global using RestaurantePro.Domain.Core.Productos.Services;
global using RestaurantePro.Domain.Core.Productos.Specifications;
global using RestaurantePro.Domain.Core.Productos.Policies;

// Domain Operaciones - Comandas
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;
global using RestaurantePro.Domain.Operaciones.Comandas.Enums;
global using RestaurantePro.Domain.Operaciones.Comandas.Events;
global using RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda;
global using RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda;
global using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;

// Domain Operaciones - Reservaciones
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
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

// Domain Comercial - Promociones
global using RestaurantePro.Domain.Comercial.Promociones.Entities;
global using RestaurantePro.Domain.Comercial.Promociones.Enums;
global using RestaurantePro.Domain.Comercial.Promociones.Events;
global using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
global using RestaurantePro.Domain.Comercial.Promociones.Specifications;

// Domain Comercial - Facturacion
global using RestaurantePro.Domain.Comercial.Facturacion.Entities;
global using RestaurantePro.Domain.Comercial.Facturacion.Enums;
global using RestaurantePro.Domain.Comercial.Facturacion.EventHandlers;
global using RestaurantePro.Domain.Comercial.Facturacion.Events;
global using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
global using RestaurantePro.Domain.Comercial.Facturacion.Services;
global using RestaurantePro.Domain.Comercial.Facturacion.Specifications;

// Domain Comercial - Pagos
global using RestaurantePro.Domain.Comercial.Pagos.Entities;
global using RestaurantePro.Domain.Comercial.Pagos.Enums;
global using RestaurantePro.Domain.Comercial.Pagos.Events;
global using RestaurantePro.Domain.Comercial.Pagos.Events.Pago;
global using RestaurantePro.Domain.Comercial.Pagos.Interfaces;

// Domain Comercial - Services
global using RestaurantePro.Domain.Comercial.Services;

// Domain Comercial - Policies
global using RestaurantePro.Domain.Comercial.Policies;

// Domain Inventario - Ingredientes
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
global using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
global using RestaurantePro.Domain.Inventario.Ingredientes.Events;
global using RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente;

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
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events.OrdenCompra;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events.ItemOrdenCompra;

// Domain Inventario - Services
global using RestaurantePro.Domain.Inventario.Services;

// Domain Inventario - Policies
global using RestaurantePro.Domain.Inventario.Policies;

// Domain Inventario - Results
global using RestaurantePro.Domain.Inventario.Results;

// Domain Proveedores
global using RestaurantePro.Domain.Proveedores.Entities;
global using RestaurantePro.Domain.Proveedores.Enums;
global using RestaurantePro.Domain.Proveedores.Interfaces;
global using RestaurantePro.Domain.Proveedores.Events;
global using RestaurantePro.Domain.Proveedores.Events.Proveedor;
global using RestaurantePro.Domain.Proveedores.Events.ContactoProveedor;
global using RestaurantePro.Domain.Proveedores.Results;
global using RestaurantePro.Domain.Proveedores.ValueObjects;
global using RestaurantePro.Domain.Proveedores.Specifications;

// Event Handlers
global using RestaurantePro.Domain.Comercial.EventHandlers;
global using RestaurantePro.Domain.Inventario.EventHandlers;
global using RestaurantePro.Domain.Operaciones.EventHandlers;

// Services
global using RestaurantePro.Domain.Operaciones.Services;
