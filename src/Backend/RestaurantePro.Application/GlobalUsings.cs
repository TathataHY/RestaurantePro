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
global using Microsoft.EntityFrameworkCore;

// MediatR para Vertical Slices
global using MediatR;

// AutoMapper para mapeo DTOs
global using AutoMapper;

// FluentValidation para validaciones
global using FluentValidation;

// 🔥 System.Text.Json para serialización 
global using System.Text.Json;

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

// Domain - Comercial
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
global using RestaurantePro.Domain.Comercial.Clientes.Enums;
global using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
global using RestaurantePro.Domain.Comercial.Facturacion.Entities;
global using RestaurantePro.Domain.Comercial.Facturacion.Enums;
global using RestaurantePro.Domain.Comercial.Pagos.Entities;
global using RestaurantePro.Domain.Comercial.Promociones.Entities;

// Domain - Operaciones
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.Enums;
global using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;

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
global using RestaurantePro.Application.Common.Enums;

// Application - Core DTOs (Por contexto)
global using RestaurantePro.Application.Core.Productos.DTOs;
global using RestaurantePro.Application.Core.Usuarios.DTOs;

// Application - Comercial DTOs (usando nombres correctos que existen)
global using RestaurantePro.Application.Comercial.Clientes.DTOs;
global using RestaurantePro.Application.Comercial.Facturacion.DTOs;
// TODO: Arreglar namespace de Fidelizacion cuando sea necesario
// global using RestaurantePro.Application.Comercial.Fidelizacion_TEMP.DTOs;

// Application - Operaciones DTOs
global using RestaurantePro.Application.Operaciones.Comandas.DTOs;
global using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

// Application - Operaciones Commands (para EventHandlers)
global using RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;

// Application - Proveedores DTOs  
global using RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs;

// Application - Configuration (AutoMapper y DI)
global using RestaurantePro.Application.Config.Mappings;
global using RestaurantePro.Application.Config.DependencyInjection;

// 🔥 NUEVO: Domain Services para EventHandlers
global using RestaurantePro.Domain.Comercial.Services;

// 🚀 NUEVO: Domain Service Facades para Commands Avanzados  
global using RestaurantePro.Domain.Operaciones.Services;

// 🔧 INTERFACES DE REPOSITORIES (para Commands y EventHandlers)
global using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
global using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

// 🆕 NUEVO: ISignalRService y IBackgroundJobService ya incluidos en IApplicationService
// Las interfaces están en Common.Interfaces - no necesitan using adicional
