#nullable enable

// Test Frameworks and tools
global using Xunit;
global using FluentAssertions;
global using Moq;
global using Moq.Language;
global using Moq.Language.Flow;
global using System;
global using System.Diagnostics;
global using System.Collections.Generic;
global using System.Linq;
global using System.Reflection;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Text.RegularExpressions;
global using System.Globalization;
global using System.Diagnostics.CodeAnalysis;
global using System.Collections.ObjectModel;

// Alias para Error
global using Error = RestaurantePro.Domain.Core.SharedKernel.Validation.Error;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;

// Microsoft
global using Microsoft.Extensions.Logging;

// Domain core imports - Base
global using RestaurantePro.Domain.Core.Base;
global using RestaurantePro.Domain.Core.Base.Interfaces;
global using RestaurantePro.Domain.Core.Base.Services;

// Domain Base - Events
global using RestaurantePro.Domain.Core.Base.Events;
global using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
global using RestaurantePro.Domain.Core.Base.Events.Handlers;
global using RestaurantePro.Domain.Core.Base.Events.Registry;
global using RestaurantePro.Domain.Core.Base.Events.Subscription;
global using RestaurantePro.Domain.Core.Base.Events.Extensions;

// Domain core imports - Core
global using RestaurantePro.Domain.Core;
global using RestaurantePro.Domain.Core.Services;

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

// Domain core imports - SharedKernel Services - Notification
global using RestaurantePro.Domain.Core.SharedKernel.Services.Notification;

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
global using RestaurantePro.Domain.Core.Usuarios.Interfaces;
global using RestaurantePro.Domain.Core.Usuarios.EventHandlers;
global using RestaurantePro.Domain.Core.Usuarios.Services;

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
global using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda;
global using RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda;
global using RestaurantePro.Domain.Operaciones.Comandas.Builders;

// Domain Operaciones - Reservaciones
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Events;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Builders;

// Domain Operaciones - Preparaciones
global using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Events;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Services;

// Domain Comercial - Clientes
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
global using RestaurantePro.Domain.Comercial.Clientes.Enums;
global using RestaurantePro.Domain.Comercial.Clientes.Events;
global using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
global using RestaurantePro.Domain.Comercial.Clientes.Events.Cliente;
global using RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion;
global using RestaurantePro.Domain.Comercial.Clientes.Factories;

// Domain Comercial - Promociones
global using RestaurantePro.Domain.Comercial.Promociones.Entities;
global using RestaurantePro.Domain.Comercial.Promociones.Enums;
global using RestaurantePro.Domain.Comercial.Promociones.Events;
global using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
global using RestaurantePro.Domain.Comercial.Promociones.Services;
global using RestaurantePro.Domain.Comercial.Promociones.Specifications;

// Domain Comercial - Facturacion
global using RestaurantePro.Domain.Comercial.Facturacion.Entities;
global using RestaurantePro.Domain.Comercial.Facturacion.Enums;
global using RestaurantePro.Domain.Comercial.Facturacion.Events;
global using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
global using RestaurantePro.Domain.Comercial.Facturacion.Services;
global using RestaurantePro.Domain.Comercial.Facturacion.Builders;

// Domain Comercial - Services
global using RestaurantePro.Domain.Comercial.Services;

// Domain Inventario - Ingredientes
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
global using RestaurantePro.Domain.Inventario.Ingredientes.Events;
global using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;

// Domain Inventario - Ingredientes - Movimientos
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Events;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;

// Domain Inventario - Compras - OrdenesCompra
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events.OrdenCompra;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;

// Domain Inventario - Services
global using RestaurantePro.Domain.Inventario.Services;

// Domain Inventario - Policies
global using RestaurantePro.Domain.Inventario.Policies;

// Domain Inventario - Results
global using RestaurantePro.Domain.Inventario.Results;

// Domain Comercial - Policies
global using RestaurantePro.Domain.Comercial.Policies;

// Domain Proveedores
global using RestaurantePro.Domain.Proveedores.Entities;
global using RestaurantePro.Domain.Proveedores.Interfaces;
global using RestaurantePro.Domain.Proveedores.Events;
global using RestaurantePro.Domain.Proveedores.Events.Proveedor;
global using RestaurantePro.Domain.Proveedores.Events.ContactoProveedor;
global using RestaurantePro.Domain.Proveedores.Enums;
global using RestaurantePro.Domain.Proveedores.ValueObjects;
global using RestaurantePro.Domain.Proveedores.Specifications;

// Event Handlers
global using RestaurantePro.Domain.Comercial.EventHandlers;
global using RestaurantePro.Domain.Inventario.EventHandlers;
global using RestaurantePro.Domain.Operaciones.EventHandlers;

// Additional Services
global using RestaurantePro.Domain.Operaciones.Services;

// Eliminando importaciones duplicadas de Productos y Recetas que generan advertencias

// Domain core imports - SharedKernel Exceptions
global using RestaurantePro.Domain.Core.SharedKernel.Exceptions;

// Domain core imports - SharedKernel Factories  
global using RestaurantePro.Domain.Core.SharedKernel.Factories;

// Domain core imports - SharedKernel Guards
global using RestaurantePro.Domain.Core.SharedKernel.Guards;

// Domain Comercial - Clientes - Exceptions
global using RestaurantePro.Domain.Comercial.Clientes.Exceptions;

// Domain Inventario - Ingredientes - Exceptions
global using RestaurantePro.Domain.Inventario.Ingredientes.Exceptions;

// Domain Inventario - Compras - Builders
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Builders;
global using RestaurantePro.Domain.Inventario.Ingredientes.Builders;

// Domain Core - Productos - Builders
global using RestaurantePro.Domain.Core.Productos.Builders;

// Domain Proveedores - Builders
global using RestaurantePro.Domain.Proveedores.Builders;

// Domain Operaciones - Reservaciones Mesas - Builders
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Builders;

// Domain Inventario - Ingredientes - Factories
global using RestaurantePro.Domain.Inventario.Ingredientes.Factories;

// Domain Operaciones - Comandas Exceptions
global using RestaurantePro.Domain.Operaciones.Comandas.Exceptions;

// Domain Operaciones - Reservaciones Exceptions
global using RestaurantePro.Domain.Operaciones.Reservaciones.Exceptions;

// Domain Operaciones - Reservaciones Mesas Exceptions
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Exceptions;

// Domain Proveedores - Exceptions
global using RestaurantePro.Domain.Proveedores.Exceptions;

// Testing Frameworks
global using Xunit;
global using FluentAssertions;
global using Moq;

// .NET Base
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Reflection;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

// MediatR
global using MediatR;

// Domain - Core SharedKernel
global using RestaurantePro.Domain.Core.SharedKernel.Results;
global using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
global using RestaurantePro.Domain.Core.SharedKernel.Validation;
global using RestaurantePro.Domain.Core.SharedKernel.Exceptions;
global using RestaurantePro.Domain.Core.SharedKernel.Enums;

// Domain - Core Notificaciones
global using RestaurantePro.Domain.Core.Notificaciones.Services;
global using RestaurantePro.Domain.Core.Notificaciones.Enums;
global using RestaurantePro.Domain.Core.Notificaciones.Entities;

// Domain - Core Productos
global using RestaurantePro.Domain.Core.Productos.Entities;
global using RestaurantePro.Domain.Core.Productos.ValueObjects;
global using RestaurantePro.Domain.Core.Productos.Interfaces;
global using RestaurantePro.Domain.Core.Productos.Builders;
global using RestaurantePro.Domain.Core.Productos.Services;

// Domain - Core Usuarios
global using RestaurantePro.Domain.Core.Usuarios.Entities;

// Domain - Proveedores
global using RestaurantePro.Domain.Proveedores.Entities;
global using RestaurantePro.Domain.Proveedores.Interfaces;
global using RestaurantePro.Domain.Proveedores.Enums;
global using RestaurantePro.Domain.Proveedores.Builders;

// Domain - Operaciones
global using RestaurantePro.Domain.Operaciones.Services;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Comandas.Enums;
global using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;

// Domain - Inventario
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
global using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
global using RestaurantePro.Domain.Inventario.Services;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;

// Domain - Comercial - Clientes
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
global using RestaurantePro.Domain.Comercial.Clientes.Enums;
global using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
global using RestaurantePro.Domain.Comercial.Clientes.Builders;

// Domain - Comercial - Servicios
global using RestaurantePro.Domain.Comercial.Services;

// Domain - Comercial - Promociones
global using RestaurantePro.Domain.Comercial.Promociones.Services;

// Domain - Comercial - Facturación
global using RestaurantePro.Domain.Comercial.Facturacion.Entities;
global using RestaurantePro.Domain.Comercial.Facturacion.Enums;
global using RestaurantePro.Domain.Comercial.Facturacion.Services;
global using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;

