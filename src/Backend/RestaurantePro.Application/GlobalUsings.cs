// .NET Base
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Reflection;
global using System.Linq.Expressions;
global using System.Text.Json;
global using System.Text.RegularExpressions;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Storage;

// MediatR para Vertical Slices
global using MediatR;

// AutoMapper para mapeo DTOs
global using AutoMapper;

// FluentValidation para validaciones
global using FluentValidation;
global using FluentValidation.Results;

// Domain - SharedKernel (Según el Domain actual)
global using RestaurantePro.Domain.Core.SharedKernel.Results;
global using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
global using RestaurantePro.Domain.Core.SharedKernel.Exceptions;

// Application - Common (DTOs compartidos y behaviors) - PRIMERO para priorizar sobre Domain
global using RestaurantePro.Application.Common.DTOs;
global using RestaurantePro.Application.Common.Behaviors;
global using RestaurantePro.Application.Common.Exceptions;
global using RestaurantePro.Application.Common.Interfaces;
global using RestaurantePro.Application.Common.Enums;

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
global using RestaurantePro.Domain.Comercial.Clientes.Builders;
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
global using RestaurantePro.Domain.Comercial.Clientes.Enums;
global using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
global using RestaurantePro.Domain.Comercial.Facturacion.Entities;
global using RestaurantePro.Domain.Comercial.Facturacion.Enums;
global using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
global using RestaurantePro.Domain.Comercial.Facturacion.Services;
global using RestaurantePro.Domain.Comercial.Pagos.Entities;
global using RestaurantePro.Domain.Comercial.Promociones.Entities;
global using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
global using RestaurantePro.Domain.Comercial.Services;

// Domain - Operaciones
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.Enums;
global using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;
global using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Services;

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
global using RestaurantePro.Domain.Inventario.Services;

// Domain - Proveedores (Estructura real)
global using RestaurantePro.Domain.Proveedores.Entities;
global using RestaurantePro.Domain.Proveedores.Interfaces;
global using RestaurantePro.Domain.Proveedores.Enums;
global using RestaurantePro.Domain.Proveedores.ValueObjects;
global using RestaurantePro.Domain.Proveedores.Builders;

// Domain - Events
global using RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion;

// Application - Core DTOs
global using RestaurantePro.Application.Core.Productos.DTOs;
global using RestaurantePro.Application.Core.Usuarios.DTOs;

// Application - Comercial DTOs
global using RestaurantePro.Application.Comercial.Clientes.DTOs;
global using RestaurantePro.Application.Comercial.Facturacion.DTOs;
global using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

// Application - Operaciones DTOs
global using RestaurantePro.Application.Operaciones.Comandas.DTOs;
global using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
global using RestaurantePro.Application.Operaciones.Mesas.DTOs;
global using RestaurantePro.Application.Operaciones.Reportes.DTOs;

// Application - Inventario DTOs
global using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
global using RestaurantePro.Application.Inventario.Reportes.DTOs;

// Application - Proveedores DTOs  
global using RestaurantePro.Application.Proveedores.Proveedores.DTOs;

// Application - Reportes DTOs
global using RestaurantePro.Application.Comercial.Reportes.DTOs;

// Application - Commands (Core)
global using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
global using RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;

// Application - Commands (Comercial)
global using RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;
global using RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;
global using RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;
global using RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos;

// Application - Commands (Operaciones)
global using RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarMesa;
global using RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;
global using RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;

// Application - Commands (Inventario)
global using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
global using RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarStock;

// Application - Commands (Proveedores)
global using RestaurantePro.Application.Proveedores.Proveedores.Commands.CrearProveedor;

// Application - Queries
global using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientePorId;
global using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesPaginados;
global using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesBajoStock;
global using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;
global using RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerAnalisisFidelizacion;
global using RestaurantePro.Application.Operaciones.Reportes.Queries.ObtenerReporteVentasDiaria;
global using RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedoresPaginados;

// Application - Event Handlers
global using RestaurantePro.Application.Operaciones.Reservaciones.EventHandlers.ReservacionCreada;

// Application - Configuration (AutoMapper y DI)
global using RestaurantePro.Application.Config.Mappings;
global using RestaurantePro.Application.Config.DependencyInjection;
global using RestaurantePro.Application.Config.Settings;
