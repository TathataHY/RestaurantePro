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

// MediatR para Vertical Slices
global using MediatR;

// AutoMapper para mapeo DTOs
global using AutoMapper;

// FluentValidation para validaciones
global using FluentValidation;

// Domain - SharedKernel (Según el Domain actual)
global using RestaurantePro.Domain.Core.SharedKernel.Results;
global using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
global using RestaurantePro.Domain.Core.SharedKernel.Exceptions;

// Domain - Core (Según estructura real)
global using RestaurantePro.Domain.Core.Productos.Entities;
global using RestaurantePro.Domain.Core.Productos.ValueObjects;
global using RestaurantePro.Domain.Core.Productos.Interfaces;
global using RestaurantePro.Domain.Core.Productos.Services;
global using RestaurantePro.Domain.Core.Productos.Builders;

global using RestaurantePro.Domain.Core.Usuarios.Entities;
global using RestaurantePro.Domain.Core.Usuarios.Enums;
global using RestaurantePro.Domain.Core.Usuarios.Interfaces;
global using RestaurantePro.Domain.Core.Usuarios.Services;

global using RestaurantePro.Domain.Core.Notificaciones.Entities;
global using RestaurantePro.Domain.Core.Notificaciones.Enums;
global using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
global using RestaurantePro.Domain.Core.Notificaciones.Services;

// Domain - Comercial (Según estructura real)
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
global using RestaurantePro.Domain.Comercial.Clientes.Enums;
global using RestaurantePro.Domain.Comercial.Clientes.Interfaces;

global using RestaurantePro.Domain.Comercial.Facturacion.Entities;
global using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;

// Domain - Operaciones (Según estructura real)
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;
global using RestaurantePro.Domain.Operaciones.Comandas.Enums;
global using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;

global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;

global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;

global using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Services;

// Domain - Inventario (Según estructura real)
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
global using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;

global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;

global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;

// Domain - Proveedores (Estructura real)
global using RestaurantePro.Domain.Proveedores.Entities;
global using RestaurantePro.Domain.Proveedores.Interfaces;
global using RestaurantePro.Domain.Proveedores.Enums;
global using RestaurantePro.Domain.Proveedores.ValueObjects;
global using RestaurantePro.Domain.Proveedores.Builders;

// Application - Common (DTOs compartidos y behaviors)
global using RestaurantePro.Application.Common.DTOs;
global using RestaurantePro.Application.Common.Behaviors;
global using RestaurantePro.Application.Common.Exceptions;
global using RestaurantePro.Application.Common.Interfaces;

// Application - Core DTOs (Por contexto)
global using RestaurantePro.Application.Core.Productos.DTOs;

// Application - Operaciones DTOs
global using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

// Application - Proveedores DTOs  
global using RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs;

// Application - Configuration (AutoMapper y DI)
global using RestaurantePro.Application.Config.Mappings;
global using RestaurantePro.Application.Config.DependencyInjection; 